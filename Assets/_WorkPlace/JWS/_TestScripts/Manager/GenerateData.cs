using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class GenerateData
{
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// 퀘스트 관련 제너레이터.
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public void InitializeItems(ItemList ItemDatabase)
    {
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// 퀘스트 관련 제너레이터.
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public List<Quest> GenerateMainQuestLists()
    {
        List<Quest> questList = new List<Quest>
        {
            // 1장: 용사의 마지막 숨결
            //new Quest("메인퀘스트", "quest101", "용사의 마지막 숨결",
            //    "용사의 마지막 전투를 목격하고, 남긴 검의 빛을 확인하세요.",
            //    new Dictionary<string, QuestCondition>
            //    {
            //        { "location_001", new QuestCondition(QuestConditionType.Explore, "location_001", "용사의 마지막 전투 장소", 1) }
            //    },
            //    new List<Reward>
            //    {
            //        new Reward("소형 체력포션", 1, 10, 10),
            //    }),

            // 1장 - 1: 마울로 향해
            /*new Quest("메인퀘스트", "quest101-1", "마을을 찾아가기",
                "마을을 찾아 이동하기.",
                new Dictionary<string, QuestCondition>
                {
                    { "location_0011", new QuestCondition(QuestConditionType.Explore, "location_0011", "마을을 찾아가기", 1) }
                },
                new List<Reward> { new Reward("소형 체력포션", 1, 10, 10) }),

            // 2장: 소년과 숲 속의 발견
            new Quest("메인퀘스트", "quest102", "숲 속의 알",
                "숲에서 딸기를 모으고, 이상한 소리를 따라 알을 발견하세요.",
                new Dictionary<string, QuestCondition>
                {
                    { "location_002", new QuestCondition(QuestConditionType.Explore, "location_002", "딸기밭 찾아가기", 1) },
                    { "딸기", new QuestCondition(QuestConditionType.Collect, "딸기", "딸기", 10) },
                    { "location_0022", new QuestCondition(QuestConditionType.Explore, "location_0022", "이상한 소리가 나는 장소", 1) },
                    { "용의알", new QuestCondition(QuestConditionType.Collect, "용의알", "용의 알", 1) }
                },
                new List<Reward> { new Reward("소형 체력포션", 1, 15, 15) }),

            // 3장: 알에서 태어난 생명체
            //new Quest("메인퀘스트", "quest103", "알의 신비",
            //    "알에서 깨어난 생명체를 몰래 숨길곳으로 데려가세요.",
            //    new Dictionary<string, QuestCondition>
            //    {
            //        { "location_003", new QuestCondition(QuestConditionType.Explore, "location_003", "알의 은신처", 1) }
            //    },
            //    new List<Reward> { new Reward("소형 체력포션", 1, 20, 20) }),

            // 4장: 소년과 생물체의 우정
            new Quest("메인퀘스트", "quest104", "신비한 동물의 도움",
                "숲 속에서 나무를 패고 생물체의 도움을 확인하세요.",
                new Dictionary<string, QuestCondition>
                {
                    { "location_004", new QuestCondition(QuestConditionType.Explore, "location_004", "숲 속의 나무", 1) },
                    { "나뭇가지", new QuestCondition(QuestConditionType.Collect, "나뭇가지", "나뭇가지", 5) }
                },
                new List<Reward> { new Reward("소형 체력포션", 1, 30, 30) }),

            // 5장: 하급 마족의 위협
            new Quest("메인퀘스트", "quest105", "엄마를 지켜라",
                "하급 마족과의 첫 전투에서 승리하고 생존하세요.",
                new Dictionary<string, QuestCondition>
                {
                    { "location_Golem", new QuestCondition(QuestConditionType.Explore, "location_Golem", "하급 마족", 1) },
                    { "Golem", new QuestCondition(QuestConditionType.Kill, "Golem", "하급 관리자", 3) }
                },
                new List<Reward> { new Reward("소형 체력포션", 1, 40, 50) }),

            // 6장: 숲 속에서 힘을 연마하다
            new Quest("메인퀘스트", "quest106", "새로운 힘",
                "숲 속에서 나무와 돌을 공격하며 힘을 연마하세요.",
                new Dictionary<string, QuestCondition>
                {
                    { "location_004", new QuestCondition(QuestConditionType.Explore, "location_004", "숲 속의 나무2", 1) },
                    { "돌", new QuestCondition(QuestConditionType.Collect, "돌", "돌", 5) },
                    { "나무", new QuestCondition(QuestConditionType.Collect, "나무", "나무", 5) }
                },
                new List<Reward> { new Reward("소형 체력포션", 0, 50, 20) }),

            // 7장: 엄마의 복수를 실행해라
            new Quest("메인퀘스트", "quest107", "홀로서기",
                "엄마를 살해한 하급 관리자 3명을 처치하세요.",
                new Dictionary<string, QuestCondition>
                {
                    { "location_Golem", new QuestCondition(QuestConditionType.Explore, "location_Golem", "하급 관리자", 1) },
                    { "Golem", new QuestCondition(QuestConditionType.Kill, "Golem", "하급 관리자", 3) }
                },
                new List<Reward> { new Reward("소형 체력포션", 1, 60, 60) }),

            // 8장: 마을의 위험에 대비하라
            new Quest("메인퀘스트", "quest108", "강해지기 위한 연습",
                "숲 속에서 위험구역을 통해 전투 능력을 강화하세요.",
                new Dictionary<string, QuestCondition>
                {
                    { "location_001", new QuestCondition(QuestConditionType.Explore, "location_001", "위험구역 찾기", 1) }
                },
                new List<Reward> { new Reward("소형 체력포션", 0, 70, 70) }),

            // 9장: 마족과의 전투
            new Quest("메인퀘스트", "quest109", "마을을 지켜라",
                "중급 관리자 모파안과 전투하세요.",
                new Dictionary<string, QuestCondition>
                {
                    { "Golem", new QuestCondition(QuestConditionType.Kill, "Golem", "하급 관리자", 3) },
                    { "Mophan", new QuestCondition(QuestConditionType.Kill, "Mophan", "중급 관리자 모파안", 1) }
                },
                new List<Reward> { new Reward("소형 체력포션", 1, 80, 80) }),

            // 10장: 새로운 여정의 시작
            new Quest("메인퀘스트", "quest110", "마왕을 찾아서",
                "마을 사람들과 대화하여 마왕의 정보를 수집하고 여정을 시작하세요.",
                new Dictionary<string, QuestCondition>
                {
                    { "location_006", new QuestCondition(QuestConditionType.Explore, "location_006", "마왕의 성에 대하여", 1) }
                },
                new List<Reward>
                {
                    new Reward("소형 체력포션", 1, 90, 90),
                    new Reward("소형 체력포션", 1, 90, 90)
                })*/

            //지혜 퀘스트
            new Quest("메인퀘스트", "1_1001", "엄마의 쪽지","옆집 아저씨에게 대화 걸기",
                new Dictionary<string, QuestCondition>
                {
                    // {"location_001", new QuestCondition(QuestConditionType.Explore, "location_001", "옆집 아저씨에게 대화 걸기", 1) },
                    { "에드릭", new QuestCondition(QuestConditionType.Meet, "MainNPC_1", "옆집 아저씨에게 대화 걸기", 1)}
                },
                new List<Reward> {new Reward("마을 지도", 1, 0, 0)}, true),

            new Quest("메인퀘스트", "1_1002", "엄마의 쪽지", "밭에서 야채를 수확하자",
                new Dictionary<string, QuestCondition>
                {
                    {"location_002", new QuestCondition(QuestConditionType.Explore, "location_002", "밭 찾기", 1) },
                    { "비트", new QuestCondition(QuestConditionType.Collect, "비트", "비트", 3) },
                    { "무", new QuestCondition(QuestConditionType.Collect, "무", "무", 3) },
                },
                new List <Reward> {new Reward("",0,10,0) }),

            new Quest("메인퀘스트", "1_1003", "엄마의 쪽지","의문의 소리 찾기",
                new Dictionary<string, QuestCondition>
                {
                    {"location_003", new QuestCondition(QuestConditionType.Explore, "location_003", "용의 알 찾기", 1) },
                    {"용의알", new QuestCondition(QuestConditionType.Collect, "용의알", "용의알",1) }
                },
                new List<Reward>{new Reward("", 0, 10,0)}),

            new Quest("메인퀘스트", "1_1004", "엄마의 쪽지","용에게 말을 걸자",
                new Dictionary<string, QuestCondition>
                {
                    {"용", new QuestCondition(QuestConditionType.Meet, "MainNPC_4", "용에게 말을 걸자",1) }
                },
                new List<Reward>
                {
                    // new Reward("스킬(FireStrike)", 1, 30, 0),
                    new Reward("낡은검", 1, 0, 0),
                    new Reward("완드", 1, 0, 0),
                }, true),

            #region 삭제된 메인 퀘스트
            /*new Quest("메인퀘스트", "1_1005", "알에서 나온 친구","옷장을 열자",
                new Dictionary<string, QuestCondition>
                {
                    {"location_002_meet", new QuestCondition(QuestConditionType.Meet, "Quest1_05", "-",1) }
                },
                new List<Reward>{new Reward("", 0, 0,0)}),

            new Quest("메인퀘스트", "1_1006", "알에서 나온 친구","도끼를 챙겨 밖으로 나가자",
                new Dictionary<string, QuestCondition>
                {
                    {"location_002_collect", new QuestCondition(QuestConditionType.Collect, "도끼", "도끼",1) }
                },
                new List<Reward>{new Reward("", 0, 10,0)}),

            new Quest("메인퀘스트", "1_1007", "알에서 나온 친구","벌목장에서 나무를 캐자",
                new Dictionary<string, QuestCondition>
                {
                    {"location_002_collect", new QuestCondition(QuestConditionType.Collect, "나무조각", "나무조각",6) }
                },
                new List<Reward>{new Reward("나무 완드", 1, 10,0)}),

            new Quest("메인퀘스트", "1_1008", "알에서 나온 친구","집으로 돌아가자",
                new Dictionary<string, QuestCondition>
                {
                    {"location_002_meet", new QuestCondition(QuestConditionType.Meet, "Quest1_05", "-",1) }
                },
                new List<Reward>{new Reward("", 0, 0,0)}),

            new Quest("메인퀘스트", "1_1009", "마족의 위협","집으로 들어가자",
                new Dictionary<string, QuestCondition>
                {
                    {"location_002_meet", new QuestCondition(QuestConditionType.Meet, "Quest1_05", "-",1) }
                },
                new List<Reward>{new Reward("", 0, 0,0)}),

            new Quest("메인퀘스트", "1_1010", "마족의 위협","정원으로 나가자",
                new Dictionary<string, QuestCondition>
                {
                    {"location_002_meet", new QuestCondition(QuestConditionType.Meet, "Quest1_05", "-",1) }
                },
                new List<Reward>
                {
                    new Reward("수련용 검", 1, 10, 0),
                    new Reward("체력 물약(소)", 1, 0, 0),
                    new Reward("마나 물약(소)", 1, 0, 0),
                    new Reward("스킬명", 1, 0, 0)
                }),

            new Quest("메인퀘스트", "1_1011", "새로운 힘","스킬을 활용하여 몬스터를 처치하자",
                new Dictionary<string, QuestCondition>
                {
                    {"location_002_kill", new QuestCondition(QuestConditionType.Kill, "몬스터id", "몬스터를 처치하자",10) }
                },
                new List<Reward>{new Reward("스킬명", 1, 20,20)}),

            new Quest("메인퀘스트", "1_1012", "새로운 힘","몬스터를 처치하여 힘을 키우자",
                new Dictionary<string, QuestCondition>
                {
                    {"location_002_level", new QuestCondition(QuestConditionType.Level, "레벨", "레벨 달성하기",5) }
                },
                new List<Reward>{new Reward("스킬명", 1, 20,20)}),

            new Quest("메인퀘스트", "1_1013", "새로운 힘","집으로 돌아가자",
                new Dictionary<string, QuestCondition>
                {
                    {"location_002_meet", new QuestCondition(QuestConditionType.Meet, "Quest1_05", "-",1) }
                },
                new List<Reward>{new Reward("", 0, 0,0)}),

            new Quest("메인퀘스트", "1_1014", "홀로서기","가족의 복수를 하자",
                new Dictionary<string, QuestCondition>
                {
                    {"location_002_kill", new QuestCondition(QuestConditionType.Kill, "하급마족id", "하급 마족을 처치하자",3) }
                },
                new List<Reward>{new Reward("", 0, 0,0)}),

            new Quest("메인퀘스트", "1_1015", "홀로서기","이장님과 대화하자",
                new Dictionary<string, QuestCondition>
                {
                    {"location_002_meet", new QuestCondition(QuestConditionType.Meet, "Quest1_05", "-",1) }
                },
                new List<Reward>
                {
                    new Reward("대검", 1, 50,100),
                    new Reward("완드", 1, 0, 0),
                    new Reward("가죽장갑", 1, 0, 0),
                    new Reward("가죽갑옷", 1, 0, 0),
                    new Reward("스킬명", 1, 0, 0),
                }),

            new Quest("메인퀘스트", "1_1016", "마을의 수호자","강해지기 위한 연습",
                new Dictionary<string, QuestCondition>
                {
                    {"location_002_level", new QuestCondition(QuestConditionType.Level, "레벨", "레벨 달성하기",15) }
                },
                new List<Reward>{new Reward("스킬명", 1, 20,20)}),

            new Quest("메인퀘스트", "1_1017", "마을의 수호자","강해지기 위한 연습",
                new Dictionary<string, QuestCondition>
                {
                    {"location_002_level", new QuestCondition(QuestConditionType.Level, "레벨", "레벨 달성하기",25) }
                },
                new List<Reward>{new Reward("", 0, 0,0)}),

            new Quest("메인퀘스트", "1_1018", "마을의 수호자","마을로 돌아가자",
                new Dictionary<string, QuestCondition>
                {
                    {"location_002_meet", new QuestCondition(QuestConditionType.Meet, "Quest1_05", "-",1) }
                },
                new List<Reward>{new Reward("", 0, 0,0)}),

            new Quest("메인퀘스트", "1_1019", "마을의 수호자","모파안의 부하들을 처치하자",
                new Dictionary<string, QuestCondition>
                {
                    {"location_002_kill", new QuestCondition(QuestConditionType.Kill, "부하id", "부하들을 처치하자",3) }
                },
                new List<Reward>{new Reward("", 0, 0,0)}),

            new Quest("메인퀘스트", "1_1020", "마을의 수호자","모파안과의 전투에서 승리하자",
                new Dictionary<string, QuestCondition>
                {
                    {"location_002_kill", new QuestCondition(QuestConditionType.Kill, "Mophan", "모파안을 처치하자",1) }
                },
                new List<Reward>
                {
                    new Reward("마족의 증표", 1, 50,100),
                    new Reward("마족의 지도", 1, 0, 0)                   
                })*/
#endregion
        };
        return questList;
    }

    public List<Quest> GenerateQuestLists()
    {
        List<Quest> questList = new List<Quest>
        {
            /*new Quest("서브퀘스트", "소형 체력포션", "회복 포션 수집",
                "회복 포션 10개를 수집하세요.",
                new Dictionary<string, QuestCondition>
                {
                    { "소형 체력포션", new QuestCondition(QuestConditionType.Collect, "소형 체력포션", "회복 포션 수집", 10) }
                },
                new List<Reward>
                {
                    new Reward("소형 체력포션", 10, 100, 50)
                }),*/

            new Quest("서브퀘스트", "1_2001", "모험가의 길","대장장이가 하는 일이 궁금한가?",
                new Dictionary<string, QuestCondition>
                {
                    {"location_002_meet", new QuestCondition(QuestConditionType.Meet, "Quest1_05", "-",1) }
                },
                new List<Reward>{new Reward("강화석 조각", 5, 0, 50)}),

            new Quest("서브퀘스트", "1_2002", "모험가의 길","대장장이가 하는 일이 궁금한가?",
                new Dictionary<string, QuestCondition>
                {
                    {"location_002_collect", new QuestCondition(QuestConditionType.Collect, "강화무기id", "무기 강화 성공!",1) },
                    {"location_002_meet", new QuestCondition(QuestConditionType.Meet, "Quest1_05", "대장장이에게 말을 걸자",1) }
                },
                new List<Reward>{new Reward("강화석", 1, 50, 50)}),

            new Quest("서브퀘스트", "1_2003", "모험가의 길","요리 방법이 궁금하다고?",
                new Dictionary<string, QuestCondition>
                {
                    {"location_002_meet", new QuestCondition(QuestConditionType.Meet, "Quest1_06", "-",1) }
                },
                new List<Reward>{new Reward("", 0, 0, 0)}),

            new Quest("서브퀘스트", "1_2004", "모험가의 길","요리 방법이 궁금하다고?",
                new Dictionary<string, QuestCondition>
                {
                    {"location_002_collect", new QuestCondition(QuestConditionType.Collect, "야채스프", "야채 스프 만들기",1) },
                    {"location_002_meet", new QuestCondition(QuestConditionType.Meet, "Quest1_05", "요리사에게 말을 걸자",1) }
                },
                new List<Reward>{new Reward("버섯", 3, 50, 50)}),

            new Quest("서브퀘스트", "1_2005", "친절한 용사","밭을 망치는 머쉬룸을 잡아줘!",
                new Dictionary<string, QuestCondition>
                {
                    {"location_002_kill", new QuestCondition(QuestConditionType.Kill, "Mushroom", "머쉬룸을 처치하자",10) },
                    {"location_002_meet", new QuestCondition(QuestConditionType.Meet, "Quest1_02", "수배지를 확인하자",1) }
                },
                new List<Reward>{new Reward("글라이딩", 1, 50,50)}),

            new Quest("서브퀘스트", "1_2006", "친절한 용사","내 장난감을 찾아줘",
                new Dictionary<string, QuestCondition>
                {
                    {"location_002_collect", new QuestCondition(QuestConditionType.Collect, "장난감id", "장난감을 찾자",1) },
                    {"location_002_meet", new QuestCondition(QuestConditionType.Meet, "Quest1_07", "소녀에게 장난감을 가져다주자.",1) }
                },
                new List<Reward>{new Reward("요리", 1, 50,0)}),

            new Quest("서브퀘스트", "1_2007", "친절한 용사","골렘을 잡아줘!",
                new Dictionary<string, QuestCondition>
                {
                    {"location_002_kill", new QuestCondition(QuestConditionType.Kill, "Golem", "골렘을 처치하자",10) },
                    {"location_002_meet", new QuestCondition(QuestConditionType.Meet, "Quest1_03", "수배지를 확인하자",1) }
                },
                new List<Reward>{new Reward("스킬(얼음)", 1, 50,50)}),

            new Quest("서브   퀘스트", "1_2008", "친절한 용사","협곡의 보물",
                new Dictionary<string, QuestCondition>
                {
                    {"location_002_kill", new QuestCondition(QuestConditionType.Kill, "보스", "보스를 처치하자",1) },
                    {"location_002_collect", new QuestCondition(QuestConditionType.Collect, "보물id", "상자를 찾자",1) },
                    {"location_002_meet", new QuestCondition(QuestConditionType.Meet, "Quest1_04", "아저씨에게 가자",1) }
                },
                new List<Reward>{new Reward("스킬(돌맹이)", 1, 50,50)}),

            new Quest("서브   퀘스트", "1_2009", "친절한 용사","괴물을 물리쳐줘",
                new Dictionary<string, QuestCondition>
                {
                    {"location_002_kill", new QuestCondition(QuestConditionType.Kill, "Mophan", "괴물을 처치하자",1) },
                    {"location_002_meet", new QuestCondition(QuestConditionType.Meet, "Npc1_01", "테오에게 가자",1) }
                },
                new List<Reward>{new Reward("스킬(보라색)", 1, 50,50)}),

            new Quest("서브   퀘스트", "1_2010", "친절한 용사","거대한 숲을 구해줘",
                new Dictionary<string, QuestCondition>
                {
                    {"location_002_kill", new QuestCondition(QuestConditionType.Kill, "Totem", "숲 속의 이상한 물건을 파괴하자",1) },
                    {"location_002_collect", new QuestCondition(QuestConditionType.Kill, "몬스터들 id", "몰려오는 몬스터들을 모두 처치하자",1) },
                    {"location_002_meet", new QuestCondition(QuestConditionType.Meet, "Quest1_10", "세이아에게 가자",1) }
                },
                new List<Reward>{new Reward("", 0, 50,50)}),

            #region 삭제된 서브 퀘스트
            /*new Quest("서브퀘스트", "1_2008", "마을의 수호자","이장의 선물",
                new Dictionary<string, QuestCondition>
                {
                    {"location_002_meet", new QuestCondition(QuestConditionType.Meet, "Quest1_05", "-",1) }
                },
                new List<Reward>{new Reward("회복의 반지", 1, 0, 0)}),

            new Quest("서브퀘스트", "1_2009", "마을의 수호자","대장장이의 선물",
                new Dictionary<string, QuestCondition>
                {
                    {"location_002_meet", new QuestCondition(QuestConditionType.Meet, "Quest1_06", "-",1) }
                },
                new List<Reward>{new Reward("강화석", 3, 0, 0)}),

            new Quest("서브퀘스트", "1_2010", "마을의 수호자","요리사의 선물",
                new Dictionary<string, QuestCondition>
                {
                    {"location_002_meet", new QuestCondition(QuestConditionType.Meet, "Quest1_07", "-",1) }
                },
                new List<Reward>{new Reward("요리", 3, 0, 0)}),

            new Quest("서브퀘스트", "1_2011", "마을의 수호자","흠 아저씨의 선물",
                new Dictionary<string, QuestCondition>
                {
                    {"location_002_meet", new QuestCondition(QuestConditionType.Meet, "Quest1_11", "-",1) }
                },
                new List<Reward>{new Reward("", 0, 0, 200)}),

            new Quest("서브퀘스트", "1_2012", "마을의 수호자","마을 소녀의 선물",
                new Dictionary<string, QuestCondition>
                {
                    {"location_002_meet", new QuestCondition(QuestConditionType.Meet, "Quest1_08", "-",1) }
                },
                new List<Reward>{new Reward("체력 물약(소)", 5, 0, 0)}),

            new Quest("서브퀘스트", "1_2013", "마을의 수호자","마을 소년의 선물",
                new Dictionary<string, QuestCondition>
                {
                    {"location_002_meet", new QuestCondition(QuestConditionType.Meet, "Quest1_09", "-",1) }
                },
                new List<Reward>{new Reward("마나 물약(소)", 5, 0, 0)}),

            new Quest("서브퀘스트", "1_2014", "마을의 수호자","세이아의 선물",
                new Dictionary<string, QuestCondition>
                {
                    {"location_002_meet", new QuestCondition(QuestConditionType.Meet, "Quest1_10", "-",1) }
                },
                new List<Reward>{new Reward("발광석", 1, 0, 0)}),*/
#endregion
        };
        return questList;
    }

    private static RandomName[] cachedNames = (RandomName[])Enum.GetValues(typeof(RandomName));

    private string GetRandomName()
    {
        RandomName name = cachedNames[UnityEngine.Random.Range(0, cachedNames.Length)];
        return name.ToString();
    }

    public void GenerateRandomNPCs(int numberOfNPCs, List<Quest> availableQuests, List<Item> availableItems, List<Quest> baseDatabase, NPCList npcDataList)
    {
        List<Quest> mainQuest = GenerateMainQuestLists();

        for (int i = 0; i < numberOfNPCs; i++)
        {
            string npcName = GetRandomName();
            NPCType npcType = (NPCType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(NPCType)).Length);

            NPCData npc = new NPCData
            {
                id = $"NPC_{i + 1}",
                name = npcName,
                npcType = npcType,
                currentState = NPCState.중립
            };

            ConfigureNPC(npc, npcType, availableQuests, availableItems);
            npcDataList.npcLists.Add(npc);
        }

        //npcDataList.mainQuestNpcLists = GenerateMainQuestNPCs(mainQuest);
        //npcDataList.shopNpcLists = CreateShopNPC();
    }

    private List<NPCData> GenerateMainQuestNPCs(List<Quest> baseDatabase)
    {
        List<NPCData> mainNpcList = new List<NPCData>();
        for (int i = 0; i < baseDatabase.Count; i++)
        {
            string npcName = GetRandomName();
            NPCData mainnpc = new NPCData
            {
                id = $"MainNPC_{i + 1}",
                name = npcName,
                npcType = NPCType.퀘스트,
                currentState = NPCState.중립,
                quests = new[] { baseDatabase[i] },
                isQuestGiver = true,
                isInteractable = true,
                description = "메인 퀘스트를 제공하는 NPC입니다."
            };
            mainNpcList.Add(mainnpc);
        }
        return mainNpcList;
    }

    private List<NPCData> CreateShopNPC()
    {
        List<NPCData> shopNpcList = new List<NPCData>();
        for (int i = 0; i < 4 ; i++)
        {
            string npcName = GetRandomName();
            ItemType itemType = ItemType.무기;
            switch (i)
            {
                case 0:
                    itemType = ItemType.무기;
                    break;
                case 1:
                    itemType = ItemType.방어구;
                    break;
                case 2:
                    itemType = ItemType.소모품;
                    break;
                case 3:
                    itemType = ItemType.장신구;
                    break;
            }

            var shopData = new ShopData
            {
                shopId = $"ShopNPC_{i + 1}",
                shopName = npcName,
                grade = 0,
                type = itemType,
                //isSpecific = UnityEngine.Random.Range(0, 2) == 1
            };
            shopData.Initialize();

            shopNpcList.Add(new NPCData
            {
                id = shopData.shopId,
                name = npcName,
                npcType = NPCType.상점,
                currentState = NPCState.중립,
                isShop = true,
                isInteractable = true,
                shopData = shopData,
                description = "아이템을 판매하는 상점 NPC입니다."
            });
        }
        return shopNpcList;
    }

    // NPC 구성
    private void ConfigureNPC(NPCData npc, NPCType npcType, List<Quest> availableQuests, List<Item> availableItems)
    {
        switch (npcType)
        {
            case NPCType.퀘스트:
                npc.isQuestGiver = true;
                npc.isInteractable = true;
                //npc.quests = GenerateRandomQuests(availableQuests, 1, 3); // 1~3개의 랜덤 퀘스트
                npc.description = "퀘스트를 제공하는 NPC입니다.";
                break;

            case NPCType.정보제공:
                npc.isInteractable = true;
                npc.description = "게임 정보를 제공하는 NPC입니다.";
                npc.dialogue = new[] { "저기 동굴에 보물이 있습니다.", "이 지역의 몬스터는 불에 약합니다." };
                break;

            case NPCType.상호작용:
                npc.description = "특정 장치를 작동시키는 NPC입니다.";
                npc.isInteractable = true;
                npc.interactionCondition = "특정 아이템 필요";
                break;

            case NPCType.힐러:
                npc.isInteractable = true;
                npc.description = "플레이어를 회복시켜주는 NPC입니다.";
                npc.dialogue = new[] { "필요한 치료가 있으신가요?", "안전을 기원합니다." };
                break;

            case NPCType.적NPC:
                npc.currentState = NPCState.적;
                npc.description = "플레이어를 공격할 수 있는 적대적 NPC입니다.";
                break;

            case NPCType.동료:
                npc.currentState = NPCState.동료;
                npc.description = "플레이어의 동료가 될 수 있는 NPC입니다.";
                break;

            default:
                npc.isInteractable = true;
                npc.description = "일반 대화만 가능한 NPC입니다.";
                break;
        }
    }


    private Item[] GenerateRandomItems(List<Item> availableItems, int min, int max)
    {
        if (availableItems == null || availableItems.Count == 0)
        {
            Debug.LogWarning("아이템 목록이 비어 있습니다. 아이템을 생성할 수 없습니다.");
            return new Item[0]; // 빈 배열 반환
        }
        int itemCount = Mathf.Clamp(UnityEngine.Random.Range(min, max + 1), 0, availableItems.Count);
        Item[] items = new Item[itemCount];

        for (int i = 0; i < itemCount; i++)
        {
            items[i] = availableItems[UnityEngine.Random.Range(0, availableItems.Count)];
        }

        return items;
    }

    //private Item[] GenerateRandomItems(List<Item> availableItems, int min, int max)
    //{
    //    int itemCount = Random.Range(min, max + 1);
    //    Item[] items = new Item[itemCount];
    //    for (int i = 0; i < itemCount; i++)
    //    {
    //        Item randomItem;
    //        do
    //        {
    //            randomItem = availableItems[Random.Range(0, availableItems.Count)];
    //        }
    //        while (randomItem.type != ItemType.소비형); // 소비형 아이템만 선택
    //        items[i] = randomItem;
    //    }
    //    return items;
    //}


    private static string[] questString = new string[5];
    private static int questCount = 0;

    public static Quest GenerateQuest(QuestConditionType type)
    {
        // 퀘스트 기본 정보 설정
        questString[0] = "서브퀘스트";
        questString[1] = type.ToString() + $"_{questCount:D5}";
        questString[2] = GetQuestTitle(type);
        questString[3] = GetQuestDescription(type);
        questString[4] = GetQuestTarget(type);

        questCount++;

        return CreateQuest(type);
    }

    private static Quest CreateQuest(QuestConditionType type)
    {
        return new Quest(
            questString[0],
            questString[1],
            questString[2],
            questString[3],
            new Dictionary<string, QuestCondition>
            {
                { questString[4], new QuestCondition(type, questString[4], questString[2], GetRequiredQuantity(type)) }
            },
            new List<Reward>
            {
                new Reward(GetRewardItem(type), GetRewardQuantity(type), GetRewardExp(type), GetRewardGold(type))
            }
        );
    }

    private static string GetQuestTitle(QuestConditionType type)
    {
        return type switch
        {
            QuestConditionType.Collect => "회복 포션 수집",
            QuestConditionType.Explore => "미지의 숲 탐험",
            QuestConditionType.Kill => "슬라임 처치",
            QuestConditionType.Meet => "마을 장로 만나기",
            _ => "알 수 없는 퀘스트"
        };
    }

    private static string GetQuestDescription(QuestConditionType type)
    {
        return type switch
        {
            QuestConditionType.Collect => "회복 포션 10개를 수집하세요.",
            QuestConditionType.Explore => "미지의 숲을 탐험하세요.",
            QuestConditionType.Kill => "슬라임을 5마리 처치하세요.",
            QuestConditionType.Meet => "마을 장로를 만나서 이야기를 나누세요.",
            _ => "퀘스트 설명이 없습니다."
        };
    }

    private static string GetQuestTarget(QuestConditionType type)
    {
        return type switch
        {
            QuestConditionType.Collect => "소형 체력포션",
            QuestConditionType.Explore => "mystic_forest",
            QuestConditionType.Kill => "slime",
            QuestConditionType.Meet => "village_elder",
            _ => "unknown_target"
        };
    }

    private static int GetRequiredQuantity(QuestConditionType type)
    {
        return type switch
        {
            QuestConditionType.Collect => 10,
            QuestConditionType.Explore => 1,
            QuestConditionType.Kill => 5,
            QuestConditionType.Meet => 1,
            _ => 1
        };
    }

    private static string GetRewardItem(QuestConditionType type)
    {
        return type switch
        {
            QuestConditionType.Collect => "소형 체력포션",
            QuestConditionType.Explore => "탐험자 배지",
            QuestConditionType.Kill => "슬라임 젤리",
            QuestConditionType.Meet => "신뢰의 증표",
            _ => "미정"
        };
    }

    private static int GetRewardQuantity(QuestConditionType type)
    {
        return type switch
        {
            QuestConditionType.Collect => 10,
            QuestConditionType.Explore => 1,
            QuestConditionType.Kill => 5,
            QuestConditionType.Meet => 1,
            _ => 1
        };
    }

    private static int GetRewardExp(QuestConditionType type)
    {
        return type switch
        {
            QuestConditionType.Collect => 100,
            QuestConditionType.Explore => 150,
            QuestConditionType.Kill => 200,
            QuestConditionType.Meet => 250,
            _ => 0
        };
    }

    private static int GetRewardGold(QuestConditionType type)
    {
        return type switch
        {
            QuestConditionType.Collect => 50,
            QuestConditionType.Explore => 70,
            QuestConditionType.Kill => 100,
            QuestConditionType.Meet => 120,
            _ => 0
        };
    }
}