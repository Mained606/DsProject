using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class WaterDepthBlocker : MonoBehaviour
{
    [SerializeField] private float maxDepth = 2.3f; // 플레이어가 들어갈 수 있는 최대 수중 깊이
    [SerializeField] private float currentDepth;                 // 현재 깊이
    private float checkDistance = 5f;           // 깊이 검사 거리
    private Vector3 lastSafePosition;           // 플레이어가 마지막으로 서있던 안전한 포지션
    private Vector3 lastSavePosition;
    [SerializeField] private Vector3 inspectionOrigin;

    [SerializeField] private LayerMask overGroundLayer;
    [SerializeField] private LayerMask groundLayer;

    private PlayerController player;
    private bool playerIsGrounded;

    private float inWaterStateCheckTime = 3f;
    private float timer = 0f;
    [SerializeField] private bool onWater;

    [SerializeField] private float waterHeight;
    [SerializeField] private float groundHeight;
    private bool isGroundRayHit;
    private bool isWaterRayHit;
    private RaycastHit groundHit;
    private RaycastHit waterHit;

    private void Start()
    {
        player = GameManager.playerTransform.GetComponent<PlayerController>();
        //lastSafePosition = player.transform.position;
        lastSavePosition = player.transform.position;

        inspectionOrigin = player.transform.position + player.transform.forward * 1f;
        waterHeight = transform.position.y;
    }

    private void Update()
    {
        inspectionOrigin = player.transform.position + player.transform.forward * 0.5f + player.transform.up * 1f;
        if (player.isInWater)
            return;

        
        UnderTheFeet();
        if (!player.isGrounded && onWater)
        {
            isGroundRayHit = Physics.Raycast(inspectionOrigin, Vector3.down, out groundHit, 10f, groundLayer);
            DeepWaterCheckOnAir();
        }
        //WaterExitCompletely();
    }

    /*
     * 1. 플레이어가 트리거 엔터, 트리거 스테이 하는 동안 검사.
     * 2. 지면에서 걸어들어가는 경우 : 플레이어의 현재 트랜스폼에서 위로 레이캐스트(물의 표면에 콜라이더가 있으니까) 그 거리를 검사해서 최대 수중 깊이보다 크면 플레이어를 마지막 위치로 강제이동
     * 3. 점프해서 빠지는 경우 : 트리거 엔터 하는 순간, 플레이어 발 아래로 레이캐스트, 땅에 닿는 거리를 검사해서 최대 수중 깊이보다 크면.. 어떡하지? 밀어낸다? 일정거리 주변 땅 탐색(안전한 곳)
     * -> 안돼서 걍 lastSafePosition으로 강제이동
    */

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == 3)
        {
            PlayerIsGroundedCheck();
            player.isInWater = true;
            onWater = true;
            if (timer != 0f)
            {
                timer = 0f;
            }
        }
        
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer != 3) return;

        //inspectionOrigin = player.transform.position + player.transform.forward * 0.5f + player.transform.up * 1f;
        isGroundRayHit = Physics.Raycast(inspectionOrigin, Vector3.down, out groundHit, 10f, groundLayer);
        if (isGroundRayHit)
        {
            groundHeight = groundHit.point.y;
        }

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

        currentDepth = waterHeight - groundHeight;
        if(currentDepth >= maxDepth)
        {
            Debug.Log("너무 깊은 물");
            player.isDeepWater = true;
        }
        else
        {
            player.isDeepWater = false;
        }
    }

    private void DeepWaterCheckOnAir()
    {
        if (!player.isInWater && onWater)
        {
            if (!isGroundRayHit)
            {
                Debug.Log("너무너무 깊은 물에 뛰어 들었습니다. 집으로 강제 이송");
                player.transform.position = lastSavePosition;
                return;
            }
        }
        
        currentDepth = waterHeight - groundHeight;

        if (currentDepth >= maxDepth)
        {
            Debug.Log("너무 깊은 물, 공중에 뜬 상태");
            player.isDeepWater = true;
        }
        else
        {
            player.isDeepWater = false;
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

    private void WaterExitCompletely()
    {
        if (timer >= inWaterStateCheckTime)
            onWater = false;
        else
            timer += Time.deltaTime;
    }
}
