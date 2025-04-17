using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveSlotUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI slotNumberText;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI playerLevelText;
    [SerializeField] private TextMeshProUGUI saveDateText;
    [SerializeField] private TextMeshProUGUI playTimeText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Button slotButton;
    
    public event Action OnSlotSelected;
    
    private void Awake()
    {
        slotButton.onClick.AddListener(() => OnSlotSelected?.Invoke());
    }
    
    public void SetSlotInfo(SaveSlotInfo slotInfo)
    {
        slotNumberText.text = $"슬롯 {slotInfo.SlotNumber}";
        playerNameText.text = slotInfo.PlayerName;
        playerLevelText.text = $"레벨: {slotInfo.PlayerLevel}";
        saveDateText.text = slotInfo.SaveDateTime;
        playTimeText.text = $"플레이 시간: {slotInfo.GetFormattedPlayTime()}";
    }
    
    public void SetEmptySlot(int slotNumber)
    {
        slotNumberText.text = $"슬롯 {slotNumber}";
        playerNameText.text = "비어 있음";
        playerLevelText.text = "";
        saveDateText.text = "";
        playTimeText.text = "";
    }
    
    public void SetSlotColor(Color color)
    {
        backgroundImage.color = color;
    }
} 