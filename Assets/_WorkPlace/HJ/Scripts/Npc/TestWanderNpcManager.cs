using UnityEngine;
using System.Collections.Generic;

public class TestWanderNpcManager : BaseManager<TestWanderNpcManager>
{
    public List<GameObject> wanderNpcs = new List<GameObject>();

    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {
        
    }
}
