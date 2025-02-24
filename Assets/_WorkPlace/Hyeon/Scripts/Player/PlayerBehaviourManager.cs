using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviourManager : BaseManager<PlayerBehaviourManager>
{
    private List<IBehaviour> behaviours = new List<IBehaviour>();

    public void AddBehaviour(IBehaviour behaviour)
    {
        behaviours.Add(behaviour);
    }

    public void RemoveBeheviour(IBehaviour behaviour)
    {
        behaviours.Remove(behaviour);
    }

    private void Update()
    {
        foreach(var behaviour in behaviours)
        {
            behaviour.Execute();
            //Debug.Log($"{behaviour} 행동 중");
        }
    }

    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {
    }
}
