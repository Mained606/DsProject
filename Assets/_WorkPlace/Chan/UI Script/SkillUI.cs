using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillUI : MonoBehaviour
{
    [SerializeField] private Transform ADPanelParent;   // AD 스킬 목록
    [SerializeField] private Transform APPanelParent;   // AP 스킬 목록
    [SerializeField] private GameObject InfoPanel;      // 우측 인포 판넬

    [SerializeField] private GameObject ADskillPrefab;  // AD 스킬 프리팹
    [SerializeField] private GameObject APskillPrefab;  // AP 스킬 프리팹

    [SerializeField] private Image skillInfoImage;       // 인포창에 표시할 스킬 아이콘
    [SerializeField] private TextMeshProUGUI skillInfoText;           // 인포창의 스킬 상세 정보 텍스트
    [SerializeField] private TextMeshProUGUI skillLevelPreviewText;   // 인포창의 미리보기 텍스트 (변동된 레벨/데미지)

    // 현재 InfoPanel에 표시된 스킬
    private Skills currentSelectedSkill;
    // 임시로 누적된 스킬 레벨 변화량
    private int tempSkillDelta = 0;

    private void OnEnable()
    {
        UpdateSkillUI();  // 스킬 목록 UI 갱신
        InfoPanel.SetActive(false);
    }

    // 스킬 목록 UI 업데이트: 기존 프리팹 제거 후 플레이어 스킬 목록 재생성
    void UpdateSkillUI()
    {
        foreach (Transform child in ADPanelParent)
            Destroy(child.gameObject);
        foreach (Transform child in APPanelParent)
            Destroy(child.gameObject);

        List<Skills> playerSkills = GetPlayerSkills();
        foreach (var skill in playerSkills)
        {
            if (skill.skillType == SkillType.Physical)
                AddSkillToPanel(ADPanelParent, ADskillPrefab, skill);
            else if (skill.skillType == SkillType.Magic)
                AddSkillToPanel(APPanelParent, APskillPrefab, skill);
        }
    }

    // 플레이어 스킬 목록 반환
    List<Skills> GetPlayerSkills()
    {
        List<Skills> playerSkills = new List<Skills>();
        foreach (var entry in SkillManager.SkillList)
        {
            if (entry.Key.Item1 == EntityType.Player)
                playerSkills.Add(entry.Value);
        }
        return playerSkills;
    }

    // 스킬 프리팹 생성 및 버튼 이벤트 등록
    void AddSkillToPanel(Transform panelParent, GameObject skillPrefab, Skills skill)
    {
        GameObject skillInstance = Instantiate(skillPrefab, panelParent);

        // 두 번째 Image 컴포넌트로 스킬 아이콘 설정
        var skillImage = skillInstance.GetComponentsInChildren<Image>()[1];
        if (skillImage != null)
            skillImage.sprite = ItemManager.Instance.GetSkillSprite(skill.skillName);

        Button skillButton = skillInstance.GetComponent<Button>();
        if (skillButton != null)
            skillButton.onClick.AddListener(() => ShowSkillInfo(skill));
    }

    // InfoPanel에 스킬 정보 표시 (클릭 시)
    void ShowSkillInfo(Skills skill)
    {
        currentSelectedSkill = skill;
        tempSkillDelta = 0; // 새로 선택하면 임시 변화량 초기화
        InfoPanel.SetActive(true);

        if (skillInfoText != null)
            skillInfoText.text = skill.ToStringTMPro();
        if (skillInfoImage != null)
            skillInfoImage.sprite = ItemManager.Instance.GetSkillSprite(skill.skillName);
        if (skillLevelPreviewText != null)
            skillLevelPreviewText.text = ""; // 미리보기 텍스트 초기 상태는 빈 문자열
    }

    public string GetSkillPreviewString(Skills skill, int delta)
    {
        // delta: 플러스/마이너스 누른 누적 변화량
        int newLevel = skill.skillLevel + delta;
        // 선형 증가: 기본 데미지의 10%씩 증가
        float newDamage = skill.currentDamage + (skill.currentDamage * 0.1f * delta);
        float newEnergyCost = skill.energyCost;
        float newCooldown = skill.cooldown;
        // 원래 ToStringTMPro()는 첫 줄에 스킬 이름과 색상이 들어가는데,
        // 여기서는 첫 줄을 빈 문자열로 남기고, 그 다음 줄부터 정보를 표기
        string preview =
        $"<b><color=#FFFFFF></color></b>\n" +   // 첫 줄은 빈 상태
        $"레벨: {newLevel}\n" +
        $"데미지: {newDamage:F1}\n" +
        $"소모 MP: {newEnergyCost}\n" +
        $"쿨타임: {newCooldown}초\n";

        return preview;
    }

    // 플러스 버튼 이벤트: 임시로 스킬 레벨 +1 적용
    public void OnSkillLevelUpButton()
    {
        if (currentSelectedSkill != null)
        {
            // 스킬이 잠겨 있으면 아무 동작도 하지 않음.
            if (!currentSelectedSkill.unLockSkill)
            {
                Debug.LogWarning($"{currentSelectedSkill.skillName}은(는) 잠겨있어 레벨업할 수 없어.");
                return;
            }
            // AdjustTempSkill()가 성공하면 누적된 임시 변화량을 가져옴
            if (CharacterManager.PlayerCharacterData.AdjustTempSkill(currentSelectedSkill, 1))
            {
                tempSkillDelta = CharacterManager.PlayerCharacterData.GetPreviewSkillLevel(currentSelectedSkill);
                if (skillLevelPreviewText != null)
                    skillLevelPreviewText.text = GetSkillPreviewString(currentSelectedSkill, tempSkillDelta);
            }
        }
    }

    // 마이너스 버튼 이벤트: 임시로 스킬 레벨 -1 적용
    public void OnSkillLevelDownButton()
    {
        if (currentSelectedSkill != null)
        {
            // 스킬이 잠겨 있으면 아무 동작도 하지 않음.
            if (!currentSelectedSkill.unLockSkill)
            {
                Debug.LogWarning($"{currentSelectedSkill.skillName}은(는) 잠겨있어 레벨다운할 수 없어.");
                return;
            }

            if (CharacterManager.PlayerCharacterData.AdjustTempSkill(currentSelectedSkill, -1))
            {
                tempSkillDelta = CharacterManager.PlayerCharacterData.GetPreviewSkillLevel(currentSelectedSkill);
                if (skillLevelPreviewText != null)
                    skillLevelPreviewText.text = GetSkillPreviewString(currentSelectedSkill, tempSkillDelta);
            }
        }
    }

    // 확정 버튼 이벤트: 임시 할당값을 최종 확정하고 미리보기 텍스트를 초기화, UI 갱신
    public void OnSkillLevelConfirmButton()
    {
        if (currentSelectedSkill != null)
        {
            CharacterManager.PlayerCharacterData.ConfirmTempSkillAllocation();
            // 최신 스킬 데이터를 다시 불러와서 currentSelectedSkill에 재할당
            currentSelectedSkill = SkillManager.Instance.GetSkill(EntityType.Player, currentSelectedSkill.skillName);
            if (skillLevelPreviewText != null)
                skillLevelPreviewText.text = "";
            ShowSkillInfo(currentSelectedSkill);
        }
    }
}