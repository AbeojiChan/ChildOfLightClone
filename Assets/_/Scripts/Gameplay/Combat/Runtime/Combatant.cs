using UnityEngine;

namespace Combat.Runtime
{
    public class Combatant : MonoBehaviour
    {
        #region Publics
        [Header("Identity & Base Stats")]
        [SerializeField] private string m_CombatantName;
        [SerializeField] private int m_MaxHealth = 100;
        [SerializeField] private int m_Health = 100;
        [SerializeField] private float m_Speed = 10f;
        [SerializeField] private bool m_IsPlayerTeam;

        public string CombatantName => m_CombatantName;
        public int MaxHealth => m_MaxHealth;
        public int Health { get => m_Health; set => m_Health = value; }
        public float Speed => m_Speed;
        public bool IsPlayerTeam => m_IsPlayerTeam;

        [Header("Runtime State")]
        public float TimelinePosition;
        public bool IsCasting;
        public float CastTimer;
        public CombatAction CurrentAction;
        public Combatant Target;

        public bool IsWaitingForInput;
        #endregion

        #region Main API
        public bool IsAlive()
        {
            return m_Health > 0;
        }

        public void ResetTimeline()
        {
            TimelinePosition = 0f;
            IsCasting = false;
            CastTimer = 0f;
            CurrentAction = null;
            Target = null;
            IsWaitingForInput = false;
        }
        #endregion
    }
}