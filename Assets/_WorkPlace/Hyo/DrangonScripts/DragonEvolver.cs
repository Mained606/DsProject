using UnityEngine;

public class DragonEvolver : MonoBehaviour
{
    private void Start()
    {
        // CharacterManager가 초기화되어 있는지 확인
        if (CharacterManager.Instance == null)
        {
            Debug.LogError("CharacterManager가 초기화되지 않았습니다!");
            return;
        }

        // DragonData가 초기화되어 있는지 확인
        if (CharacterManager.DragonData == null)
        {
            Debug.LogError("DragonData가 초기화되지 않았습니다!");
            return;
        }

        // 초기화 확인 로그
        //Debug.Log("DragonEvolver 초기화 완료: CharacterManager와 DragonData가 정상적으로 존재합니다.");
    }

    public void EvolveDragon()
    {
        // 항상 먼저 초기화 상태 확인
        if (CharacterManager.Instance == null)
        {
            Debug.LogError("CharacterManager가 초기화되지 않았습니다!");
            return;
        }

        if (CharacterManager.DragonData == null)
        {
            Debug.LogError("DragonData가 초기화되지 않았습니다. CharacterManager를 확인하세요.");
            return;
        }

        bool success = CharacterManager.DragonData.Evolve();
        //Debug.Log($"드래곤 진화 시도 결과: {(success ? "성공" : "실패")}");
    }

    public void AddDragonExp(int amount = 100)
    {
        // 항상 먼저 초기화 상태 확인
        if (CharacterManager.Instance == null)
        {
            Debug.LogError("CharacterManager가 초기화되지 않았습니다!");
            return;
        }

        if (CharacterManager.DragonData == null)
        {
            Debug.LogError("드래곤 데이터가 초기화되지 않았습니다. CharacterManager를 확인하세요.");
            return;
        }

        // 이제 안전하게 경험치 추가 가능
        CharacterManager.DragonData.AddBondExperience(amount);
        //Debug.Log($"드래곤 경험치 {amount} 추가 완료");
    }
}