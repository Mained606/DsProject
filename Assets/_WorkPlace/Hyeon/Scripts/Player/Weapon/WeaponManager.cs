using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> weapons;
    private GameObject currentWeapon;
    private int prevWeapon = -1;

    private int currentWeaponIndex = -1;

    private void Start()
    {
        InitializeWeapons();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            SwitchToNextWeapon();
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            SwitchToPreviousWeapon();
        }
    }

    private void InitializeWeapons()
    {
        foreach(var weapon in weapons)
        {
            weapon.SetActive(false);
        }
    }

    public void EquipWeapon(int index)
    {
        if(index < 0 || index >= weapons.Count)
        {
            Debug.LogWarning("Invalid weapon index");
            return;
        }
        currentWeaponIndex = index;
        SwitchWeapon(currentWeaponIndex);
    }
    
    // 다음 인덱스 무기로 스위칭
    public void SwitchToNextWeapon()
    {
        if (!GameManager.playerTransform.GetComponent<PlayerController>().CanWeaponSwitch)
            return;

        int nextIndex = (currentWeaponIndex + 1) % weapons.Count;
        EquipWeapon(nextIndex);
    }

    public void SwitchToPreviousWeapon()
    {
        if (!GameManager.playerTransform.GetComponent<PlayerController>().CanWeaponSwitch)
            return;

        int prevIndex = (currentWeaponIndex - 1 + weapons.Count) % weapons.Count;
        EquipWeapon(prevIndex);
    }

    public void SwitchWeapon(int index = -1, bool hasWeapon = false)
    { 
        currentWeapon = null;

        for (int i = 0; i < transform.childCount; i++)
        {
            // i번째 자식 가져오기
            GameObject gob = transform.GetChild(i).gameObject;

            gob.SetActive(index == i);
            // 현재 인덱스와 비교하여 활성화/비활성화 처리
            if (hasWeapon && i == prevWeapon)
            {
                gob.SetActive(i == prevWeapon);
                prevWeapon = -1;
            }

            // 활성화된 무기를 플레이어의 weaponCollider로 설정
            if (gob.activeSelf)
            {
                currentWeapon = gob;
                prevWeapon = i;
                GameManager.playerTransform.GetComponent<PlayerCombat>().weaponCollider = gob.GetComponent<Collider>();
            }
        }

        GameManager.playerTransform.GetComponent<PlayerCombat>().hasWeapon = currentWeapon == null? false : true;

    }
}
