using UnityEngine;

public class BreakTree : MonoBehaviour
{
    #region Variables
    public GameObject originTree;   //원래의 나무
    public GameObject brokenTree;   //나무 조각
    public int woodPieceCount = 3;  //나무 조각 갯수
    public float woodPieceForce = 5f; // 나무 조각 튕김 힘
    public int treeHealth = 10;     //나무 체력

    #endregion

    //맞을 경우
    public void TakeDamage(int damage)
    {
        //나무 맞음
        treeHealth -= damage;

        //나무 피 0 되면 없어짐
        if(treeHealth <= 0)
        {
            DestroyTree();
        }        
    }

    //나무 파괴
    private void DestroyTree()
    {
        //나무 조각 생성
        for(int i = 0; i < woodPieceCount; ++i)
        {
            SpawnWoodPiece();
        }

        //나무 삭제
        Destroy(originTree);
    }

    //나무 조각 생성 메서드
    private void SpawnWoodPiece()
    {
        // 랜덤 위치와 회전
        Vector3 randomPosition = transform.position + new Vector3(
            Random.Range(-0.5f, 0.5f),
            Random.Range(0.5f, 1.5f),
            Random.Range(-0.5f, 0.5f)
        );
        Quaternion randomRotation = Quaternion.Euler(
            Random.Range(0, 360),
            Random.Range(0, 360),
            Random.Range(0, 360)
        );

        //나무 조각 생성
        GameObject woodPiece = Instantiate(brokenTree,randomPosition,randomRotation);

        // Rigidbody를 통해 물리적 힘을 추가
        Rigidbody rb = woodPiece.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 randomForce = new Vector3(
                Random.Range(-1f, 1f),
                1f,
                Random.Range(-1f, 1f)
            ).normalized * woodPieceForce;

            rb.AddForce(randomForce, ForceMode.Impulse);
        }
    }
}
