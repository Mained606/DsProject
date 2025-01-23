using UnityEngine;

public abstract class InteractableOb : MonoBehaviour
{
    protected int hitCount = 0; // 현재 타격 수
    public int maxHits = 3;     // 부서지기까지 필요한 타격 수

    // 상호작용 처리 메서드
    public virtual void Interact(string toolTag)
    {
        hitCount++;
        Debug.Log($"현재 타격 수: {hitCount}/{maxHits}");

        if (hitCount >= maxHits)
        {
            DestroyObject();
        }
    }

    // 오브젝트 삭제 처리
    protected virtual void DestroyObject()
    {        
        Destroy(gameObject);
    }
}
