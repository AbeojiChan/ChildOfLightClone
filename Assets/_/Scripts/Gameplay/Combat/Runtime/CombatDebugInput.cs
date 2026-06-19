using UnityEngine;

namespace Combat.Runtime
{
    public class CombatDebugInput : MonoBehaviour
    {
        #region Publics
        [Header("Dependencies")]
        [SerializeField] private CombatManager m_CombatManager;
        [SerializeField] private Combatant m_Hero;
        [SerializeField] private Combatant m_Villain;

        [Header("Test Configuration")]
        [SerializeField] private CombatAction m_AttackAction;
        [SerializeField] private CombatAction m_HealAction;
        #endregion

        #region Unity API
        private void Update()
        {
            if (m_CombatManager == null || m_Hero == null) return;

            if (m_Hero.IsWaitingForInput)
            {
                HandlePlayerSimulation();
            }
        }
        #endregion

        #region Tools and Utilities
        private void HandlePlayerSimulation()
        {

            if (Input.GetKeyDown(KeyCode.A))
            {
                if (m_AttackAction != null && m_Villain != null)
                {
                    Debug.Log($"⌨️ [DEBUG INPUT] Key A pressed: Selecting {m_AttackAction.ActionName} for {m_Hero.CombatantName}.");
                    m_CombatManager.SelectActionForPlayer(m_Hero, m_AttackAction, m_Villain);
                }
            }
            else if (Input.GetKeyDown(KeyCode.Z))
            {
                if (m_HealAction != null)
                {
                    Debug.Log($"⌨️ [DEBUG INPUT] Key Z pressed: Selecting {m_HealAction.ActionName} (Self-Heal) for {m_Hero.CombatantName}.");
                    m_CombatManager.SelectActionForPlayer(m_Hero, m_HealAction, m_Hero);
                }
            }
        }
        #endregion
    }
}