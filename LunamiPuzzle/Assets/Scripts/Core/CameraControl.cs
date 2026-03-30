using UnityEngine;

namespace Core
{
    public class CameraControl:SingletonMono<CameraControl>
    {
        [HideInInspector]
        public Camera cameraMain;

        protected override void OnAwake()
        {
            base.OnAwake();
            cameraMain = GetComponent<Camera>();
        }
    }
}