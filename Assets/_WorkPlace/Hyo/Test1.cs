using System;
using UnityEngine;

public class Test1 : MonoBehaviour
{
    public MonsterData monster;
    
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            CharacterManager.Instance.OnMonsterDefeated(this.monster, this.gameObject.transform.position);
            
            Destroy(this.gameObject);
            Debug.Log(this.gameObject.name + "죽었습니다");
        }
    }
}
