using System.Collections.Generic;
using UnityEngine;

public class EnhancementManager : BaseManager<EnhancementManager>
{
    [SerializeField] private List<string> EnhancementItemIds = new List<string>();
    private Dictionary<int, (int success, int downgrade, int destroy)> levelEnhanceChance;

    protected override void Start()
    {
        base.Start();

        levelEnhanceChance = new Dictionary<int, (int success, int downgrade, int destroy)>
        {
            { 0, (100, 0, 0) },
            { 1, (95, 5, 0) },
            { 2, (90, 10, 0) },
            { 3, (85, 15, 0) },
            { 4, (80, 15, 5) },
            { 5, (75, 15, 10) },
            { 6, (70, 20, 10) },
            { 7, (60, 25, 15) },
            { 8, (50, 30, 20) }
        };
    }

    public void Enhance(Item item)
    {
        var enhanceChances = levelEnhanceChance[item.itemSkill.level];
        float random = Random.value * 100;

        if(random <= enhanceChances.success)
        {
            Successed();
        }
        else if(random < enhanceChances.success + enhanceChances.downgrade)
        {
            Downgraded();
        }
        else
        {
            Destroyed();
        }
    }

    private void Successed()
    {
        Debug.Log("강화 성공");
    }

    private void Downgraded()
    {
        Debug.Log("강화 실패, 다운그레이드");
    }
    
    private void Destroyed()
    {
        Debug.Log("강화 실패, 아이템 파괴");
    }

    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {
    }
}
