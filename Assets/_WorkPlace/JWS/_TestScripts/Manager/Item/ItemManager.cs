using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace JWS
{
    public class ItemManager : BaseManager<ItemManager>
    {
        [SerializeField] private List<Item> itemDatabase = new List<Item>();

        public static List<Item> ItemDatabase => Instance.itemDatabase;

        protected override void Awake()
        {
            base.Awake();
            InitializeItemDatabase();
        }

        private void InitializeItemDatabase()
        {
            // 소비아이템
            itemDatabase.Add(new Item("item001", "회복 포션", "체력을 회복합니다.", ItemType.소비아이템, 10, true));
            itemDatabase.Add(new Item("item002", "마나 포션", "마나를 회복합니다.", ItemType.소비아이템, 5, true));
            itemDatabase.Add(new Item("item003", "힘의 물약", "일시적으로 힘을 증가시킵니다.", ItemType.소비아이템, 3, true));
            itemDatabase.Add(new Item("item004", "속도의 물약", "일시적으로 이동 속도를 증가시킵니다.", ItemType.소비아이템, 2, true));
            itemDatabase.Add(new Item("item005", "생명의 물약", "생명력을 회복합니다.", ItemType.소비아이템, 8, true));
            itemDatabase.Add(new Item("item006", "저항의 물약", "상태 이상 저항력을 증가시킵니다.", ItemType.소비아이템, 4, true));
            itemDatabase.Add(new Item("item007", "정화의 물약", "중독과 같은 상태 이상을 제거합니다.", ItemType.소비아이템, 3, true));
            itemDatabase.Add(new Item("item008", "폭발 물약", "적에게 큰 피해를 입히는 물약입니다.", ItemType.소비아이템, 2, true));
            itemDatabase.Add(new Item("item009", "독 물약", "적을 중독시키는 물약입니다.", ItemType.소비아이템, 6, true));
            itemDatabase.Add(new Item("item010", "체력의 물약", "체력을 대폭 회복합니다.", ItemType.소비아이템, 5, true));
            itemDatabase.Add(new Item("item011", "힘의 묘약", "힘을 영구적으로 증가시킵니다.", ItemType.소비아이템, 1, false));
            itemDatabase.Add(new Item("item012", "마력의 묘약", "마력을 영구적으로 증가시킵니다.", ItemType.소비아이템, 1, false));
            itemDatabase.Add(new Item("item013", "불멸의 묘약", "불멸의 효과를 제공합니다.", ItemType.소비아이템, 1, false));
            itemDatabase.Add(new Item("item014", "투명 물약", "일정 시간 동안 투명 상태를 유지합니다.", ItemType.소비아이템, 3, true));
            itemDatabase.Add(new Item("item015", "공격력 증가 물약", "공격력을 일시적으로 증가시킵니다.", ItemType.소비아이템, 4, true));
            itemDatabase.Add(new Item("item016", "방어력 증가 물약", "방어력을 일시적으로 증가시킵니다.", ItemType.소비아이템, 4, true));
            itemDatabase.Add(new Item("item017", "치유 포션", "즉각적으로 체력을 회복합니다.", ItemType.소비아이템, 10, true));
            itemDatabase.Add(new Item("item018", "속성 강화 물약", "특정 속성을 강화합니다.", ItemType.소비아이템, 3, true));
            itemDatabase.Add(new Item("item019", "신속 물약", "캐릭터의 행동 속도를 증가시킵니다.", ItemType.소비아이템, 2, true));
            itemDatabase.Add(new Item("item020", "전투 포션", "전투 능력을 강화합니다.", ItemType.소비아이템, 1, false));

            // 재료아이템
            itemDatabase.Add(new Item("item021", "용의 비늘", "희귀한 용의 비늘입니다.", ItemType.재료아이템, 1, true));
            itemDatabase.Add(new Item("item022", "수정 조각", "희귀한 수정 조각입니다.", ItemType.재료아이템, 1, true));
            itemDatabase.Add(new Item("item023", "유령의 눈", "유령으로부터 얻은 희귀한 재료입니다.", ItemType.재료아이템, 1, true));
            itemDatabase.Add(new Item("item024", "거미의 독", "거미로부터 채취한 독입니다.", ItemType.재료아이템, 1, true));
            itemDatabase.Add(new Item("item025", "늑대의 송곳니", "늑대의 날카로운 송곳니입니다.", ItemType.재료아이템, 1, true));
            itemDatabase.Add(new Item("item026", "불꽃의 결정", "불의 정령으로부터 얻은 결정입니다.", ItemType.재료아이템, 1, true));
            itemDatabase.Add(new Item("item027", "마법의 잉크", "마법 주문을 쓰는 데 필요한 잉크입니다.", ItemType.재료아이템, 1, true));
            itemDatabase.Add(new Item("item028", "고대 석판", "고대의 비밀이 새겨진 석판입니다.", ItemType.재료아이템, 1, false));
            itemDatabase.Add(new Item("item029", "은 조각", "은으로 만든 작은 조각입니다.", ItemType.재료아이템, 1, true));
            itemDatabase.Add(new Item("item030", "황금 가루", "희귀한 황금 가루입니다.", ItemType.재료아이템, 1, true));

            // 장착아이템
            itemDatabase.Add(new Item("item031", "철검", "기본적인 검입니다.", ItemType.무기아이템, 1, false));
            itemDatabase.Add(new Item("item032", "강철 방패", "적의 공격을 막아주는 강철 방패입니다.", ItemType.방어아이템, 1, false));
            itemDatabase.Add(new Item("item033", "가죽 갑옷", "기본적인 방어구입니다.", ItemType.방어아이템, 1, false));
            itemDatabase.Add(new Item("item034", "은검", "은으로 제작된 검입니다.", ItemType.무기아이템, 1, false));
            itemDatabase.Add(new Item("item035", "룬검", "마법의 룬이 새겨진 검입니다.", ItemType.무기아이템, 1, false));
            itemDatabase.Add(new Item("item036", "마법 망토", "마법 방어력을 증가시키는 망토입니다.", ItemType.방어아이템, 1, false));
            itemDatabase.Add(new Item("item037", "기사의 갑옷", "기사들이 사용하는 고급 방어구입니다.", ItemType.방어아이템, 1, false));
            itemDatabase.Add(new Item("item038", "전설의 검", "전설적인 전사들이 사용했던 검입니다.", ItemType.무기아이템, 1, false));
            itemDatabase.Add(new Item("item039", "고대 방패", "고대 문양이 새겨진 방패입니다.", ItemType.방어아이템, 1, false));
            itemDatabase.Add(new Item("item040", "마법 검", "마법 공격력을 가진 검입니다.", ItemType.무기아이템, 1, false));

            // 퀘스트아이템
            itemDatabase.Add(new Item("item041", "마법 보석", "희귀한 퀘스트 아이템입니다.", ItemType.퀘스트아이템, 1, false));
            itemDatabase.Add(new Item("item042", "보물 지도", "숨겨진 보물의 위치를 알려줍니다.", ItemType.퀘스트아이템, 1, false));
            itemDatabase.Add(new Item("item043", "황금 열쇠", "특정한 문을 열 수 있는 열쇠입니다.", ItemType.퀘스트아이템, 1, false));
            itemDatabase.Add(new Item("item044", "잃어버린 편지", "누군가에게 전달해야 할 편지입니다.", ItemType.퀘스트아이템, 1, false));
            itemDatabase.Add(new Item("item045", "잃어버린 반지", "귀중한 반지로 보입니다.", ItemType.퀘스트아이템, 1, false));
            itemDatabase.Add(new Item("item046", "고대 유물", "고대의 비밀을 담고 있는 유물입니다.", ItemType.퀘스트아이템, 1, false));
            itemDatabase.Add(new Item("item047", "영혼의 수정", "영혼이 깃든 희귀한 수정입니다.", ItemType.퀘스트아이템, 1, false));
            itemDatabase.Add(new Item("item048", "고대 열쇠", "고대 유적의 문을 여는 열쇠입니다.", ItemType.퀘스트아이템, 1, false));
            itemDatabase.Add(new Item("item049", "고블린의 손도끼", "고블린에게서 빼앗은 도끼입니다.", ItemType.퀘스트아이템, 1, false));
            itemDatabase.Add(new Item("item050", "전설의 증표", "전설적인 전사의 증표입니다.", ItemType.퀘스트아이템, 1, false));
        }

        public void AddItemLogic(string itemId, int quantity = 1)
        {
            var item = GetItemById(itemId);
            item.quantity = quantity;
            if (item != null)
            {
                InventoryManager.Instance.AddItemLogic(item);
            }
            else
            {
                Debug.LogWarning($"[ItemManager] 아이템 ID '{itemId}'를 데이터베이스에서 찾을 수 없습니다.");
            }
        }

        public void RemoveItemLogic(string itemId, int quantity = 1)
        {
            var item = GetItemById(itemId);
            if (item != null)
            {
                InventoryManager.Instance.RemoveItemLogic(item.id, quantity);
                Debug.Log($"[ItemManager] 아이템 '{item.name}' {quantity}개 제거 완료");
            }
            else
            {
                Debug.LogWarning($"[ItemManager] 아이템 ID '{itemId}'를 데이터베이스에서 찾을 수 없습니다.");
            }
        }


        public Item GetItemById(string itemId)
        {
            var item = itemDatabase.FirstOrDefault(i => i.id == itemId);
            if (item == null)
            {
                Debug.LogWarning($"[ItemManager] ID: {itemId} 아이템을 찾을 수 없습니다.");
            }
            return item;
        }

        public void HandleStateChange()
        {
            Debug.Log("[ItemManager] 상태 변화에 따른 아이템 관련 로직 실행");
        }

        protected override void HandleGameStateChange(global::GameSystemState newState, object additionalData)
        {

        }
    }
}