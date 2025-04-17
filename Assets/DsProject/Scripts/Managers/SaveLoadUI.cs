using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveLoadUI : MonoBehaviour
{
    [SerializeField] private GameObject uiPanel; // UI 패널
    [SerializeField] private Transform slotContainer; // 슬롯 컨테이너
    [SerializeField] private SaveSlotUI slotPrefab; // 슬롯 프리팹
    [SerializeField] private Button closeButton; // 닫기 버튼
    [SerializeField] private Button loadButton; // 로드 버튼 (로드 UI에서만 사용)
    [SerializeField] private Button deleteButton; // 삭제 버튼
    [SerializeField] private Button saveButton; // 저장 버튼 (세이브 UI에서만 사용)
    
    [SerializeField] private Color emptySlotColor = Color.gray; // 빈 슬롯 색상
    [SerializeField] private Color selectedSlotColor = Color.yellow; // 선택된 슬롯 색상
    [SerializeField] private Color usedSlotColor = Color.white; // 사용 중인 슬롯 색상
    
    public enum UIMode { Save, Load }
    [SerializeField] private UIMode mode; // UI 모드
    
    private List<SaveSlotUI> slotUIs = new List<SaveSlotUI>();
    private int selectedSlotIndex = -1;
    private Dictionary<int, SaveSlotInfo> saveSlots;
    
    private void Awake()
    {
        // 버튼 이벤트 설정
        if (closeButton != null) closeButton.onClick.AddListener(CloseUI);
        if (loadButton != null) loadButton.onClick.AddListener(LoadSelectedSave);
        if (deleteButton != null) deleteButton.onClick.AddListener(DeleteSelectedSave);
        if (saveButton != null) saveButton.onClick.AddListener(SaveToSelectedSlot);
        
        // 초기 상태 설정
        if (loadButton != null) loadButton.interactable = false;
        if (deleteButton != null) deleteButton.interactable = false;
    }
    
    private void OnEnable()
    {
        // 세이브 매니저 이벤트 구독
        SaveManager.Instance.OnSaveSlotsChanged += RefreshUI;
        
        // UI 새로고침
        RefreshUI();
    }
    
    private void OnDisable()
    {
        // 세이브 매니저 이벤트 구독 해제
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.OnSaveSlotsChanged -= RefreshUI;
        }
    }
    
    public void ShowUI()
    {
        uiPanel.SetActive(true);
        RefreshUI();
    }
    
    public void CloseUI()
    {
        uiPanel.SetActive(false);
    }
    
    private void RefreshUI()
    {
        // 기존 슬롯 제거
        foreach (var slotUI in slotUIs)
        {
            Destroy(slotUI.gameObject);
        }
        slotUIs.Clear();
        
        // 세이브 슬롯 정보 가져오기
        saveSlots = SaveManager.Instance.GetAllSaveSlots();
        
        // 슬롯 생성
        for (int i = 1; i <= 3; i++)
        {
            SaveSlotUI slotUI = Instantiate(slotPrefab, slotContainer);
            slotUIs.Add(slotUI);
            
            int slotIndex = i; // 클로저를 위한 변수
            
            // 슬롯에 데이터가 있는 경우
            if (saveSlots.TryGetValue(i, out SaveSlotInfo slotInfo))
            {
                slotUI.SetSlotInfo(slotInfo);
                slotUI.SetSlotColor(usedSlotColor);
            }
            else
            {
                // 빈 슬롯
                slotUI.SetEmptySlot(i);
                slotUI.SetSlotColor(emptySlotColor);
            }
            
            // 슬롯 클릭 이벤트 설정
            slotUI.OnSlotSelected += () => SelectSlot(slotIndex - 1); // 인덱스는 0부터 시작하므로 -1
        }
        
        // 버튼 상태 업데이트
        UpdateButtonStates();
    }
    
    private void SelectSlot(int index)
    {
        if (index < 0 || index >= slotUIs.Count)
            return;
        
        // 이전에 선택된 슬롯 색상 원래대로
        if (selectedSlotIndex >= 0 && selectedSlotIndex < slotUIs.Count)
        {
            int realSlotNumber = selectedSlotIndex + 1;
            if (saveSlots.ContainsKey(realSlotNumber))
            {
                slotUIs[selectedSlotIndex].SetSlotColor(usedSlotColor);
            }
            else
            {
                slotUIs[selectedSlotIndex].SetSlotColor(emptySlotColor);
            }
        }
        
        // 새 슬롯 선택
        selectedSlotIndex = index;
        slotUIs[selectedSlotIndex].SetSlotColor(selectedSlotColor);
        
        // 버튼 상태 업데이트
        UpdateButtonStates();
    }
    
    private void UpdateButtonStates()
    {
        bool isSlotSelected = selectedSlotIndex >= 0;
        bool isSelectedSlotUsed = isSlotSelected && saveSlots.ContainsKey(selectedSlotIndex + 1);
        
        // 로드 버튼은 사용 중인 슬롯이 선택되었을 때만 활성화
        if (loadButton != null)
        {
            loadButton.interactable = isSelectedSlotUsed;
        }
        
        // 삭제 버튼은 사용 중인 슬롯이 선택되었을 때만 활성화
        if (deleteButton != null)
        {
            deleteButton.interactable = isSelectedSlotUsed;
        }
        
        // 저장 버튼은 슬롯이 선택되었을 때는 항상 활성화
        if (saveButton != null)
        {
            saveButton.interactable = isSlotSelected;
        }
    }
    
    private void LoadSelectedSave()
    {
        if (selectedSlotIndex >= 0 && saveSlots.ContainsKey(selectedSlotIndex + 1))
        {
            SaveManager.Instance.LoadGame(selectedSlotIndex + 1);
            CloseUI();
        }
    }
    
    private void DeleteSelectedSave()
    {
        if (selectedSlotIndex >= 0 && saveSlots.ContainsKey(selectedSlotIndex + 1))
        {
            // 확인 창 표시 (별도 구현 필요)
            ConfirmDeleteSave();
        }
    }
    
    private void ConfirmDeleteSave()
    {
        // 여기에 확인 창 표시 로직 구현 (별도 UI 클래스로 분리 가능)
        // 확인 시 실제 삭제 실행
        SaveManager.Instance.DeleteSave(selectedSlotIndex + 1);
    }
    
    private void SaveToSelectedSlot()
    {
        if (selectedSlotIndex >= 0)
        {
            int slotNumber = selectedSlotIndex + 1;
            
            // 이미 데이터가 있는 경우 확인 창 표시
            if (saveSlots.ContainsKey(slotNumber))
            {
                ConfirmOverwriteSave(slotNumber);
            }
            else
            {
                // 빈 슬롯이므로 바로 저장
                SaveManager.Instance.SaveGame(slotNumber);
                CloseUI();
            }
        }
    }
    
    private void ConfirmOverwriteSave(int slotNumber)
    {
        // 여기에 확인 창 표시 로직 구현 (별도 UI 클래스로 분리 가능)
        // 확인 시 실제 저장 실행
        SaveManager.Instance.SaveGame(slotNumber);
        CloseUI();
    }
} 