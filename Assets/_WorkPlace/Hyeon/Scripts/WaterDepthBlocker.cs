using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class WaterDepthBlocker : MonoBehaviour
{
    [SerializeField] private float maxDepth = 2.3f;             // 플레이어가 들어갈 수 있는 최대 수중 깊이
    [SerializeField] private float forwardDepth;                // 감지 구역 깊이
    [SerializeField] private float currentDepth;                // 플레이어가 서있는 위치의 깊이
    private float checkDistance = 10f;                          // 깊이 검사 거리
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
        if (player.isInWater)
        {
            RealDepthCheck();
            DrowningCheck();
            return;
        }

        UnderTheFeet();
        if (!player.isGrounded && onWater)
        {
            isGroundRayHit = Physics.Raycast(inspectionOrigin, Vector3.down, out groundHit, checkDistance, groundLayer);
            DiveInToDeep();
        }
        
    }

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
            player.isInWater = false;
        }
    }

    private void DeepWaterCheckOnWalk()
    {

        forwardDepth = waterHeight - groundHeight;
        if(forwardDepth >= maxDepth)
        {
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
            Debug.LogWarning("깊은 물에 다이빙 했습니다. 집으로 강제 이송");
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
                Debug.LogWarning("물에 빠졌습니다. 강제 구출!");
                player.PlayerRespawn();
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
