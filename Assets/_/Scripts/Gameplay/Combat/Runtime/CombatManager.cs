using UnityEngine;

namespace Combat.Runtime
{
    public class CombatManager : MonoBehaviour
    {
        #region Publics
        [Header("Timeline Settings")]
        [SerializeField] private float m_CastZoneStart = 80f;
        [SerializeField] private float m_TimelineEnd = 100f;

        [Header("Available Actions Pool")]
        [SerializeField] private CombatAction[] m_TestActions;
        #endregion

        #region Unity API
        private void Start()
        {
            CacheTeams();
        }

        private void Update()
        {
            ProcessTimelineDrivers();
        }
        #endregion

        #region Main API
        /// <summary>
        /// Méthode publique à appeler depuis l'UI ou un gestionnaire d'input pour valider le choix du joueur.
        /// </summary>
        public void SelectActionForPlayer(Combatant player, CombatAction selectedAction, Combatant selectedTarget)
        {
            if (player == null || !player.IsAlive() || !player.IsWaitingForInput) return;

            player.IsWaitingForInput = false;
            player.IsCasting = true;
            player.CastTimer = 0f;
            player.CurrentAction = selectedAction;
            player.Target = selectedTarget;

            Debug.Log($"🎯 [PLAYER INPUT] {player.CombatantName} selected '{selectedAction.ActionName}' targeting {selectedTarget.CombatantName}. Entering cast zone.");
        }

        /// <summary>
        /// Applique l'interruption directe sur un combattant cible s'il est en train de caster.
        /// </summary>
        public void InterruptCombatant(Combatant target, float penalty)
        {
            if (target == null || !target.IsCasting) return;

            Debug.Log($"💥 [INTERRUPT] {target.CombatantName} was interrupted! Pushed back by {penalty} on the timeline.");

            target.IsCasting = false;
            target.CastTimer = 0f;
            target.CurrentAction = null;
            target.Target = null;
            target.IsWaitingForInput = false;

            target.TimelinePosition = Mathf.Max(0f, target.TimelinePosition - penalty);
        }
        #endregion

        #region Tools and Utilities
        private void CacheTeams()
        {
            Combatant[] allCombatants = GetComponentsInChildren<Combatant>();

            int playerCount = 0;
            int enemyCount = 0;

            for (int i = 0; i < allCombatants.Length; i++)
            {
                if (allCombatants[i].IsPlayerTeam) playerCount++;
                else enemyCount++;
            }

            m_PlayerTeam = new Combatant[playerCount];
            m_EnemyTeam = new Combatant[enemyCount];

            int playerIdx = 0;
            int enemyIdx = 0;

            for (int i = 0; i < allCombatants.Length; i++)
            {
                if (allCombatants[i].IsPlayerTeam)
                {
                    m_PlayerTeam[playerIdx] = allCombatants[i];
                    playerIdx++;
                }
                else
                {
                    m_EnemyTeam[enemyIdx] = allCombatants[i];
                    enemyIdx++;
                }
            }
        }

        private void ProcessTimelineDrivers()
        {
            // Traitement de l'équipe joueur
            for (int i = 0; i < m_PlayerTeam.Length; i++)
            {
                if (m_PlayerTeam[i].IsAlive())
                {
                    EvaluateCombatantState(m_PlayerTeam[i]);
                }
            }

            // Traitement de l'équipe ennemie
            for (int i = 0; i < m_EnemyTeam.Length; i++)
            {
                if (m_EnemyTeam[i].IsAlive())
                {
                    EvaluateCombatantState(m_EnemyTeam[i]);
                }
            }
        }

        private void EvaluateCombatantState(Combatant combatant)
        {
            // Si le joueur est bloqué au menu de sélection, sa timeline n'avance plus, mais le reste du monde oui.
            if (combatant.IsWaitingForInput) return;

            // Cas 1 : Progression standard dans la Waiting Zone
            if (!combatant.IsCasting)
            {
                combatant.TimelinePosition += combatant.Speed * Time.deltaTime;

                if (combatant.TimelinePosition >= m_CastZoneStart)
                {
                    combatant.TimelinePosition = m_CastZoneStart;
                    HandleSeuilReached(combatant);
                }
            }
            // Cas 2 : Incantation active dans la Casting Zone
            else
            {
                combatant.CastTimer += Time.deltaTime;

                float ratio = Mathf.Clamp01(combatant.CastTimer / combatant.CurrentAction.CastTime);
                combatant.TimelinePosition = Mathf.Lerp(m_CastZoneStart, m_TimelineEnd, ratio);

                if (combatant.CastTimer >= combatant.CurrentAction.CastTime)
                {
                    ExecuteStoredAction(combatant);
                }
            }
        }

        private void HandleSeuilReached(Combatant combatant)
        {
            if (combatant.IsPlayerTeam)
            {
                // C'est un joueur : on fige sa progression individuelle et on lève le flag pour l'UI
                combatant.IsWaitingForInput = true;
                Debug.Log($"🔔 [WAITING FOR INPUT] {combatant.CombatantName} reached the Cast Zone threshold! Waiting for player selection...");
            }
            else
            {
                // C'est l'IA : exécution immédiate selon tes directives (tirage instantané)
                ExecuteEnemyAI(combatant);
            }
        }

        private void ExecuteEnemyAI(Combatant enemy)
        {
            if (m_TestActions == null || m_TestActions.Length == 0) return;

            enemy.IsCasting = true;
            enemy.CastTimer = 0f;

            // Choix de l'action de l'IA (aléatoire parmi le pool de test)
            int randomActionIdx = Random.Range(0, m_TestActions.Length);
            enemy.CurrentAction = m_TestActions[randomActionIdx];

            // Choix explicite de la cible (un joueur vivant aléatoire)
            enemy.Target = ResolveRandomAliveOpponent(enemy.IsPlayerTeam);

            if (enemy.Target != null)
            {
                Debug.Log($"🤖 [IA CAST] {enemy.CombatantName} instantly selected '{enemy.CurrentAction.ActionName}' targeting {enemy.Target.CombatantName}.");
            }
        }

        private void ExecuteStoredAction(Combatant attacker)
        {
            Combatant target = attacker.Target;

            if (target != null && target.IsAlive())
            {
                Debug.Log($"⚔️ [EXECUTE] {attacker.CombatantName} releases '{attacker.CurrentAction.ActionName}' on {target.CombatantName}!");

                if (attacker.CurrentAction.CanBeInterrupted && target.IsCasting)
                {
                    InterruptCombatant(target, attacker.CurrentAction.InterruptPenalty);
                }

                if (attacker.CurrentAction.IsHeal)
                {
                    target.Health = Mathf.Min(target.MaxHealth, target.Health + attacker.CurrentAction.Damage);
                }
                else
                {
                    target.Health = Mathf.Max(0, target.Health - attacker.CurrentAction.Damage);
                }
            }

            attacker.ResetTimeline();
        }

        private Combatant ResolveRandomAliveOpponent(bool isAttackerPlayerTeam)
        {
            Combatant[] targetsPool = isAttackerPlayerTeam ? m_EnemyTeam : m_PlayerTeam;

            int aliveCount = 0;
            for (int i = 0; i < targetsPool.Length; i++)
            {
                if (targetsPool[i].IsAlive()) aliveCount++;
            }

            if (aliveCount == 0) return null;

            int randomIndex = Random.Range(0, aliveCount);
            int currentAliveScan = 0;

            for (int i = 0; i < targetsPool.Length; i++)
            {
                if (targetsPool[i].IsAlive())
                {
                    if (currentAliveScan == randomIndex)
                    {
                        return targetsPool[i];
                    }
                    currentAliveScan++;
                }
            }

            return null;
        }
        #endregion

        #region Private and Protected
        private Combatant[] m_PlayerTeam;
        private Combatant[] m_EnemyTeam;
        #endregion
    }
}