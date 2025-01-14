using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine;

public class HistoryManager
{
    private List<HistoryLog> historyLogs;  // 히스토리 로그를 저장하는 리스트
    private const int LogsPerPage = 20;    // 한 페이지에 출력할 로그 개수
    private string currentTags = "All";

    public HistoryManager()
    {
        historyLogs = new List<HistoryLog>();
    }

    // 히스토리 로그 추가
    public void AddHistory(string eventDescription, string eventType, int eventID)
    {
        HistoryLog newLog = new HistoryLog(eventDescription, eventType, eventID);
        historyLogs.Add(newLog);
        DisplayHistory(currentTags);
    }

    // 모든 히스토리 로그 조회
    public List<HistoryLog> GetAllHistory()
    {
        return historyLogs;
    }

    // 이벤트 유형에 따른 히스토리 로그 조회
    public List<HistoryLog> GetHistoryByType(string eventType)
    {
        return historyLogs.Where(log => log.eventType == eventType).ToList();
    }

    // 특정 기간 내의 히스토리 조회
    public List<HistoryLog> GetHistoryByDateRange(DateTime start, DateTime end)
    {
        return historyLogs.Where(log => log.timestamp >= start && log.timestamp <= end).ToList();
    }

    // 히스토리 로그 정렬 (시간순)
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

    // 특정 이벤트 ID로 히스토리 조회
    public HistoryLog? GetHistoryByID(int eventID)
    {
        return historyLogs.FirstOrDefault(log => log.eventID == eventID);
    }

    // 히스토리 로그 삭제
    public void RemoveHistoryByID(int eventID)
    {
        historyLogs.RemoveAll(log => log.eventID == eventID);
    }

    // 전체 히스토리 로그 수 가져오기
    public int GetTotalLogCount()
    {
        return historyLogs.Count;
    }

    // 특정 태그로 히스토리 로그를 필터링하고 페이지네이션으로 반환
    public List<HistoryLog> GetHistoryByTagAndPage(string tag, int pageNumber)
    {
        var filteredLogs = historyLogs.Where(log => log.eventType == tag)
                                      .Skip((pageNumber - 1) * LogsPerPage)
                                      .Take(LogsPerPage)
                                      .ToList();

        return filteredLogs;
    }

    // 태그별 로그를 줄바꿈으로 페이지별 출력하는 함수
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

    // 특정 ID로 히스토리 로그를 필터링하고 페이지네이션으로 반환
    public List<HistoryLog> GetHistoryByIDAndPage(int eventID, int pageNumber)
    {
        var filteredLogs = historyLogs.Where(log => log.eventID == eventID)
                                      .Skip((pageNumber - 1) * LogsPerPage)
                                      .Take(LogsPerPage)
                                      .ToList();

        return filteredLogs;
    }

    // ID별 로그를 줄바꿈으로 페이지별 출력하는 함수
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


    // 페이징할 수 있는 페이지 수 가져오기
    public int GetTotalPages()
    {
        return (int)Math.Ceiling((double)historyLogs.Count / LogsPerPage);
    }

    // 전체 로그를 텍스트로 반환 (스크롤뷰 안에 표시할 텍스트)
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

    // 태그에 따른 색상을 반환하는 함수
    private string GetColorByTag(string tag)
    {
        switch (tag)
        {
            case "Quest":
                return "#FFD700"; // 금색
            case "PlayerDamage":
                return "#FF0000"; // 빨간색
            case "EnemyDamage":
                return "#00FF00"; // 초록색
            case "Other":
                return "#1E90FF"; // 파란색
            default:
                return "#FFFFFF"; // 기본 흰색
        }
    }
}
