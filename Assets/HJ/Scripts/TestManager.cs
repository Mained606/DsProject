using UnityEngine;

namespace HJ
{
    public class TestManager : MonoBehaviour
    {
        public static TestManager Instance { get; private set; }

        public Transform player;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}