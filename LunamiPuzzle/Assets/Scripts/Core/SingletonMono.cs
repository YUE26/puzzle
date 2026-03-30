using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Core
{
    public class SingletonMono<T> : MonoBehaviour, ICore where T : Object
    {
        private static T instance = default(T);

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<T>();
                }

                return instance;
            }
        }

        private void Awake()
        {
            if (instance != null)
            {
                Destroy(this.gameObject);
                return;
            }

            DontDestroyOnLoad(this.gameObject);
            instance = this.gameObject.GetComponent<T>();
            OnAwake();
        }

        protected virtual void OnAwake()
        {
        }
    }
}