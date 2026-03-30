using Core.Interfaces;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Core.Input
{
    [CreateAssetMenu(fileName = "InputReader", menuName = "Core/Input/InputReader")]
    public class InputReader : ScriptableObject, PlayerAction.IPlayerActions, IInput
    {
        public event UnityAction InteractEvent;
        public event UnityAction LeftEvent;
        public event UnityAction RightEvent;
        public event UnityAction DownEvent;
        public event UnityAction UpEvent;

        private PlayerAction playerInputAction;

        private void OnEnable()
        {
            if (playerInputAction == null)
            {
                playerInputAction = new PlayerAction();
                playerInputAction.Player.SetCallbacks(this);
            }
        }

        private void OnDisable()
        {
            playerInputAction.Player.Disable();
        }

        public void EnablePlayerInput()
        {
            playerInputAction.Player.Enable();
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                InteractEvent?.Invoke();
        }

        public void OnLeft(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                LeftEvent?.Invoke();
        }

        public void OnRight(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                throw new System.NotImplementedException();
            }
        }

        public void OnDown(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                throw new System.NotImplementedException();
        }

        public void OnUp(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}