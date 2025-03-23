using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SkillSlot : MonoBehaviour, IDropHandler
{
    [SerializeField] private Image iconImage;
    private Animator animator;

    private Image cooldownOverlay;
    private int slotIndex;
    private Skills currentSkill;
    private Sprite currentIcon;

    private void Awake()
    {
        cooldownOverlay = GetComponentsInChildren<Image>()[1];
    }

    public void Initialize(int index)
    {
        slotIndex = index;
        ClearSlot();
    }

    public void SetSkill(Skills skill, Sprite icon = null)
    {
        ClearSlot();

        currentSkill = skill;
        currentIcon = icon ?? ItemManager.Instance.GetSkillSprite(skill.skillName);
        iconImage.sprite = currentIcon;
        iconImage.enabled = true;
    }

    public void ClearSlot()
    {
        currentSkill = null;
        currentIcon = null;
        iconImage.sprite = null;
        iconImage.enabled = false;
        if (cooldownOverlay != null) cooldownOverlay.fillAmount = 0f;
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("[SkillSlot] 드랍 발생");

        GameObject droppedObj = eventData.pointerDrag;
        if (droppedObj == null)
        {
            Debug.LogWarning("[SkillSlot] pointerDrag가 null이다.");
            return;
        }

        SkillDrag dragItem = droppedObj.GetComponent<SkillDrag>();
        if (dragItem == null)
        {
            Debug.LogWarning("[SkillSlot] 드래그된 오브젝트에 SkillDrag가 없다.");
            return;
        }

        Debug.Log($"[SkillSlot] SkillDrag 감지됨: {dragItem.SkillData.skillName}");

        SkillQuickSlotUI.Instance.AssignSkillToSlot(dragItem.SkillData, dragItem.Icon, slotIndex);
    }

    private void Update()
    {
        if (currentSkill == null || currentSkill.cooldownTimer == null) return;

        float percent = currentSkill.cooldownTimer.RemainingPercent;
        if (cooldownOverlay != null)
            cooldownOverlay.fillAmount = percent;
    }

    public Skills GetAssignedSkill() => currentSkill;
}
