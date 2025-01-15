using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{
    private int swordDamage = 10;

    private Collider swordCollider;

    public bool CanReceiveInput { get; set; } = true;
    public bool inputReceived = false;

    public int MaxComboCount { get; set; } = 3;

    public HashSet<GameObject> DamagedTargets { get; set; } = new HashSet<GameObject>();

    private void Start()
    {
        swordCollider = GetComponentInParent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!swordCollider.enabled) return;
        if (!DamagedTargets.Contains(other.gameObject))
        {
            DamagedTargets.Add(other.gameObject);
            int monsterHP = 0;
            if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                MonsterData monster = other.GetComponent<Test1>().monster;
                if (monster != null)
                {
                    monsterHP = monster.currentHp;
                    // monster.TakeDamage(swordDamage);
                    CombatManager.Instance.ProcessAttack(CharacterManager.PlayerCharacterData, monster, other.transform, true);
                    Debug.Log($"Damaged: {monster.characterName}, monsterHP: {monsterHP}, monsterCurrentHP: {monster.currentHp}");
                }
            }
            else
            {
                Debug.Log("암튼 뭔갈 침");
            }
        }
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (!swordCollider.enabled || DamagedTargets.Contains(other.gameObject)) return;

    //    DamagedTargets.Add(other.gameObject);

    //    // MonsterData 가져오기
    //    // Test1 스크립트를 통해 MonsterData 가져오기
    //    Test1 test1 = other.GetComponent<Test1>();
    //    if (test1 == null || test1.monster == null)
    //    {
    //        Debug.LogWarning("타겟에 Test1 또는 MonsterData가 없습니다.");
    //        return;
    //    }
    //    MonsterData targetData = test1.monster;

    //    // 플레이어 데이터 가져오기
    //    PlayerData playerData = CharacterManager.Instance.GetPlayerCharacter();
    //    if (playerData == null)
    //    {
    //        Debug.LogWarning("플레이어 데이터가 없습니다.");
    //        return;
    //    }

    //    // CombatManager에 공격 요청
    //    CombatManager.Instance.ProcessAttack(playerData, targetData, other.transform, true);
    //}

    //// 애니메이션 이벤트를 통해 호출 혹은 공격이 종료되었을 때 타겟 초기화
    //public void ResetDamagedTargets()
    //{
    //    DamagedTargets.Clear();
    //}

}
