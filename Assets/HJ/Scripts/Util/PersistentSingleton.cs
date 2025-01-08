using UnityEngine;

namespace HJ
{
    public class PersistentSingleton<T> : Singleton<T> where T : Singleton<T>
    {
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);  //씬이 변경되어도 gameObject가 삭제되지 않게 함
        }
    }
}