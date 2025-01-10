using System;
using System.Collections.Generic;
using UnityEngine;

public class TestUIManager : BaseManager<TestUIManager>
{
    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {

    }
    private void ShowMenuUI()
    {
        Debug.Log($"ShowMenuUI");
    }

    private void ShowPauseUI()
    {
        Debug.Log("ShowPauseUI");
    }

    private void HideAllUI()
    {
        Debug.Log("HideAllUI");
    }
}
