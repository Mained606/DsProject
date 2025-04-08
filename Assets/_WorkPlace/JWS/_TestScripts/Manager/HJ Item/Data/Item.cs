using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 01.21 희정 아이템 이펙트 항목 추가, 아이템 스탯 헤더 수정n
/// 01.22 아이템 복제, 스탯 복제 함수 수정
/// 01.23 아이템 장착위치 enum에 방패 추가, 아이템 등급은 장착 아이템에만 적용, 아이템 생성자에 제작재료 타입 조건 추가,
/// 양손검, 한손검 아이템 추가로 인해서 weapontype 변수 추가
/// 01.24 퀘스트 아이템인지 확인하기 위한 변수 추가
/// 
/// 2.19 스탯 합산 함수 추가
/// 2.20 스탯에 요리용 회복 스탯 추가, 스탯 관련 함수 수정
/// 2.24 아이템타입에 요리재료, 요리 추가
/// 2.26 버프 스탯이 있는지 확인하는 함수 추가, 버프 스탯에 요리재료 지속시간 추가
/// 03.05 무기 타입에 완드 추가
/// </summary>
[Serializable]
public class Item : ISheetData
{
    [Header("공통 속성")]
    public string id;                      // 아이템 고유 ID
    public string name;                    // 아이템 이름
    [TextArea(3, 10)]
    public string description;             // 아이템 설명
    public ItemType type;                  // 아이템 타입
    public int costValue;                  // 아이템 가격
    public int quantity;                   // 현재 소지 개수
    public int maxStack;                   // 최대 소지 가능 개수
    public bool isDiscardable;             // 버릴 수 있는지 여부
    public bool isStackable;               // 중첩 가능 여부
    public Sprite sprite;                  // 아이템 아이콘
    public float dropChance;               // 드랍 확률 (%)
    public ItemEffect effect;              // 아이템 이펙트
    public bool isQuestItem;               // 퀘스트 아이템 여부

    [Header("스탯(장착아이템: 적용할 전체 값\n버프 물약: 해당하는 스탯만 값을 1로 설정\n요리재료 효과량 전체)")]
    public ItemStat itemStat;              // 스탯 정보 (힘, 민첩 등)
    public ItemSkill itemSkill;            // 아이템 스킬
    public Durability durability;          // 내구도
    public ItemGrade grade;                // 아이템 등급
    public EquipmentSlot equipmentSlot;    // 장착 위치 (무기, 방어구 등)
    public WeaponType weaponType;          // 무기 타입
    public bool isEquired;                 // 장착여부

    [Header("소모품 속성")]
    public ConsumableType consumableType;  // 소모품 타입
    public int effectAmount;               // 효과량 (예: 체력 회복량)

    [Header("퀘스트 속성")]
    public string questId;                 // 퀘스트 ID (퀘스트 아이템용)

    public Item() { }

    // 생성자
    public Item(string id, string name, string description, ItemType type, ItemGrade grade, 
        int quantity = 1, int maxStack = 99, bool isStackable = true, bool isDiscardable = true, int costValue = 0)
    {
        this.id = id;
        this.name = name;
        this.description = description;
        this.type = type;
        this.grade = grade;
        this.quantity = quantity;
        this.maxStack = maxStack;
        this.isStackable = isStackable;
        this.isDiscardable = isDiscardable;
        this.costValue = costValue;

        if (type == ItemType.무기 || type == ItemType.방어구)
        {
            // 장착 아이템 초기화
            this.itemStat = new ItemStat(1, 1, 1, 1); // 기본 스탯
            this.durability = new Durability(100);       // 기본 내구도
            this.isEquired = false;
            this.itemSkill = new ItemSkill();
            //this.itemStat.Initialize();
        }
        else if (type == ItemType.장신구)
        {
            // 퀘스트 아이템 초기화
            this.isDiscardable = false; // 퀘스트 아이템은 버릴 수 없음
            this.isEquired = false;
        }
        else if (type == ItemType.소모품 || type == ItemType.제작재료 || type == ItemType.요리재료)
        {
            // 소모품 초기화
            this.effectAmount = 0;
            this.maxStack = 99;  // 소모품 기본 최대 스택
            this.isStackable = true;
        }
        else if (type == ItemType.퀘스트)
        {
            // 퀘스트 아이템 초기화
            this.isDiscardable = false; // 퀘스트 아이템은 버릴 수 없음
        }
        else if(type == ItemType.요리)
        {
            this.isStackable = false;
        }
    }

    // 아이템 복제
    public Item Clone()
    {
        Item newItem = new Item(this.id, this.name, this.description, this.type, this.grade,
            this.quantity, this.maxStack, this.isStackable, this.isDiscardable, this.costValue);

        if (this.itemStat != null)
        {
            newItem.itemStat = this.itemStat.Clone();
        }

        if (this.durability != null)
        {
            newItem.durability = this.durability.Clone(); // 내구도도 복제
        }

        if(this.itemSkill != null)
        {
            newItem.itemSkill = this.itemSkill.Clone();
        }

        newItem.effect = this.effect;
        newItem.isQuestItem = this.isQuestItem;
        newItem.sprite = this.sprite;
        newItem.dropChance = this.dropChance;
        newItem.equipmentSlot = this.equipmentSlot;
        newItem.weaponType = this.weaponType;
        newItem.consumableType = this.consumableType;
        newItem.effectAmount = this.effectAmount;
        newItem.questId = this.questId;

        return newItem;
    }

    // UI에 표시할 아이템 정보 (TextMeshPro 전용)
    public string ToStringTMPro()
    {
        // 등급 색상
        string gradeColor = GetGradeColor(grade);

        // 무기 속성 색상
        string elementColor = itemSkill != null ? GetElementColor(itemSkill.element) : "#FFFFFF";
        string elementInfo = itemSkill != null && itemSkill.element != ElementalAttribute.None
            ? $"                            <size=120%>속성: <color={elementColor}>{itemSkill.element}</color></size>"
            : "";

        // 공통 정보
        string itemType = $"종류: <i><color=#87CEEB>{type}</color></i>    등급: <i><color={gradeColor}>{grade}</color></i>{elementInfo}\n";
        string itemDescription = $"<color=#FFFFFF>{description}</color>\n";
        string itemQuantity = isStackable
            ? $"수량: <color=#00FF00>{quantity}/{maxStack}</color>"
            : "수량: <color=#FF0000>중첩 불가</color>";

        // 장비 전용
        string equipmentInfo = type == ItemType.무기 || type == ItemType.방어구 || type == ItemType.장신구
            ? $"\n장착 위치: <color=#FFD700>{equipmentSlot}</color>" +
              (type == ItemType.무기 ? $"    무기 타입: <color=#FFD700>{weaponType}</color>" : "")
            : "";

        // 하단 효과 정보
        string statInfo = "";

        // 1. 장비
        if (type == ItemType.무기 || type == ItemType.방어구 || type == ItemType.장신구)
        {
            statInfo = itemStat != null
                ? $"\n<b><color=#FFD700>[스탯 정보]</color></b>\n\n" +  // ⬅ 공백 추가됨
                  (itemStat.Strength > 0 ? $"힘 : {itemStat.Strength}    " : "") +
                  (itemStat.Dexterity > 0 ? $"민첩 : {itemStat.Dexterity}    " : "") +
                  (itemStat.Intelligence > 0 ? $"지능 : {itemStat.Intelligence}    " : "") +
                  (itemStat.Vitality > 0 ? $"활력 : {itemStat.Vitality}    " : "") +
                  (itemStat.PhysicalAttack > 0 ? $"\n물리 공격력 : {itemStat.PhysicalAttack}    " : "") +
                  (itemStat.MagicAttack > 0 ? $"마법 공격력 : {itemStat.MagicAttack}    " : "") +
                  (itemStat.PhysicalDefense > 0 ? $"물리 방어력 : {itemStat.PhysicalDefense}    " : "") +
                  (itemStat.MagicDefense > 0 ? $"\n마법 방어력 : {itemStat.MagicDefense}    " : "") +
                  (itemStat.CriticalChance > 0 ? $"치명타 확률 : {itemStat.CriticalChance}%    " : "") +
                  (itemStat.AttackSpeed > 0 ? $"공격 속도 : {itemStat.AttackSpeed}    " : "") +
                  (itemStat.Evasion > 0 ? $"\n회피율 : {itemStat.Evasion}%    " : "")
                : "";
        }

        // 2. 소모품
        else if (type == ItemType.소모품)
        {
            string effectLine = "";
            if (effect.effectType == EffectType.Hp)
                effectLine = $"체력 회복량: <color=#00FF00>{effectAmount}</color>";
            else if (effect.effectType == EffectType.Mp)
                effectLine = $"마나 회복량: <color=#00BFFF>{effectAmount}</color>";
            else if (effect.effectType == EffectType.Buff)
            {
                string buffText = itemStat?.GetEffectDescription() ?? "";
                string durationText = effect.duration > 0 ? $"\n지속시간: <color=#FFD700>{effect.duration}초</color>" : "";
                effectLine = $"<b>버프 효과:</b> {buffText}{durationText}";
            }

            statInfo = !string.IsNullOrEmpty(effectLine)
                ? $"\n<b><color=#FFD700>[소모품 효과]</color></b>\n\n{effectLine}"
                : "";
        }

        // 3. 요리
        else if (type == ItemType.요리)
        {
            string recovery = "";
            if (itemStat.HealHp > 0) recovery += $"체력 회복: <color=#00FF00>+{itemStat.HealHp}</color>    ";
            if (itemStat.HealMp > 0) recovery += $"마나 회복: <color=#00BFFF>+{itemStat.HealMp}</color>";

            string buffText = itemStat.HasBuffStat() ? itemStat.GetEffectDescription() : "";
            string durationText = effect.duration > 0 ? $"\n지속시간: <color=#FFD700>{effect.duration}초</color>" : "";

            statInfo = $"\n<b><color=#FFD700>[요리 효과]</color></b>\n\n";  // ⬅ 공백 추가됨
            if (!string.IsNullOrEmpty(recovery)) statInfo += recovery + "\n";
            if (!string.IsNullOrEmpty(buffText)) statInfo += $"버프 효과: {buffText}{durationText}";
        }

        // 최종 문자열 조합
        return $"{itemType}{itemDescription}\n{itemQuantity}\n{equipmentInfo}{statInfo}".Trim();
    }

    #region 타입별 속성 03.12
    // 아이템 속성별 색상
    public string GetElementColor(ElementalAttribute element)
    {
        switch (element)
        {
            case ElementalAttribute.Fire: return "#FF4500";  // 주황색
            case ElementalAttribute.Water: return "#1E90FF"; // 파란색
            case ElementalAttribute.Electric: return "#FFFF00"; // 노란색
           case ElementalAttribute.Earth: return "#8B4513"; // 갈색
            default: return "#FFFFFF"; // 무속성 흰색
        }
    }
    #endregion


    // 등급에 따른 색상 반환 메서드
    public string GetGradeColor(ItemGrade grade)
    {
        switch (grade)
        {
            case ItemGrade.일반: return "#FFFFFF";     // 흰색
            case ItemGrade.고급: return "#1EFF00";  // 초록색
            case ItemGrade.희귀: return "#0070DD";      // 파랑색
            case ItemGrade.에픽: return "#A335EE";      // 보라색
            case ItemGrade.전설: return "#FF8000"; // 주황색
            case ItemGrade.신화: return "#FF0000";    // 빨간색
            default: return "#FFFFFF";                 // 기본 흰색
        }
    }

    public void ParseData(IList<object> row)
    {
        if (row.Count < 10) throw new Exception("아이템 데이터 부족");

        id = row[0].ToString();
        name = row[1].ToString();
        description = row[2].ToString();

        // Enum 파싱 (ItemType)
        type = Enum.TryParse(row[3].ToString(), out ItemType parsedType) ? parsedType : ItemType.소모품;

        // 아이템 등급 (장착 아이템만 적용)
        grade = Enum.TryParse(row[4].ToString(), out ItemGrade parsedGrade) ? parsedGrade : ItemGrade.일반;

        // 아이템 속성들
        costValue = int.TryParse(row[5].ToString(), out int cost) ? cost : 0;
        quantity = int.TryParse(row[6].ToString(), out int qty) ? qty : 1;
        maxStack = int.TryParse(row[7].ToString(), out int stack) ? stack : 99;

        isStackable = bool.TryParse(row[8].ToString(), out bool stackable) ? stackable : true;
        isDiscardable = bool.TryParse(row[9].ToString(), out bool discardable) ? discardable : true;

        // 장착 관련 속성 (무기, 방어구일 때만)
        if (type == ItemType.무기 || type == ItemType.방어구 || type == ItemType.장신구)
        {
            equipmentSlot = Enum.TryParse(row[10].ToString(), out EquipmentSlot parsedSlot) ? parsedSlot : EquipmentSlot.손;
            weaponType = Enum.TryParse(row[11].ToString(), out WeaponType parsedWeapon) ? parsedWeapon : WeaponType.한손무기;
            isEquired = bool.TryParse(row[12].ToString(), out bool equipped) ? equipped : false;
        }

        // 소모품 관련 속성
        if (type == ItemType.소모품 || type == ItemType.요리 || type == ItemType.요리재료)
        {
            consumableType = Enum.TryParse(row[13].ToString(), out ConsumableType parsedConsumable) ? parsedConsumable : ConsumableType.없음;
            effectAmount = int.TryParse(row[14].ToString(), out int effectVal) ? effectVal : 0;
        }

        // 퀘스트 아이템 여부
        if (type == ItemType.퀘스트)
        {
            isQuestItem = true;
            questId = row[15].ToString();
        }

        // 아이템 드랍 확률
        dropChance = float.TryParse(row[16].ToString(), out float dropRate) ? dropRate : 0f;

        // 아이템 스탯 (무기, 방어구, 장신구 등 장착 아이템에 적용)
        if (type == ItemType.무기 || type == ItemType.방어구 || type == ItemType.장신구)
        {
            itemStat = new ItemStat(
                int.TryParse(row[17].ToString(), out int str) ? str : 0,
                int.TryParse(row[18].ToString(), out int dex) ? dex : 0,
                int.TryParse(row[19].ToString(), out int intel) ? intel : 0,
                int.TryParse(row[20].ToString(), out int vit) ? vit : 0
            );

            itemStat.MaxHealth = int.TryParse(row[21].ToString(), out int mhp) ? mhp : 0;
            itemStat.MaxMana = int.TryParse(row[22].ToString(), out int mmp) ? mmp : 0;
            itemStat.PhysicalAttack = float.TryParse(row[23].ToString(), out float patk) ? patk : 0f;
            itemStat.MagicAttack = float.TryParse(row[24].ToString(), out float matk) ? matk : 0f;
            itemStat.PhysicalDefense = float.TryParse(row[25].ToString(), out float pdef) ? pdef : 0f;
            itemStat.MagicDefense = float.TryParse(row[26].ToString(), out float mdef) ? mdef : 0f;
            itemStat.CriticalChance = float.TryParse(row[27].ToString(), out float crit) ? crit : 0f;
            itemStat.AttackSpeed = float.TryParse(row[28].ToString(), out float atkSpd) ? atkSpd : 0f;
            itemStat.Evasion = float.TryParse(row[29].ToString(), out float evasion) ? evasion : 0f;
            itemStat.BlockChance = float.TryParse(row[30].ToString(), out float block) ? block : 0f;

            durability = new Durability(int.TryParse(row[31].ToString(), out int dura) ? dura : 100);
        }

        Debug.Log($"[Item] {name} 데이터 로드 완료!");
    }
}

[Serializable]
public class ItemStat
{
    [Header("기본 스탯")]
    public int Strength;          // 힘
    public int Dexterity;         // 민첩
    public int Intelligence;      // 지능
    public int Vitality;          // 활력
    //public int Luck;              // 운

    [Header("전투 스탯")]
    public int MaxHealth;         // 최대 체력
    public int MaxMana;           // 최대 마나
    public float PhysicalAttack;    // 물리 공격력
    public float MagicAttack;       // 마법 공격력
    public float PhysicalDefense;   // 물리 방어력
    public float MagicDefense;      // 마법 방어력

    [Header("보조 스탯")]
    public float CriticalChance;    // 치명타 확률 (%)
    public float AttackSpeed;       // 공격 속도
    public float Evasion;           // 회피율 (%)

    [Header("방패 확률")]
    public float BlockChance;     // 방어확률(방패 전용)

    [Header("요리용 회복 스탯")]
    public int HealHp;
    public int HealMp;

    [Header("요리 추가 버프 지속시간")]
    public float durationBonus;

    // 생성자
    public ItemStat(int strength, int dexterity, int intelligence, int vitality)
    {
        Strength = strength;
        Dexterity = dexterity;
        Intelligence = intelligence;
        Vitality = vitality;
    }

    // 스탯 초기화는 사용할때 사용하는걸로.
    public void Initialize()
    {
        MaxHealth = Vitality * 10;
        MaxMana = Intelligence * 5;
        PhysicalAttack = Strength * 2;
        MagicAttack = Intelligence * 2;
        PhysicalDefense = Vitality * 1;
        MagicDefense = Intelligence * 1;
        //CriticalChance = Luck / 2;
        AttackSpeed = Dexterity / 2;
        Evasion = Dexterity / 3;
    }

    // 복제 메서드
    public ItemStat Clone()
    {
        ItemStat newStat = new ItemStat(Strength, Dexterity, Intelligence, Vitality);

        newStat.MaxHealth = this.MaxHealth;
        newStat.MaxMana = this.MaxMana;
        newStat.PhysicalAttack = this.PhysicalAttack;
        newStat.MagicAttack = this.MagicAttack;
        newStat.PhysicalDefense = this.PhysicalDefense;
        newStat.MagicDefense = this.MagicDefense;

        newStat.CriticalChance = this.CriticalChance;
        newStat.AttackSpeed = this.AttackSpeed;
        newStat.Evasion = this.Evasion;

        newStat.BlockChance = this.BlockChance;

        newStat.HealHp = this.HealHp;
        newStat.HealMp = this.HealMp;

        return newStat;
    }

    //HJ 추가
    public ItemStat AddStats(ItemStat baseStat, ItemStat additionalStat)
    {
        // 새로운 ItemStat 객체를 생성하고, 스탯을 합산하여 반환
        ItemStat result = new ItemStat(
            baseStat.Strength + additionalStat.Strength,
            baseStat.Dexterity + additionalStat.Dexterity,
            baseStat.Intelligence + additionalStat.Intelligence,
            baseStat.Vitality + additionalStat.Vitality
        )
        {
            MaxHealth = baseStat.MaxHealth + additionalStat.MaxHealth,
            MaxMana = baseStat.MaxMana + additionalStat.MaxMana,
            PhysicalAttack = baseStat.PhysicalAttack + additionalStat.PhysicalAttack,
            MagicAttack = baseStat.MagicAttack + additionalStat.MagicAttack,
            PhysicalDefense = baseStat.PhysicalDefense + additionalStat.PhysicalDefense,
            MagicDefense = baseStat.MagicDefense + additionalStat.MagicDefense,
            CriticalChance = baseStat.CriticalChance + additionalStat.CriticalChance,
            AttackSpeed = baseStat.AttackSpeed + additionalStat.AttackSpeed,
            Evasion = baseStat.Evasion + additionalStat.Evasion,

            BlockChance = baseStat.BlockChance + additionalStat.BlockChance,

            HealHp = baseStat.HealHp + additionalStat.HealHp,
            HealMp = baseStat.HealMp + additionalStat.HealMp
        };

        return result;
    }

    //HJ 추가
    //아이템 효과 설명
    public string GetEffectDescription()
    {
        List<string> effects = new List<string>();

        if (HealHp > 0) effects.Add($"HP +{HealHp}");
        if (HealMp > 0) effects.Add($"MP +{HealMp}");

        if (Strength > 0) effects.Add($"힘 +{Strength}");
        if (Dexterity > 0) effects.Add($"민첩 +{Dexterity}");
        if (Intelligence > 0) effects.Add($"지능 +{Intelligence}");
        if (Vitality > 0) effects.Add($"활력 +{Vitality}");

        if (MaxHealth > 0) effects.Add($"최대 체력 +{MaxHealth}");
        if (MaxMana > 0) effects.Add($"최대 마나 +{MaxMana}");
        if (PhysicalAttack > 0) effects.Add($"물리 공격력 +{PhysicalAttack}");
        if (MagicAttack > 0) effects.Add($"마법 공격력 +{MagicAttack}");
        if (PhysicalDefense > 0) effects.Add($"물리 방어력 +{PhysicalDefense}");
        if (MagicDefense > 0) effects.Add($"마법 방어력 +{MagicDefense}");

        if (CriticalChance > 0) effects.Add($"치명타 확률 +{CriticalChance}%");
        if (AttackSpeed > 0) effects.Add($"공격 속도 +{AttackSpeed}");
        if (Evasion > 0) effects.Add($"회피율 +{Evasion}%");

        return effects.Count > 0 ? string.Join(", ", effects) : string.Empty;
    }

    //HJ 추가
    public bool HasBuffStat()
    {
        if (Strength > 0 || Dexterity > 0 || Intelligence > 0 || Vitality > 0 ||
            MaxHealth > 0 || MaxMana > 0 || PhysicalAttack > 0 || MagicAttack > 0 || PhysicalDefense > 0 || MagicDefense > 0 ||
            CriticalChance > 0 || AttackSpeed > 0 || Evasion > 0)
            return true;

        return false;
    }

    public void ParseData(IList<object> row)
    {
        if (row.Count < 11) throw new Exception("아이템 스탯 데이터 부족");

        Strength = int.TryParse(row[0].ToString(), out int str) ? str : 0;
        Dexterity = int.TryParse(row[1].ToString(), out int dex) ? dex : 0;
        Intelligence = int.TryParse(row[2].ToString(), out int intel) ? intel : 0;
        Vitality = int.TryParse(row[3].ToString(), out int vit) ? vit : 0;

        MaxHealth = int.TryParse(row[4].ToString(), out int mhp) ? mhp : 0;
        MaxMana = int.TryParse(row[5].ToString(), out int mmp) ? mmp : 0;
        PhysicalAttack = float.TryParse(row[6].ToString(), out float patk) ? patk : 0f;
        MagicAttack = float.TryParse(row[7].ToString(), out float matk) ? matk : 0f;
        PhysicalDefense = float.TryParse(row[8].ToString(), out float pdef) ? pdef : 0f;
        MagicDefense = float.TryParse(row[9].ToString(), out float mdef) ? mdef : 0f;

        Debug.Log("[ItemStat] 아이템 스탯 데이터 로드 완료!");
    }
}

// 아이템 타입 Enum
public enum ItemType
{
    무기,         // Weapon
    방어구,       // Armor
    소모품,       // Consumable
    퀘스트,       // QuestItem
    제작재료,     // Material
    장신구,        // Accessory
    요리재료,
    요리
}

public enum WeaponType
{
    한손무기,
    양손무기,
    완드
}

// 장착 위치 Enum
public enum EquipmentSlot
{
    방패,   // Shield
    머리,   // Head
    몸,     // Body
    손,     // Hand
    발,     // Feet
    장신구     // Accesorry
}

// 소모품 타입 Enum
public enum ConsumableType
{
    없음,       // None
    체력포션,   // HealthPotion
    마나포션,   // ManaPotion
    버프,        // Buff
}


// 아이템 등급 Enum
public enum ItemGrade
{
    일반,      // Common
    고급,      // Uncommon
    희귀,      // Rare
    에픽,      // Epic
    전설,      // Legendary
    신화       // Mythic
}
