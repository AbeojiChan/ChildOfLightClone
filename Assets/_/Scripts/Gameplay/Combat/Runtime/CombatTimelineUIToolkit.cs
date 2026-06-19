using UnityEngine;
using UnityEngine.UIElements;

namespace Combat.Runtime
{
    [RequireComponent(typeof(UIDocument))]
    public class CombatTimelineUIToolkit : MonoBehaviour
    {
        #region Publics
        [Header("Dependencies")]
        [SerializeField] private CombatManager m_CombatManager;

        [Header("Timeline Settings")]
        [SerializeField] private float m_TimelineEnd = 100f;

        [Header("Sprites for Tokens (Tiny Swords)")]
        [SerializeField] private Sprite m_HeroIcon;
        [SerializeField] private Sprite m_VillainIcon;
        #endregion

        #region Unity API
        private void OnEnable()
        {
            m_UiDocument = GetComponent<UIDocument>();

            if (m_UiDocument != null && m_CombatManager != null)
            {
                CacheUIElements();
                InitializeTimeline();
            }
        }

        private void LateUpdate()
        {
            UpdateTokenPositions();
            HandleActionMenuMenuLogic();
        }
        #endregion

        #region Tools and Utilities
        private void CacheUIElements()
        {
            var root = m_UiDocument.rootVisualElement;

            // Extraction des éléments clés de notre UXML
            m_TimelineTrack = root.Q<VisualElement>("TimelineTrack");
            m_ActionMenuPanel = root.Q<VisualElement>("ActionMenuPanel");
            m_MenuTitle = root.Q<Label>("MenuTitle");
            m_ActionButtonsContainer = root.Q<VisualElement>("ActionButtonsContainer");
            m_TargetButtonsContainer = root.Q<VisualElement>("TargetButtonsContainer");
        }

        private void InitializeTimeline()
        {
            if (m_TimelineTrack == null) return;

            Combatant[] allCombatants = m_CombatManager.GetComponentsInChildren<Combatant>();

            m_TrackedCombatants = new Combatant[allCombatants.Length];
            m_TokenElements = new VisualElement[allCombatants.Length];

            for (int i = 0; i < allCombatants.Length; i++)
            {
                Combatant combatant = allCombatants[i];

                VisualElement token = new VisualElement();
                token.style.position = Position.Absolute;
                token.style.width = 40;
                token.style.height = 40;
                token.style.top = -10;
                token.style.translate = new Translate(new Length(-50, LengthUnit.Percent), 0);

                Sprite assignedSprite = combatant.IsPlayerTeam ? m_HeroIcon : m_VillainIcon;
                if (assignedSprite != null)
                {
                    token.style.backgroundImage = new StyleBackground(assignedSprite);
                }

                m_TimelineTrack.Add(token);

                m_TrackedCombatants[i] = combatant;
                m_TokenElements[i] = token;
            }
        }

        private void UpdateTokenPositions()
        {
            if (m_TrackedCombatants == null || m_TimelineTrack == null) return;

            float trackWidth = m_TimelineTrack.layout.width;
            if (float.IsNaN(trackWidth) || trackWidth <= 0) return;

            for (int i = 0; i < m_TrackedCombatants.Length; i++)
            {
                Combatant combatant = m_TrackedCombatants[i];
                VisualElement token = m_TokenElements[i];

                if (combatant == null || token == null) continue;

                if (!combatant.IsAlive())
                {
                    token.style.display = DisplayStyle.None;
                    continue;
                }

                float ratio = combatant.TimelinePosition / m_TimelineEnd;
                float targetXPixels = ratio * trackWidth;
                token.style.left = targetXPixels;
            }
        }

        private void HandleActionMenuMenuLogic()
        {
            if (m_CombatManager == null || m_ActionMenuPanel == null) return;

            // 1. Recherche du premier héros vivant en attente d'input (sans LINQ)
            Combatant activeHero = null;
            for (int i = 0; i < m_TrackedCombatants.Length; i++)
            {
                if (m_TrackedCombatants[i] != null &&
                    m_TrackedCombatants[i].IsPlayerTeam &&
                    m_TrackedCombatants[i].IsAlive() &&
                    m_TrackedCombatants[i].IsWaitingForInput)
                {
                    activeHero = m_TrackedCombatants[i];
                    break;
                }
            }

            // 2. Gestion de l'affichage du panneau
            if (activeHero != null)
            {
                // Si le panneau n'est pas encore affiché pour ce héros précis, on le construit
                if (m_CurrentMenuOwner != activeHero)
                {
                    m_CurrentMenuOwner = activeHero;
                    BuildMenuForHero(activeHero);
                }
            }
            else
            {
                // Plus personne n'attend, on cache le menu
                if (m_ActionMenuPanel.style.display != DisplayStyle.None)
                {
                    m_ActionMenuPanel.style.display = DisplayStyle.None;
                    m_CurrentMenuOwner = null;
                    ClearSelectionState();
                }
            }
        }

        private void BuildMenuForHero(Combatant hero)
        {
            if (m_MenuTitle != null) m_MenuTitle.text = $"{hero.CombatantName.ToUpper()} TURN";

            // Nettoyage des anciens boutons
            m_ActionButtonsContainer.Clear();
            m_TargetButtonsContainer.Clear();
            ClearSelectionState();

            // Récupération du pool d'actions du Manager (via notre KJD convention d'assignation)
            // Pour le prototype, on va directement interroger les actions de test configurées sur le CombatManager
            CombatAction[] availableActions = m_CombatManager.GetTestActionsPool();

            // 1. Génération des boutons d'actions
            for (int i = 0; i < availableActions.Length; i++)
            {
                CombatAction action = availableActions[i];
                if (action == null) continue;

                Button actionBtn = new Button();
                actionBtn.text = $"{action.ActionName} ({action.CastTime}s)";
                actionBtn.style.marginRight = 5;
                actionBtn.style.marginBottom = 5;

                // Capture de la variable locale pour le délégué
                CombatAction targetAction = action;
                actionBtn.clicked += () => OnActionSelected(targetAction);

                m_ActionButtonsContainer.Add(actionBtn);
            }

            // On affiche enfin le menu complet
            m_ActionMenuPanel.style.display = DisplayStyle.Flex;
        }

        private void OnActionSelected(CombatAction action)
        {
            m_SelectedAction = action;
            Debug.Log($"🎨 [UI SELECTION] Selected Action: {action.ActionName}. Now select a target.");

            // Une fois l'action choisie, on génère la liste des cibles valides (les ennemis vivants)
            BuildTargetButtons();
        }

        private void BuildTargetButtons()
        {
            m_TargetButtonsContainer.Clear();

            for (int i = 0; i < m_TrackedCombatants.Length; i++)
            {
                Combatant potentialTarget = m_TrackedCombatants[i];
                if (potentialTarget == null || !potentialTarget.IsAlive()) continue;

                // Suivant la nature de l'action (Soin vs Attaque), on pourrait filtrer. 
                // Pour notre prototype simple : Bouton pour tout le monde.
                Button targetBtn = new Button();
                targetBtn.text = potentialTarget.CombatantName;
                targetBtn.style.marginRight = 5;
                targetBtn.style.marginBottom = 5;

                Combatant targetCombatant = potentialTarget;
                targetBtn.clicked += () => OnTargetSelected(targetCombatant);

                m_TargetButtonsContainer.Add(targetBtn);
            }
        }

        private void OnTargetSelected(Combatant target)
        {
            if (m_CurrentMenuOwner == null || m_SelectedAction == null || target == null) return;

            Debug.Log($"🎨 [UI SUBMIT] Sending choices for {m_CurrentMenuOwner.CombatantName} to Manager.");

            // Envoi de la commande finale validée par l'UI au manager
            m_CombatManager.SelectActionForPlayer(m_CurrentMenuOwner, m_SelectedAction, target);
        }

        private void ClearSelectionState()
        {
            m_SelectedAction = null;
        }
        #endregion

        #region Private and Protected
        private UIDocument m_UiDocument;
        private VisualElement m_TimelineTrack;
        private VisualElement m_ActionMenuPanel;
        private Label m_MenuTitle;
        private VisualElement m_ActionButtonsContainer;
        private VisualElement m_TargetButtonsContainer;

        private Combatant[] m_TrackedCombatants;
        private VisualElement[] m_TokenElements;

        private Combatant m_CurrentMenuOwner;
        private CombatAction m_SelectedAction;
        #endregion
    }
}