using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SavePoint : MonoBehaviour
{
    [Header("세이브 포인트 설정")]
    [Tooltip("세이브 포인트의 고유 ID")]
    public string savePointId = "SavePoint_01";
    
    [Tooltip("세이브 가능 메시지")]
    public string saveMessage = "F키를 눌러 저장";
    
    [Header("UI 설정")]
    [Tooltip("메시지를 표시할 UI 텍스트")]
    public TextMeshProUGUI messageText;
    
    [Tooltip("UI 오브젝트")]
    public GameObject uiObject;
    
    private bool playerInRange = false;
    
    private void Start()
    {
        // UI 초기화
        if (uiObject != null)
        {
            uiObject.SetActive(false);
        }
    }
    
    private void Update()
    {
        // 플레이어가 범위 내에 있고 F키를 누르면 저장
        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            if (SaveManager.Instance != null)
            {
                // 세이브 메시지 업데이트
                if (messageText != null)
                {
                    messageText.text = "저장 완료!";
                    
                    // 2초 후 메시지 복원
                    StartCoroutine(RestoreMessageAfterDelay(2f));
                }
            }
        }
    }
    
    private IEnumerator RestoreMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (messageText != null && playerInRange)
        {
            messageText.text = saveMessage;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // 플레이어인지 확인
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            
            // SaveManager에 알림
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.OnPlayerEnterSavePoint(this);
            }
            
            // UI 표시
            if (uiObject != null)
            {
                uiObject.SetActive(true);
            }
            
            // 메시지 설정
            if (messageText != null)
            {
                messageText.text = saveMessage;
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        // 플레이어인지 확인
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            
            // SaveManager에 알림
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.OnPlayerExitSavePoint();
            }
            
            // UI 숨김
            if (uiObject != null)
            {
                uiObject.SetActive(false);
            }
        }
    }
} 