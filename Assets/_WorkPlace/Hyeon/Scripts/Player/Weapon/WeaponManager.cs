using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> weapons;

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

        if(currentWeaponIndex != -1)
        {
            weapons[currentWeaponIndex].SetActive(false);
        }

        weapons[index].SetActive(true);
        currentWeaponIndex = index;
    }
    
    // 다음 인덱스 무기로 스위칭
    public void SwitchToNextWeapon()
    {
        int nextIndex = (currentWeaponIndex + 1) % weapons.Count;
        EquipWeapon(nextIndex);
    }

    public void SwitchToPreviousWeapon()
    {
        int prevIndex = (currentWeaponIndex - 1 + weapons.Count) % weapons.Count;
        EquipWeapon(prevIndex);
    }
}
