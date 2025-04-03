using UnityEngine;

public class WaterDepthBlocker : MonoBehaviour
{
    [SerializeField] private float maxDepth = 2f; // 플레이어가 들어갈 수 있는 최대 수중 깊이
    private float checkDistance = 1.5f;           // 검사 거리
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask waterLayer;
    [SerializeField] private LayerMask groundLayer;

    private PlayerController player;
    private bool playerIsGrounded;

    private void Start()
    {
        player = GameManager.playerTransform.GetComponent<PlayerController>();
    }

    /*
     * 1. 플레이어가 트리거 엔터, 트리거 스테이 하는 동안 검사.
     * 2. 지면에서 걸어들어가는 경우 : 플레이어의 현재 트랜스폼에서 위로 레이캐스트(물의 표면에 콜라이더가 있으니까) 그 거리를 검사해서 최대 수중 깊이보다 크면 플레이어를 마지막 위치로 강제이동
     * 3. 점프에서 빠지는 경우 : 트리거 엔터 하는 순간, 플레이어 발 아래로 레이캐스트, 땅에 닿는 거리를 검사해서 최대 수중 깊이보다 크면.. 어떡하지? 밀어낸다?
    */

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == 3)
        {
            PlayerIsGroundedCheck();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer != 3) return;

        
    }

    private void PlayerIsGroundedCheck()
    {
        playerIsGrounded = player.isGrounded;
    }
}
