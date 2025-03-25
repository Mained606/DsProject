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
        foreach (Transform child in ADPanelParent) Destroy(child.gameObject);
        foreach (Transform child in APPanelParent) Destroy(child.gameObject);

        List<Skills> playerSkills = GetPlayerSkills();
        foreach (var skill in playerSkills)
        {
            var parent = skill.skillType == SkillType.Physical ? ADPanelParent : APPanelParent;
            var prefab = skill.skillType == SkillType.Physical ? ADskillPrefab : APskillPrefab;
            AddSkillToPanel(parent, prefab, skill);
        }
    }

    // 플레이어 스킬 목록 반환
    List<Skills> GetPlayerSkills()
    {
        List<Skills> skills = new();
        foreach (var entry in SkillManager.SkillList)
        {
            if (entry.Key.Item1 == EntityType.Player)
                skills.Add(entry.Value);
        }
        return skills;
    }

    #region 수정완료
    // 스킬 프리팹 생성 및 버튼 이벤트 등록
    void AddSkillToPanel(Transform panelParent, GameObject prefab, Skills skill)
    {
        GameObject go = Instantiate(prefab, panelParent);
        var img = go.GetComponentsInChildren<Image>()[1];
        Sprite icon = ItemManager.Instance.GetSkillSprite(skill.skillName);
        img.sprite = icon;

        if (!go.TryGetComponent(out SkillDrag drag))
            drag = go.AddComponent<SkillDrag>();
        drag.Initialize(skill, icon);

        if (go.TryGetComponent(out Button btn))
        {
            btn.onClick.AddListener(() => ShowSkillInfo(skill));
        }
    }
    #endregion

    #region 수정전 기존 메서드 
    /*void AddSkillToPanel(Transform panelParent, GameObject skillPrefab, Skills skill)
    {
        GameObject skillInstance = Instantiate(skillPrefab, panelParent);

        // 두 번째 Image 컴포넌트로 스킬 아이콘 설정
        var skillImage = skillInstance.GetComponentsInChildren<Image>()[1];
        if (skillImage != null)
            skillImage.sprite = ItemManager.Instance.GetSkillSprite(skill.skillName);

        Button skillButton = skillInstance.GetComponent<Button>();
        if (skillButton != null)
            skillButton.onClick.AddListener(() => ShowSkillInfo(skill));
    }*/
    #endregion

    // InfoPanel에 스킬 정보 표시 (클릭 시)5
    public void ShowSkillInfo(Skills skill)
    {
        currentSelectedSkill = skill;
        tempSkillDelta = 0;
        if (!InfoPanel.activeSelf)
            InfoPanel.SetActive(true);

        skillInfoImage.sprite = ItemManager.Instance.GetSkillSprite(skill.skillName);
        skillInfoText.text = skill.ToStringTMPro();
        skillLevelPreviewText.text = "";
    }

    public string GetSkillPreviewString(Skills skill, int delta)
    {
        int newLevel = skill.skillLevel + delta;
        float newDamage = skill.currentDamage + (skill.currentDamage * 0.1f * delta);
        return $"<b><color=#FFFFFF></color></b>\n레벨: {newLevel}\n데미지: {newDamage:F1}\n소모 MP: {skill.energyCost}\n쿨타임: {skill.cooldown}초\n";
    }

    // 플러스 버튼 이벤트: 임시로 스킬 레벨 +1 적용
    public void OnSkillLevelUpButton()
    {
        if (currentSelectedSkill == null || !currentSelectedSkill.unLockSkill) return;

        if (CharacterManager.PlayerCharacterData.AdjustTempSkill(currentSelectedSkill, 1))
        {
            tempSkillDelta = CharacterManager.PlayerCharacterData.GetPreviewSkillLevel(currentSelectedSkill);
            skillLevelPreviewText.text = GetSkillPreviewString(currentSelectedSkill, tempSkillDelta);
        }
    }

    public void OnSkillLevelDownButton()
    {
        if (currentSelectedSkill == null || !currentSelectedSkill.unLockSkill) return;

        if (CharacterManager.PlayerCharacterData.AdjustTempSkill(currentSelectedSkill, -1))
        {
            tempSkillDelta = CharacterManager.PlayerCharacterData.GetPreviewSkillLevel(currentSelectedSkill);
            skillLevelPreviewText.text = GetSkillPreviewString(currentSelectedSkill, tempSkillDelta);
        }
    }

    public void OnSkillLevelConfirmButton()
    {
        if (currentSelectedSkill == null) return;

        CharacterManager.PlayerCharacterData.ConfirmTempSkillAllocation();
        currentSelectedSkill = SkillManager.Instance.GetSkill(EntityType.Player, currentSelectedSkill.skillName);
        skillLevelPreviewText.text = "";
        ShowSkillInfo(currentSelectedSkill);
    }
}