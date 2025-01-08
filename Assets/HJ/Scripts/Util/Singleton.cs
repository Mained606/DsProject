using UnityEngine;

namespace HJ
{
    public class Singleton<T> : MonoBehaviour where T : Singleton<T>    //Where은 제한
    {
        private static T instance;

        public static T Instance
        {
            get { return instance; }
        }

        public static bool InstanceExist
        {
            get { return instance != null; }
        }

        protected virtual void Awake()
        {
            if(instance != null)
            {
                Destroy(this.gameObject);
                return;
            }
            instance = (T)this;
        }

        protected virtual void OnDestroy()
        {
            if(instance == null)
            {
                instance = null;
            }
        }
    }
}

