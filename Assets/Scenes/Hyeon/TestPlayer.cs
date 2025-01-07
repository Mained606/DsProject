//using UnityEngine;
//using UnityEngine.InputSystem;

//public class TestPlayer : MonoBehaviour
//{
//    private float moveSpeed = 5.0f;

//    public PlayerInput playerInput;
//    private Vector2 moveInput;
//    private InputAction moveAction => playerInput.actions["Move"];

//    private void Start()
//    {
//        playerInput = GetComponent<PlayerInput>();
//        TestGameManager.Input.keyaction += OnMove;
//    }
//    void OnMove()
//    {
//        moveInput = moveAction.ReadValue<Vector2>();

//        Vector3 newPosition = new Vector3(moveInput.x, 0f, moveInput.y);
//        Vector3 direction = ((newPosition + transform.position) - transform.position).normalized;
//        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 0.2f);
//        transform.position += newPosition * Time.deltaTime * moveSpeed;
//    }

//    private void OnDisable()
//    {
//        TestManager.Input.keyaction -= OnMove;
//    }
//}
