using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    private CharacterController characterController;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;         // 이동 속도
    public float rotationSpeed = 15f;   // 회전 속도 (숫자가 클수록 빠르게 회전)
    public float jumpHeight = 2f;       // 점프 높이
    public float gravity = -9.81f;      // 중력

    private Vector3 velocity;           // 중력 및 점프 속도 관리용
    private Vector3 currentMoveDirection; // 현재 이동 방향

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        // InputManager에서 입력 값 가져오기
        var inputManager = GameManager.Instance.GetManager<InputManager>("InputManager");
        if (inputManager == null) return;

        // 입력 값으로 이동 방향 계산
        Vector2 moveInput = inputManager.MoveInput;
        Vector3 targetMoveDirection = new Vector3(moveInput.x, 0, moveInput.y);

        if (targetMoveDirection.magnitude > 0.1f)
        {
            // 방향 벡터 정규화
            targetMoveDirection.Normalize();

            // 이동 방향을 점진적으로 변경 (부드러운 이동)
            currentMoveDirection = Vector3.Lerp(
                currentMoveDirection,   // 현재 이동 방향
                targetMoveDirection,    // 목표 이동 방향
                rotationSpeed * Time.deltaTime // 보간 속도
            );

            // 이동 처리
            characterController.Move(currentMoveDirection * moveSpeed * Time.deltaTime);
        }

        // 중력 및 점프 처리
        if (characterController.isGrounded)
        {
            velocity.y = 0; // 중력 초기화
            if (inputManager.IsJumping)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }
        velocity.y += gravity * Time.deltaTime;

        // 캐릭터에 중력 적용
        characterController.Move(velocity * Time.deltaTime);
    }
}
