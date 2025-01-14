using System;

namespace JWS
{
    [Serializable]
    public class Item
    {
        public string id; // 아이템 고유 ID
        public string name; // 아이템 이름
        public string description; // 설명
        public ItemType type; // 아이템 타입
        public ItemGrade itemGrade; // 아이템 등급
        public int quantity; // 현재 소지 개수
        public bool isStackable; // 중첩 가능 여부
        public ItemStat itemStat; // 아이템 스탯

        // 생성자
        public Item(string id, string name, string description, ItemType type, int quantity, bool isStackable = true, ItemGrade grade = ItemGrade.일반)
        {
            this.id = id;
            this.name = name;
            this.description = description;
            this.type = type;
            this.itemGrade = grade;
            this.quantity = quantity;
            this.isStackable = isStackable;
            if (type == ItemType.무기아이템 || type == ItemType.방어아이템)
            {
                this.itemStat = new ItemStat(1,1,1,1,1);
            }
        }

        public Item CreateNew()
        {
            Item newItem = new Item(
                this.id,
                this.name,
                this.description,
                this.type,
                this.quantity,
                this.isStackable,
                this.itemGrade
            );
            if (this.itemStat != null)
            {
                newItem.itemStat = this.itemStat.Clone();
            }
            return newItem;
        }

        public string ToStringTMPro()
        {
            string itemName = $"<b>이름 : <color=#FFD700>{name}</color></b>"; // 황금색
            string itemType = $"종류 : <i><color=#87CEEB>{type}</color></i> 등급 : <i><color=#87CEEB>{itemGrade}</color></i>"; // 하늘색
            string itemDescription = $"<color=#FFFFFF>{description}</color>"; // 흰색
            string itemQuantity = $"수량: <color=#00FF00>{quantity}</color>"; // 초록색
            return $"{itemName}\n{itemType}\n{itemDescription}\n{itemQuantity}";
        }
    }

    [Serializable]
    public class ItemStat
    {
        // 기본 스탯
        public int Strength;          // 힘: 물리 공격력 및 체력 증가
        public int Dexterity;         // 민첩: 치명타 확률, 회피율, 공격 속도
        public int Intelligence;      // 지능: 마법 공격력 및 마나 양 증가
        public int Vitality;          // 활력: 최대 체력 및 방어력 증가
        public int Luck;              // 운: 치명타 확률, 드랍 확률 등 기타 효과

        // 전투 관련 스탯
        public int MaxHealth;         // 최대 체력
        public int MaxMana;           // 최대 마나
        public int PhysicalAttack;    // 물리 공격력
        public int MagicAttack;       // 마법 공격력
        public int PhysicalDefense;   // 물리 방어력
        public int MagicDefense;      // 마법 방어력

        // 보조 스탯
        public int CriticalChance;    // 치명타 확률 (%)
        public int AttackSpeed;       // 공격 속도
        public int Evasion;           // 회피율 (%)
        public int HealthRegen;       // 체력 재생 속도
        public int ManaRegen;         // 마나 재생 속도

        public ItemStat(int strength, int dexterity, int intelligence, int vitality, int luck)
        {
            Strength = strength;
            Dexterity = dexterity;
            Intelligence = intelligence;
            Vitality = vitality;
            Luck = luck;
            Initialize();
        }

        public ItemStat Clone()
        {
            ItemStat itemStat = new ItemStat(
                this.Strength, 
                this.Dexterity, 
                this.Intelligence, 
                this.Vitality, 
                this.Luck
            );
            return itemStat;
        }

        // 생성자
        public void Initialize()
        {
            // 기본 스탯을 기반으로 다른 스탯 계산 (예제 공식)
            MaxHealth = Vitality * 10;             // 활력 1당 체력 10 증가
            MaxMana = Intelligence * 5;            // 지능 1당 마나 5 증가
            PhysicalAttack = Strength * 2;         // 힘 1당 물리 공격력 2 증가
            MagicAttack = Intelligence * 2;        // 지능 1당 마법 공격력 2 증가
            PhysicalDefense = Vitality * 1;        // 활력 1당 물리 방어력 1 증가
            MagicDefense = Intelligence * 1;       // 지능 1당 마법 방어력 1 증가

            CriticalChance = Luck / 2;             // 운 2당 치명타 확률 1% 증가
            AttackSpeed = Dexterity / 2;           // 민첩 2당 공격 속도 증가
            Evasion = Dexterity / 3;               // 민첩 3당 회피율 증가
            HealthRegen = Vitality / 2;            // 활력 2당 체력 재생 증가
            ManaRegen = Intelligence / 3;          // 지능 3당 마나 재생 증가
        }

        public override string ToString()
        {
            return $"[기본 스탯]\n" +
                   $"힘: {Strength}, 민첩: {Dexterity}, 지능: {Intelligence}, 활력: {Vitality}, 운: {Luck}\n" +
                   $"[전투 스탯]\n" +
                   $"체력: {MaxHealth}, 마나: {MaxMana}, 물리 공격력: {PhysicalAttack}, 마법 공격력: {MagicAttack}, " +
                   $"물리 방어력: {PhysicalDefense}, 마법 방어력: {MagicDefense}\n" +
                   $"[보조 스탯]\n" +
                   $"치명타 확률: {CriticalChance}%, 공격 속도: {AttackSpeed}, 회피율: {Evasion}%, " +
                   $"체력 재생: {HealthRegen}, 마나 재생: {ManaRegen}";
        }
    }

    // 아이템 타입 Enum
    public enum ItemType
    {
        소비아이템,   // 소비형 (예: 포션)
        재료아이템,   // 재료형 (예: 비늘, 꽃)
        무기아이템,    // 장비형 (예: 검, 방어구)
        방어아이템,    // 장비형 (예: 검, 방어구)
        퀘스트아이템,    // 퀘스트용
        특수아이템    // 퀘스트용
    }

    public enum ItemGrade
    {
        일반,
        레어,
        유니크,
        레전드리
    }
}