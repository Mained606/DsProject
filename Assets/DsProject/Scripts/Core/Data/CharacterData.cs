using UnityEngine;

public enum CharacterType
{
    Player,    // 플레이어
    Monster,   // 몬스터
    Pet,       // 펫
    Npc,       // NPC
    Boss       // 보스
}

public class CharacterData
{
    public string Name { get; private set; }
    public CharacterType Type { get; private set; }
    public float MaxHealth { get; private set; }
    public float CurrentHealth { get; set; } // 현재 체력
    public float AttackPower { get; private set; }
    public float MovementSpeed { get; private set; }

    public CharacterData(string name, CharacterType type, float maxHealth, float attackPower, float movementSpeed)
    {
        Name = name;
        Type = type;
        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;
        AttackPower = attackPower;
        MovementSpeed = movementSpeed;
    }

    public bool IsDead => CurrentHealth <= 0; // 사망 상태 확인
}