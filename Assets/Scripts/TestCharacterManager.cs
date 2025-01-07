using UnityEngine;

public class TestCharacterManager : MonoBehaviour
{
    public TestCharacterStats playerStats { get; private set; }

    private void Awake()
    {
        playerStats = new TestCharacterStats();
    }
    private void Start()
    {
        TestGameManager.Instance.RegisterManager(this);
    }

    public TestCharacterStats GetPlayerStats()
    {
        return playerStats;
    }

}
