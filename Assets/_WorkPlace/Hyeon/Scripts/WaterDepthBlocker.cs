using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class WaterDepthBlocker : MonoBehaviour
{
    [SerializeField] private float maxDepth = 2.3f; // 플레이어가 들어갈 수 있는 최대 수중 깊이
    [SerializeField] private float forwardDepth;                 // 감지 구역 깊이
    [SerializeField] private float currentDepth;                // 플레이어가 서있는 위치의 깊이
    private float checkDistance = 10f;           // 깊이 검사 거리
    [SerializeField] private Vector3 inspectionOrigin;

    [SerializeField] private LayerMask overGroundLayer;
    [SerializeField] private LayerMask groundLayer;

    private PlayerController player;

    [SerializeField] private bool onWater;

    [SerializeField] private float waterHeight;
    [SerializeField] private float groundHeight;
    private bool isGroundRayHit;
    private bool isWaterRayHit;
    private RaycastHit groundHit;
    private RaycastHit waterHit;

    private float drowningCheckTime = 1f;
    private float timer = 0f;

    private void Start()
    {
        player = GameManager.playerTransform.GetComponent<PlayerController>();

        inspectionOrigin = player.transform.position + player.transform.forward * 1f;

        timer = 0f;
    }

    private void Update()
    {
        inspectionOrigin = player.transform.position + player.transform.forward * 0.5f + player.transform.up * 1f;
        Debug.Log($"플레이어 위치 {player.transform.position}");
        if (player.isInWater)
        {
            RealDepthCheck();
            DrowningCheck();
            return;
        }

        UnderTheFeet();
        if (!player.isGrounded && onWater)
        {
            Debug.Log("다이빙 조건문");
            isGroundRayHit = Physics.Raycast(inspectionOrigin, Vector3.down, out groundHit, checkDistance, groundLayer);
            //DeepWaterCheckOnAir();
            DiveInToDeep();
        }
        
    }

    /*
     * 1. 플레이어가 트리거 엔터, 트리거 스테이 하는 동안 검사.
     * 2. 지면에서 걸어들어가는 경우 : 플레이어의 현재 트랜스폼에서 위로 레이캐스트(물의 표면에 콜라이더가 있으니까) 그 거리를 검사해서 최대 수중 깊이보다 크면 플레이어를 마지막 위치로 강제이동
     * 3. 점프해서 빠지는 경우 : 트리거 엔터 하는 순간, 플레이어 발 아래로 레이캐스트, 땅에 닿는 거리를 검사해서 최대 수중 깊이보다 크면.. 어떡하지? 밀어낸다? 일정거리 주변 땅 탐색(안전한 곳)
     * -> 안돼서 걍 lastSafePosition으로 강제이동
    */

    public void TriggerEnter(GameObject water, Collider other)
    {
        if(other.gameObject.layer == 3)
        {
            player.isInWater = true;
            onWater = true;
        }
        
    }

    public void TriggerStay(GameObject water, Collider other)
    {
        if (other.gameObject.layer != 3) return;

        isGroundRayHit = Physics.Raycast(inspectionOrigin, Vector3.down, out groundHit, checkDistance, groundLayer);
        if (isGroundRayHit)
        {
            groundHeight = groundHit.point.y;
        }

        if (player.isGrounded)
        {
            // 걸어 들어간 경우 검사
            DeepWaterCheckOnWalk();
        }
        else
        {
            // 공중에서 빠지는 경우 검사
            DeepWaterCheckOnAir();
        }
    }

    public void TriggerExit(GameObject water, Collider other)
    {
        if(other.gameObject.layer == 3)
        {
            //Debug.Log("물 밖으로 나감");
            player.isInWater = false;
        }
    }

    private void DeepWaterCheckOnWalk()
    {

        forwardDepth = waterHeight - groundHeight;
        if(forwardDepth >= maxDepth)
        {
            //Debug.Log("너무 깊은 물");
            player.isDeepWater = true;
        }
        else
        {
            player.isDeepWater = false;
        }
    }

    private void DeepWaterCheckOnAir()
    {
        forwardDepth = waterHeight - groundHeight;

        if (forwardDepth >= maxDepth)
        {
            //Debug.Log("너무 깊은 물, 공중에 뜬 상태");
            player.isDeepWater = true;
        }
        else
        {
            player.isDeepWater = false;
        }
    }

    private void DiveInToDeep()
    {
        if (!isGroundRayHit)
        {
            Debug.Log("깊은 물에 다이빙 했습니다. 집으로 강제 이송");
            player.PlayerRespawn();
            forwardDepth = 0f;
            currentDepth = 0f;
            return;
        }
        else
        {
            groundHeight = groundHit.point.y;
            forwardDepth = waterHeight - groundHeight;
            if (forwardDepth >= maxDepth)
            {
                player.isDeepWater = true;
                Debug.Log("다이빙 막기");
            }
            
        }
    }

    private void UnderTheFeet()
    {
        isWaterRayHit = Physics.Raycast(inspectionOrigin, Vector3.down, out waterHit, 1f, overGroundLayer);
        if (isWaterRayHit)
        {
            if (waterHit.transform.gameObject.layer == 4)
            {
                onWater = true;
                waterHeight = waterHit.transform.position.y;
            }
            else
            {
                onWater = false;
            }
        }
    }

    private void RealDepthCheck()
    {
        if (Physics.Raycast(player.transform.position + Vector3.up * 1f, Vector3.down, out RaycastHit hit, checkDistance, groundLayer))
        {
            currentDepth = waterHeight - hit.point.y;
        }
    }
    private void DrowningCheck()
    {
        if(currentDepth >= maxDepth)
        {
            if (timer >= drowningCheckTime)
            {
                Debug.Log("물에 빠졌습니다. 강제 구출!");
                //player.characterController.Move(lastSavePosition);
                player.PlayerRespawn();
                Debug.Log($"플레이어 위치 {player.transform.position}");
                forwardDepth = 0f;
                currentDepth = 0f;
                timer = 0f;
                return;
            }
            else
            {
                timer += Time.deltaTime;
            }
        }
    }
}
