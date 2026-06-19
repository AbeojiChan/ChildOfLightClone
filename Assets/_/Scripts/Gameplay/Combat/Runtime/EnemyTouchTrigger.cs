using UnityEngine;
using UnityEngine.SceneManagement;

namespace Combat.Runtime
{
    [RequireComponent(typeof(Collider2D))]
    public class EnemyTouchTrigger : MonoBehaviour
    {
        #region Publics
        [Header("Scene Transition")]
        [SerializeField] private string m_CombatSceneName = "02_CombatArena";
        #endregion

        #region Unity API
        private void Awake()
        {
            m_Collider = GetComponent<Collider2D>();

            // Sécurité KJD Convention : On force le collider en mode Trigger par code
            m_Collider.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // On vérifie si c'est bien le joueur qui entre en contact
            if (other.CompareTag("Player"))
            {
                TriggerCombatTransition();
            }
        }
        #endregion

        #region Tools and Utilities
        private void TriggerCombatTransition()
        {
            Debug.Log($"⚔️ [OVERWORLD] Enemy touched! Initiating transition to scene: {m_CombatSceneName}");

            // Pour le prototype, on charge directement la scène d'arène.
            // (Plus tard dans l'architecture, on pourra passer par un GameManager persistant pour sauvegarder les positions)
            SceneManager.LoadScene(m_CombatSceneName);
        }
        #endregion

        #region Private and Protected
        private Collider2D m_Collider;
        #endregion
    }
}