using UnityEngine;
using System.Collections;
using System.Linq;

namespace DsProject.Scripts.System.Save
{
    /// <summary>
    /// 퀘스트 상태에 따라 특정 오브젝트의 활성화/비활성화 상태를 관리하는 스크립트
    /// </summary>
    public class QuestObjectStateHandler : MonoBehaviour
    {
        [Header("퀘스트 관련 오브젝트")]
        [SerializeField] private GameObject dragonEgg;
        [SerializeField] private GameObject dragonNPC;
        [SerializeField] private GameObject playerDragon;
        
        [Header("설정")]
        [SerializeField] private float initDelay = 0.5f;
        [SerializeField] private bool debugMode = false;
        
        private void Start()
        {
            // 게임 로드 이벤트 구독
            SaveManager.OnGameLoaded += OnGameLoaded;
            
            // 게임 시작시 한 번 실행하여 상태 초기화
            StartCoroutine(InitializeObjectStates());
        }
        
        private void OnDestroy()
        {
            // 이벤트 구독 해제
            SaveManager.OnGameLoaded -= OnGameLoaded;
        }
        
        private void OnGameLoaded(SaveData saveData)
        {
            // 게임 로드 시 오브젝트 상태 초기화
            StartCoroutine(InitializeObjectStates());
        }
        
        private IEnumerator InitializeObjectStates()
        {
            // 퀘스트 매니저가 로드될 때까지 약간의 지연
            yield return new WaitForSeconds(initDelay);
            
            // 퀘스트 완료 상태에 따라 오브젝트 상태 설정
            UpdateObjectStates();
        }
        
        private void UpdateObjectStates()
        {
            // QuestManager가 없거나 초기화되지 않은 경우 처리 중단
            if (QuestManager.Instance == null)
            {
                LogDebug("QuestManager가 초기화되지 않았습니다.");
                return;
            }
            
            // 완료된 퀘스트 목록
            var completedQuests = QuestManager.CompletedQuests;
            if (completedQuests == null)
            {
                LogDebug("완료된 퀘스트 목록이 null입니다.");
                return;
            }
            
            // 퀘스트 완료 상태 확인
            bool isQuest1_1002Completed = completedQuests.Any(q => q.id == "1_1002");
            bool isQuest1_1003Completed = completedQuests.Any(q => q.id == "1_1003");
            bool isQuest1_1004Completed = completedQuests.Any(q => q.id == "1_1004");
            
            LogDebug($"퀘스트 상태: 1_1002({isQuest1_1002Completed}), 1_1003({isQuest1_1003Completed}), 1_1004({isQuest1_1004Completed})");
            
            // 용의알 상태 업데이트
            if (dragonEgg != null)
            {
                // 1_1002 완료 && 1_1003 미완료: 용의알 활성화
                // 1_1003 완료: 용의알 비활성화
                bool shouldActivate = isQuest1_1002Completed && !isQuest1_1003Completed;
                
                if (dragonEgg.activeSelf != shouldActivate)
                {
                    LogDebug($"용의알 상태 변경: {dragonEgg.activeSelf} -> {shouldActivate}");
                    dragonEgg.SetActive(shouldActivate);
                }
            }
            else
            {
                LogDebug("용의알 참조가 설정되지 않았습니다.");
            }
            
            // 용(NPC) 상태 업데이트
            if (dragonNPC != null)
            {
                // 1_1003 완료 && 1_1004 미완료: 용(NPC) 활성화
                // 1_1004 완료: 용(NPC) 비활성화
                bool shouldActivate = isQuest1_1003Completed && !isQuest1_1004Completed;
                
                if (dragonNPC.activeSelf != shouldActivate)
                {
                    LogDebug($"용(NPC) 상태 변경: {dragonNPC.activeSelf} -> {shouldActivate}");
                    dragonNPC.SetActive(shouldActivate);
                }
            }
            else
            {
                LogDebug("용(NPC) 참조가 설정되지 않았습니다.");
            }
            
            // 플레이어 드래곤 상태 업데이트
            if (playerDragon != null)
            {
                // 1_1004 완료: 플레이어 드래곤 활성화
                bool shouldActivate = isQuest1_1004Completed;
                
                if (playerDragon.activeSelf != shouldActivate)
                {
                    LogDebug($"플레이어 드래곤 상태 변경: {playerDragon.activeSelf} -> {shouldActivate}");
                    playerDragon.SetActive(shouldActivate);
                }
            }
            else
            {
                LogDebug("플레이어 드래곤 참조가 설정되지 않았습니다.");
            }
        }
        
        private void LogDebug(string message)
        {
            if (debugMode)
            {
                Debug.Log($"[QuestObjectStateHandler] {message}");
            }
        }
        
        // Unity Editor에서 Awake 이후 참조 설정을 위한 메서드
        public void SetReferences(GameObject egg, GameObject npc, GameObject dragon)
        {
            dragonEgg = egg;
            dragonNPC = npc;
            playerDragon = dragon;
        }
    }
} 