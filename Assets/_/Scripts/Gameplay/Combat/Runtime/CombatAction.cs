using UnityEngine;

namespace Combat.Runtime
{
    [CreateAssetMenu(fileName = "NewCombatAction", menuName = "Combat/Combat Action")]
    public class CombatAction : ScriptableObject
    {
        #region Publics

        [Header("Identity")]
        public string ActionName = "New Action";

        [Header("Effect")]
        [Tooltip("Damage dealt to the target. Ignored if IsHeal is true.")]
        public int Damage = 10;

        [Tooltip("If true, Damage is applied as healing to the target's Health instead of damage.")]
        public bool IsHeal = false;

        [Header("Timing")]
        [Tooltip("Time in seconds spent in the Casting Zone before this action executes.")]
        public float CastTime = 0f;

        [Header("Interruption")]
        [Tooltip("Whether this action can be interrupted by incoming damage while casting.")]
        public bool CanBeInterrupted = true;

        [Tooltip("How far back (in timeline units, 0-100 scale) the target is pushed when interrupted.")]
        public float InterruptPenalty = 20f;

        #endregion

        #region Unity API

        #endregion

        #region Main API

        #endregion
    }
}