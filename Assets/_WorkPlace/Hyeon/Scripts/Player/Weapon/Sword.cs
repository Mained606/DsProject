using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{
    private int swordDamage = 10;

    private Collider swordCollider;

    private int currentComboIndex = 0;

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
            MonsterData monster = other.GetComponent<TestMonster>().monsterData;
            if (monster != null)
            {
                monsterHP = monster.currentHp;
                monster.TakeDamage(swordDamage);
                Debug.Log($"Damaged: {monster.characterName}, monsterHP: {monsterHP}, monsterCurrentHP: {monster.currentHp}");
            }

        }
    }
}
