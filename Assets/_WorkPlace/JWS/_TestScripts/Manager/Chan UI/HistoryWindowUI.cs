using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class HistoryWindowUI : MonoBehaviour
{
    [SerializeField] private Transform historyParent;
    [SerializeField] private GameObject tagWindow;
    [SerializeField] private TextMeshProUGUI historyLog;
    private MessageTag selectedTag = MessageTag.전체;

    private void Start()
    {
        selectedTag = MessageTag.전체;
        MessageTag[] tags = (MessageTag[])Enum.GetValues(typeof(MessageTag));
        for (int i = 0; i < tags.Length; i++)
        {
            GameObject go = Instantiate(tagWindow, historyParent);
            TextMeshProUGUI text = go.GetComponentInChildren<TextMeshProUGUI>();
            Button button = go.GetComponent<Button>();
            MessageTag currentTag = tags[i];
            button.onClick.AddListener(() => OnCLickButton(currentTag));
            text.text = currentTag.ToString();
        }

        DisplayHistory(selectedTag);
    }



    public void DisplayHistory(MessageTag tags = MessageTag.전체)
    {
        if (tags != MessageTag.전체)
        {
            historyLog.text = UIManager.HistoryManager.GetPagedTextByTag(tags, 1);
        }
        else
        {
            historyLog.text = UIManager.HistoryManager.GetAllHistoryAsText();
        }
        Canvas.ForceUpdateCanvases();
    }

    private void OnCLickButton(MessageTag tags)
    {
        selectedTag = tags;
        if (tags != MessageTag.전체)
        {
            historyLog.text = UIManager.HistoryManager.GetPagedTextByTag(tags, 1);
        }
        else
        {
            historyLog.text = UIManager.HistoryManager.GetAllHistoryAsText();
        }
    }
}
