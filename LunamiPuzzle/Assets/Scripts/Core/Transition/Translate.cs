using Core.Editors;
using UnityEngine;

namespace Core.Transition
{
    public class Translate:MonoBehaviour
    {
        [SceneName]public string FromName;
        [SceneName]public string ToName;

        public void TranslateScene()
        {
            TransitionManager.Instance.Transition(FromName,ToName);
        }
    }
}