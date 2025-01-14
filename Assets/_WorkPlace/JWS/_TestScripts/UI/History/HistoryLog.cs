using System;

public struct HistoryLog
{
    public DateTime timestamp;  // 이벤트 발생 시간
    public string eventDescription;  // 이벤트 내용
    public string eventType;  // 이벤트 유형(예: 전투, 상호작용 등)
    public int eventID;  // 이벤트 고유 ID (필요한 경우)

    // 생성자
    public HistoryLog(string eventDescription, string eventType, int eventID)
    {
        this.timestamp = DateTime.Now;
        this.eventDescription = eventDescription;
        this.eventType = eventType;
        this.eventID = eventID;
    }

    // 출력 포맷
    public override string ToString()
    {
        string _color = GetColorByTag(eventType);
        return $"<mspace=0.5em>{timestamp.ToString("HH:mm:ss")}</mspace>    <color={_color}>{eventDescription}</color>";
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
            case "System":
                return "#D3D3D3"; // 옅은 그레이
            case "Other":
                return "#1E90FF"; // 파란색
            default:
                return "#FFFFFF"; // 기본 흰색
        }
    }
}
