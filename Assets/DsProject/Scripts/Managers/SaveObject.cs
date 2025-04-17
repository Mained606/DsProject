using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveObject : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject saveUICanvas; // 세이브 UI 캔버스
    [SerializeField] private SaveLoadUI saveUI; // 세이브 UI 컴포넌트
    [SerializeField] private float interactionRange = 2f; // 상호작용 범위
    [SerializeField] private string promptMessage = "E를 눌러 게임을 저장하세요"; // 프롬프트 메시지
    
    private bool isPlayerInRange = false;
    
    private void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            OnInteract();
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            ShowInteractionPrompt(true);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            ShowInteractionPrompt(false);
        }
    }
    
    public void OnInteract()
    {
        // 세이브 UI 활성화
        if (saveUICanvas != null && saveUI != null)
        {
            saveUICanvas.SetActive(true);
            saveUI.ShowUI();
        }
    }
    
    private void ShowInteractionPrompt(bool show)
    {
        // UI 매니저나 프롬프트 매니저를 통해 상호작용 프롬프트 표시
        // 예: UIManager.Instance.ShowInteractionPrompt(promptMessage, show);
    }
}

// 상호작용 인터페이스
public interface IInteractable
{
    void OnInteract();
} 