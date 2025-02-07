using UnityEngine;

public class JumpSkillController : MonoBehaviour
{
    private BaseBossAI bossAI;  // 보스를 참조할 변수

    public void SetBoss(BaseBossAI boss)
    {
        bossAI = boss;  // 보스 데이터를 설정
    }

    // 트리거와 콜라이더로 플레이어를 감지한 후, 공격을 처리하는 메서드
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && bossAI != null)
        {
            
        }
    }
}
