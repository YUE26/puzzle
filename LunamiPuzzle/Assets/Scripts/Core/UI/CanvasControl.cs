using UnityEngine;

namespace Core.UI
{
    public class CanvasControl: SingletonMono<CanvasControl>
    {
        public Transform canvasTransform;
        public static CanvasControl instance;
        

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}