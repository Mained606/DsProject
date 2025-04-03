using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class WaterDepthBlocker : MonoBehaviour
{
    [SerializeField] private float maxDepth = 2.3f; // 플레이어가 들어갈 수 있는 최대 수중 깊이
    [SerializeField] private float currentDepth;                 // 현재 깊이
    private float checkDistance = 5f;           // 깊이 검사 거리
    //private float groundCheckRadius = 10f;       // 주변 땅 검사 반경
    private Vector3 lastSafePosition;           // 플레이어가 마지막으로 서있던 안전한 포지션

    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask waterLayer;
    [SerializeField] private LayerMask groundLayer;

    private PlayerController player;
    private bool playerIsGrounded;

    private float inWaterStateCheckTime = 3f;
    private float timer = 0f;

    private void Start()
    {
        player = GameManager.playerTransform.GetComponent<PlayerController>();
        lastSafePosition = player.transform.position;
    }

    //private void Update()
    //{
    //    if (player.isInWater && timer != 0f)
    //    {
    //        timer = 0f;
    //    }

    //    if(!player.isInWater && timer <= inWaterStateCheckTime)
    //    {
    //        timer += Time.deltaTime;
    //    }
    //    else
    //    {
    //        if(lastSafePosition != Vector3.zero)
    //        {
    //            //lastSafePosition = Vector3.zero;
    //        }
    //    }
    //}

    /*
     * 1. 플레이어가 트리거 엔터, 트리거 스테이 하는 동안 검사.
     * 2. 지면에서 걸어들어가는 경우 : 플레이어의 현재 트랜스폼에서 위로 레이캐스트(물의 표면에 콜라이더가 있으니까) 그 거리를 검사해서 최대 수중 깊이보다 크면 플레이어를 마지막 위치로 강제이동
     * 3. 점프해서 빠지는 경우 : 트리거 엔터 하는 순간, 플레이어 발 아래로 레이캐스트, 땅에 닿는 거리를 검사해서 최대 수중 깊이보다 크면.. 어떡하지? 밀어낸다? 일정거리 주변 땅 탐색(안전한 곳)
     * 4. 그래도 없으면 너무 망망대해인 걸로 간주. 플레이어에게 경각심을 심어주자.
    */

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == 3)
        {
            PlayerIsGroundedCheck();
            player.isInWater = true;
        }
        
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer != 3) return;

        if (playerIsGrounded)
        {
            // 걸어 들어간 경우 검사
            DeepWaterCheckOnWalk();
        }
        else
        {
            // 공중에서 빠지는 경우 검사
            DeepWaterCheckOnAir();
        }

        PlayerIsGroundedCheck();
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.layer == 3)
        {
            //Debug.Log("물 밖으로 나감");
            player.isInWater = false;
        }
    }

    private void PlayerIsGroundedCheck()
    {
        playerIsGrounded = player.isGrounded;
    }

    private void DeepWaterCheckOnWalk()
    {
        if(Physics.Raycast(player.transform.position, Vector3.up, out RaycastHit waterHit, checkDistance, waterLayer))
        {
            currentDepth = Vector3.Distance(player.transform.position, waterHit.point);
            if(currentDepth >= maxDepth)
            {
                // 너무 깊은 물
                Debug.Log("너무 깊은 물입니다. 이동 불가!");

                player.transform.position = lastSafePosition;   // 플레이어를 마지막으로 안전했던 위치에 강제 이동
            }
            else
            {
                lastSafePosition = player.transform.position;   // 현재 플레이어 위치를 마지막으로 안전한 위치로 저장
            }
        }
    }

    private void DeepWaterCheckOnAir()
    {
        if(Physics.Raycast(player.transform.position, Vector3.up, out RaycastHit waterHit, checkDistance, waterLayer))
        {
            if(Physics.Raycast(player.transform.position + Vector3.up * 1f, Vector3.down, out RaycastHit groundHit, checkDistance + 1f, groundLayer))
            {
                currentDepth = Vector3.Distance(waterHit.point, groundHit.point);
                if(currentDepth >= maxDepth)
                {
                    Debug.Log("너무 깊은 물에 뛰어 들었습니다. 이동 불가!");
                    //PlayerPushBack(waterHit);
                    player.transform.position = lastSafePosition;
                }
                //else
                //{
                //    player.transform.position = groundHit.point;
                //}
            }
            else
            {
                Debug.Log("너무 깊은 물에 뛰어 들었습니다. 이동 불가!");
                //PlayerPushBack(waterHit);
                player.transform.position = lastSafePosition;
            }
        }
    }

    private void PlayerPushBack(RaycastHit waterHit)
    {
        if(Physics.Raycast(player.transform.position + Vector3.up * 1f, Vector3.down, out RaycastHit groundHit, checkDistance + 1f, groundLayer))
        {
            float distance = Vector3.Distance(waterHit.point, groundHit.point);
            if(distance > maxDepth)
            {
                player.transform.position -= player.transform.forward * 5f;
                Debug.Log("플레이어 뒤로 미는 중");
            }
        }
        else
        {
            player.transform.position -= player.transform.forward * 5f;
            Debug.Log("플레이어 뒤로 미는 중");
        }
    }
}
