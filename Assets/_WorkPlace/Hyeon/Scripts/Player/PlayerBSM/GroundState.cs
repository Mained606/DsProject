using UnityEngine;
using System;

namespace JWSTEST
{
    public class GroundState : MonoBehaviour
    {
        [Header("✅ 그라운드 체크 설정")]
        public CharacterController characterController;
        public LayerMask groundLayer;
        public float groundCheckDistance = 0.1f;
        public float sphereRadius = 0.3f;
        public float checkInterval = 0.05f;

        [Header("✅ 경사면 감지설정")]
        public float maxSlopeAngle = 45f;

        private bool isGrounded;
        private bool isOnSlope;
        private Vector3 groundNormal;
        private float nextCheckTime = 0f;

        public event Action<bool> OnGroundedChanged;

        void Update()
        {
            if (Time.time >= nextCheckTime)
            {
                bool prevGrounded = isGrounded;
                isGrounded = CheckGrounded();
                isOnSlope = CheckOnSlope();

                if (prevGrounded != isGrounded)
                    OnGroundedChanged?.Invoke(isGrounded);

                nextCheckTime = Time.time + checkInterval;
            }
        }

        private bool CheckGrounded()
        {
            if (characterController.isGrounded) return true;

            RaycastHit hit;
            Vector3 origin = transform.position + Vector3.up * 0.1f;

            if (Physics.SphereCast(origin, sphereRadius, Vector3.down, out hit, characterController.height / 2 + groundCheckDistance, groundLayer))
                return true;

            return false;
        }

        private bool CheckOnSlope()
        {
            RaycastHit hit;
            Vector3 origin = transform.position + Vector3.up * 0.1f;
            if (Physics.Raycast(origin, Vector3.down, out hit, characterController.height / 2 + groundCheckDistance, groundLayer))
            {
                groundNormal = hit.normal;
                float slopeAngle = Vector3.Angle(Vector3.up, groundNormal);
                return slopeAngle > 0 && slopeAngle <= maxSlopeAngle;
            }
            return false;
        }

        public bool IsGrounded() => isGrounded;

        public bool IsOnSlope() => isOnSlope;

        public Vector3 GetGroundNormal() => groundNormal;
    }
}