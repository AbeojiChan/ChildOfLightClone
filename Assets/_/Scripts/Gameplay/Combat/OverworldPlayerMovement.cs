using UnityEngine;

namespace Combat.Runtime
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class OverworldPlayerMovement : MonoBehaviour
    {
        #region Publics
        [Header("Movement Settings")]
        [SerializeField] private float m_MoveSpeed = 5f;
        #endregion

        #region Unity API
        private void Awake()
        {
            m_Rigidbody = GetComponent<Rigidbody2D>();

            // Configuration automatique pour la 2D Top-Down (Sécurité KJD)
            m_Rigidbody.bodyType = RigidbodyType2D.Kinematic;
            m_Rigidbody.simulated = true;
            m_Rigidbody.useFullKinematicContacts = true;
        }

        private void Update()
        {
            HandleInput();
        }

        private void FixedUpdate()
        {
            ProcessMovement();
        }
        #endregion

        #region Tools and Utilities
        private void HandleInput()
        {
            // GetAxisRaw donne un contrôle instantané (0 ou 1), parfait pour le pixel art
            m_MovementInput.x = Input.GetAxisRaw("Horizontal");
            m_MovementInput.y = Input.GetAxisRaw("Vertical");

            // Normalisation pour éviter que le joueur aille plus vite en diagonale
            if (m_MovementInput.sqrMagnitude > 1f)
            {
                m_MovementInput.Normalize();
            }
        }

        private void ProcessMovement()
        {
            // Déplacement physique propre via le Rigidbody
            Vector2 targetPosition = m_Rigidbody.position + m_MovementInput * m_MoveSpeed * Time.fixedDeltaTime;
            m_Rigidbody.MovePosition(targetPosition);
        }
        #endregion

        #region Private and Protected
        private Rigidbody2D m_Rigidbody;
        private Vector2 m_MovementInput;
        #endregion
    }
}