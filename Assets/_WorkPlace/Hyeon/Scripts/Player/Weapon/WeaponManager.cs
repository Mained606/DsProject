///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// JWS 수정                                                                                                            ///  
/// 2025.01.27 17:20 인벤토리에서 더블클릭으로 무기 장착 시스템 완성                                                    ///  
/// 이후 클릭이 아닌 소켓을 이용할때도 그대로 이용만 하면 됨.                                                           ///
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using Unity.VisualScripting;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    private Transform weaponObjectPosition;
    private GameObject currentWeaponObject = null;
    public GameObject CurrentWeaponObject   //HJ 03.13 추가
    {
        get { return currentWeaponObject; }
        set { currentWeaponObject = value; }
    }
    private PlayerCombat combat;

    private void Start()
    {
        InitializeWeapons();

        combat = GameManager.playerTransform.GetComponent<PlayerCombat>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F11))
        {
            ItemEffectManager.Instance.UnequipmentEffect(EquipmentSlot.손);
        }
        if (Input.GetKeyDown(KeyCode.F12))
        {
            var itemsToAdd = new[] { "완드", "나뭇가지", "나무완드", "도끼", "낡은검", "양손검","비트","무","버섯"}; // 비트 무 버섯 추가 
            foreach (var item in itemsToAdd) ItemManager.Instance.AddItemLogic(item);
        }
    }

    private void InitializeWeapons()
    {
        if (weaponObjectPosition != null)
        {
            foreach (Transform transOb in weaponObjectPosition)
            {
                string objectName = transOb.name;
                if (System.Enum.TryParse(objectName, out WeaponObjectName weaponName))
                {
                    int index = (int)weaponName;
                    transOb.SetSiblingIndex(index);
                    Debug.Log($"'{objectName}'은 WeaponObjectName의 인덱스 {index}입니다.");
                }
                else
                {
                    transOb.SetSiblingIndex(weaponObjectPosition.childCount - 1);
                    Debug.LogWarning($"'{objectName}'은 WeaponObjectName에 존재하지 않으므로 마지막으로 이동합니다.");
                }
                transOb.gameObject.SetActive(false);
            }
        }
    }

    public void EquipWeapon(Item weaponItem = null)
    {
        if (weaponItem != null && weaponItem.equipmentSlot != EquipmentSlot.손)
            return;
        if (weaponItem == null)
        {
            currentWeaponObject.SetActive(false);
            currentWeaponObject = null;
            combat.hasWeapon = false;
            combat.playerAnimator.SetBool("PhysicsWeapon", false);
            combat.playerAnimator.SetBool("MagicalWeapon", false);
            combat.physicsWeapon = false;
            combat.magicalWeapon = false;
        }
        else
        {
            if (System.Enum.TryParse(weaponItem.id, out WeaponName weaponName))
            {
                SwitchWeapon((int)weaponName);
                if(weaponItem.weaponType == WeaponType.한손무기 || weaponItem.weaponType == WeaponType.양손무기)
                {
                    combat.physicsWeapon = true;
                    combat.magicalWeapon = false;
                    combat.playerAnimator.SetBool("PhysicsWeapon", true);
                    combat.playerAnimator.SetBool("MagicalWeapon", false);
                }
                else if(weaponItem.weaponType == WeaponType.완드)
                {
                    combat.physicsWeapon = false;
                    combat.magicalWeapon = true;
                    combat.playerAnimator.SetBool("PhysicsWeapon", false);
                    combat.playerAnimator.SetBool("MagicalWeapon", true);
                }
            }
            combat.hasWeapon = true;
        }
    }

    public void SwitchWeapon(int index = -1)
    {
        Debug.Log($"Index = {index}");
        if(index == -1)
        {
            if(currentWeaponObject != null)
            {
                currentWeaponObject.SetActive(!currentWeaponObject.activeSelf);
            }
            return;
        }
        currentWeaponObject = transform.GetChild(index).gameObject;
        currentWeaponObject.SetActive(true);
        var playerCombat = GameManager.playerTransform.GetComponent<PlayerCombat>();
        playerCombat.weaponCollider = currentWeaponObject.GetComponent<Collider>();
    }

    private enum WeaponObjectName
    {
        Staff_Basic,
        Staff_Medium,
        Wand_Basic,
        Simple_Axe_Variant,
        Sword1_1_3,
        MoonSword_6b
    }

    private enum WeaponName
    {
        완드,
        나뭇가지,
        나무완드,
        도끼,
        낡은검,
        양손검
    }
}


