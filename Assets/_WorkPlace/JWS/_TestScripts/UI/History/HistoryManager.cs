using System;
using System.Collections.Generic;
using System.Linq;

public class HistoryManager
{
    private List<HistoryLog> historyLogs;
    private const int LogsPerPage = 20;
    private string currentTags = "All";

    public HistoryManager()
    {
        historyLogs = new List<HistoryLog>();
    }

    public void AddHistory(string eventDescription, string eventType, int eventID)
    {
        HistoryLog newLog = new HistoryLog(eventDescription, eventType, eventID);
        historyLogs.Add(newLog);
        DisplayHistory(currentTags);
    }

    public List<HistoryLog> GetAllHistory()
    {
        return historyLogs;
    }

    public List<HistoryLog> GetHistoryByType(string eventType)
    {
        return historyLogs.Where(log => log.eventType == eventType).ToList();
    }

    public List<HistoryLog> GetHistoryByDateRange(DateTime start, DateTime end)
    {
        return historyLogs.Where(log => log.timestamp >= start && log.timestamp <= end).ToList();
    }

    public List<HistoryLog> GetSortedHistoryByTimestamp(bool ascending = true)
    {
        if (ascending)
        {
            return historyLogs.OrderBy(log => log.timestamp).ToList();
        }
        else
        {
            return historyLogs.OrderByDescending(log => log.timestamp).ToList();
        }
    }

    public HistoryLog? GetHistoryByID(int eventID)
    {
        return historyLogs.FirstOrDefault(log => log.eventID == eventID);
    }

    public void RemoveHistoryByID(int eventID)
    {
        historyLogs.RemoveAll(log => log.eventID == eventID);
    }

    public int GetTotalLogCount()
    {
        return historyLogs.Count;
    }

    public List<HistoryLog> GetHistoryByTagAndPage(string tag, int pageNumber)
    {
        var filteredLogs = historyLogs.Where(log => log.eventType == tag)
                                      .Skip((pageNumber - 1) * LogsPerPage)
                                      .Take(LogsPerPage)
                                      .ToList();

        return filteredLogs;
    }

    public string GetPagedTextByTag(string tag, int pageNumber)
    {
        var logs = GetHistoryByTagAndPage(tag, pageNumber);

        if (logs.Count == 0)
        {
            return $"No logs found for tag '{tag}' on page {pageNumber}.";
        }

        string pageText = string.Empty;
        foreach (var log in logs)
        {
            pageText += $"{log.ToString()}\n";
        }

        return pageText;
    }

    public List<HistoryLog> GetHistoryByIDAndPage(int eventID, int pageNumber)
    {
        var filteredLogs = historyLogs.Where(log => log.eventID == eventID)
                                      .Skip((pageNumber - 1) * LogsPerPage)
                                      .Take(LogsPerPage)
                                      .ToList();

        return filteredLogs;
    }

    public string GetPagedTextByID(int eventID, int pageNumber)
    {
        var logs = GetHistoryByIDAndPage(eventID, pageNumber);

        if (logs.Count == 0)
        {
            return $"No logs found for event ID '{eventID}' on page {pageNumber}.";
        }

        string pageText = string.Empty;
        foreach (var log in logs)
        {
            pageText += $"{log.ToString()}\n";
        }

        return pageText;
    }

    public int GetTotalPages()
    {
        return (int)Math.Ceiling((double)historyLogs.Count / LogsPerPage);
    }

    public string GetAllHistoryAsText()
    {
        string fullHistoryText = string.Empty;
        foreach (var log in historyLogs)
        {
            fullHistoryText += $"{log.ToString()}\n";
        }
        return fullHistoryText;
    }

    public void DisplayHistory(string tags)
    {
        //currentTags = tags.Trim();
        //if (currentTags != "All")
        //{
        //    GameManager.Instance._historyText.text = GetPagedTextByTag(currentTags, 1);
        //}
        //else
        //{
        //    GameManager.Instance._historyText.text = GetAllHistoryAsText();
        //}
        //Canvas.ForceUpdateCanvases();  // UI를 즉시 업데이트
        //GameManager.Instance._historyRect.verticalNormalizedPosition = 0f;  // 0은 맨 아래로 스크롤
    }

    private readonly Dictionary<MessageTag, string> tagColors = new Dictionary<MessageTag, string>
    {
        { MessageTag.Quest, "#FFD700" }, // 금색
        { MessageTag.PlayerDamage, "#FF0000" }, // 빨간색
        { MessageTag.EnemyDamage, "#00FF00" }, // 초록색
        { MessageTag.Other, "#1E90FF" }, // 파란색
        { MessageTag.PlayerHeal, "#32CD32" }, // 라임 그린
        { MessageTag.PlayerBuff, "#87CEEB" }, // 스카이 블루
        { MessageTag.PlayerDebuff, "#8B0000" }, // 다크 레드
        { MessageTag.CriticalHit, "#FF4500" }, // 오렌지 레드
        { MessageTag.Miss, "#708090" }, // 슬레이트 그레이
        { MessageTag.Dodge, "#00CED1" }, // 다크 터콰이즈
        { MessageTag.EnemySpawn, "#8A2BE2" }, // 보라색
        { MessageTag.EnemyDefeated, "#FFDAB9" }, // 피치
        { MessageTag.Warning, "#FFA500" }, // 오렌지
        { MessageTag.Error, "#DC143C" }, // 크림슨
        { MessageTag.SystemNotice, "#ADD8E6" }, // 라이트 블루
        { MessageTag.Event, "#FFC0CB" }, // 핑크
        { MessageTag.ItemPickup, "#FFD700" }, // 금색
        { MessageTag.ItemRare, "#9400D3" }, // 다크 바이올렛
        { MessageTag.GoldGain, "#FFFF00" }, // 노란색
        { MessageTag.AllyHeal, "#00FA9A" }, // 미디움 스프링 그린
        { MessageTag.AllyDamage, "#FF6347" }, // 토마토 레드
        { MessageTag.TeamBuff, "#4169E1" }, // 로얄 블루
        { MessageTag.SkillUse, "#6495ED" }, // 코넬 플라워 블루
        { MessageTag.MagicCast, "#9932CC" } // 다크 오키드
    };

    private string GetColorByTag(MessageTag tag)
    {
        if (tagColors.TryGetValue(tag, out string color))
        {
            return color;
        }
        return "#FFFFFF"; // 기본 흰색
    }
}

public enum MessageTag
{
    Quest,
    PlayerDamage,
    EnemyDamage,
    Other,
    PlayerHeal,
    PlayerBuff,
    PlayerDebuff,
    CriticalHit,
    Miss,
    Dodge,
    EnemySpawn,
    EnemyDefeated,
    Warning,
    Error,
    SystemNotice,
    Event,
    ItemPickup,
    ItemRare,
    GoldGain,
    AllyHeal,
    AllyDamage,
    TeamBuff,
    SkillUse,
    MagicCast
}
