using System.Collections.Generic;
using UnityEngine;

public enum QuestType
{
    Main,
    Sub,
    History
}

public class TestQuestList : MonoBehaviour
{
    [SerializeField] public List<QuestTable> questList = new List<QuestTable>();

    private void Awake()
    {
        InitializeQuests();
    }

    /// <summary>
    /// 퀘스트 데이터 초기화
    /// </summary>
    private void InitializeQuests()
    {
  
    }

    /// <summary>
    /// 전체 퀘스트 리스트를 반환
    /// </summary>
    public List<QuestTable> GetQuestList()
    {
        return questList;
    }
}
