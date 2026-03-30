using System;
using UnityEngine;
using UnityEngine.Events;

namespace Core.Input
{
    public class InputModule:SingletonMono<InputModule>
    {
        public InputReader InputReader;
        
        public Vector2 MousePosition => InputReader.MousePosition;

        protected override void OnAwake()
        {
            base.OnAwake();
            if (InputReader != null)
            {
                InputReader.EnablePlayerInput();
            }
        }

        public void AddInteractEvent(UnityAction action)
        {
            InputReader.InteractEvent += action;
        }

        public void RemoveInteractEvent(UnityAction action)
        {
            InputReader.InteractEvent -= action;
        }

        public void AddLeftEvent(UnityAction action)
        {
            InputReader.LeftEvent += action;
        }

        public void RemoveLeftEvent(UnityAction action)
        {
            InputReader.LeftEvent -= action;
        }

        public void AddRightEvent(UnityAction action)
        {
            InputReader.RightEvent += action;
        }

        public void RemoveRightEvent(UnityAction action)
        {
            InputReader.RightEvent -= action;
        }

        public void AddUpEvent(UnityAction action)
        {
            InputReader.UpEvent += action;
        }

        public void RemoveUpEvent(UnityAction action)
        {
            InputReader.UpEvent -= action;
        }

        public void AddDownEvent(UnityAction action)
        {
            InputReader.DownEvent += action;
        }

        public void RemoveDownEvent(UnityAction action)
        {
            InputReader.DownEvent -= action;
        }
    }
}