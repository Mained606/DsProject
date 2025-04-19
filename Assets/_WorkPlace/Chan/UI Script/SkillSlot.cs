using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SkillSlot : MonoBehaviour, IDropHandler
{
    [SerializeField] private Image iconImage;
    private Animator animator;

    [SerializeField]private Image cooldownOverlay;
    private int slotIndex;
    private Skills currentSkill;
    private Sprite currentIcon;

    [SerializeField]private Image bg;

   
    public void Initialize(int index)
    {
        slotIndex = index;
        bg.color = new Color(1f, 1f, 1f, 0f);
        ClearSlot();
    }

    public void SetSkill(Skills skill, Sprite icon = null)
    {
        ClearSlot();
        currentSkill = skill;
        currentIcon = icon ?? ItemManager.Instance.GetSkillSprite(skill.skillName);
        iconImage.sprite = currentIcon;
        iconImage.enabled = true;

        if (cooldownOverlay != null)
        {
            if (currentSkill.cooldownTimer == null || !currentSkill.cooldownTimer.IsRunning)
            {
                cooldownOverlay.fillAmount = 0f;
            }
        }
        bg.color = new Color(1f, 1f, 1f, 1f);
    }

    public void ClearSlot()
    {
        currentSkill = null;
        currentIcon = null;
        iconImage.sprite = null;
        iconImage.enabled = false;
        if (cooldownOverlay != null) cooldownOverlay.fillAmount = 0f;
        bg.color = new Color(1f, 1f, 1f, 0f);
    }

    public void OnDrop(PointerEventData eventData)
    {

        GameObject droppedObj = eventData.pointerDrag;
        if (droppedObj == null)
        {
            return;
        }

        SkillDrag dragItem = droppedObj.GetComponent<SkillDrag>();
        if (dragItem == null)
        {
            return;
        }
        //Debug.Log($"[SkillSlot] SkillDrag 감지됨: {dragItem.SkillData.skillName}");
        SkillQuickSlotUI.Instance.AssignSkillToSlot(dragItem.SkillData, dragItem.Icon, slotIndex);
    }

    private void Update()
    {
        if (currentSkill == null || currentSkill.cooldownTimer == null) return;

        if (cooldownOverlay != null && currentSkill.cooldownTimer.IsRunning)
        {
            cooldownOverlay.fillAmount = currentSkill.cooldownTimer.RemainingPercent;
        }
    }

    public Skills GetAssignedSkill() => currentSkill;
}
