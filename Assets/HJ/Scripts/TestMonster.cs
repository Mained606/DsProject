using System.Collections.Generic;
using UnityEngine;

public class TestMonster : MonoBehaviour
{
    [SerializeField] private List<int> dropItemIds = new List<int>();


    private void Start()
    {
        Die();
    }
    

    private void Die()
    {
        ItemManager.Instance.SpawnItemBox(transform.position, dropItemIds);
    }
}
