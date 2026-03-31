using Core.Event;
using GamePlay.Interfaces;
using Repo.Event;
using UnityEngine;
using UnityEngine.Events;

namespace GamePlay.MiniGame
{
    public class MiniGame : MonoBehaviour, IMiniGame
    {
        public UnityEvent finishGame;
        [SceneName]
        public string gameScene;

        public bool isPass;

        private void OnEnable()
        {
            EventModule.AddListener(EventName.EvtFinishMiniGame, CheckGameStateEvent);
        }

        private void OnDisable()
        {
            EventModule.RemoveListener(EventName.EvtFinishMiniGame, CheckGameStateEvent);
        }

        public void InitMiniGame()
        {
            OnInitMiniGame();
        }

        public void ResetMiniGame()
        {
            OnResetMiniGame();
        }

        public void ChooseGameData(int week)
        {
            OnChooseGameData();
        }

        public void CheckGameStateEvent(object obj)
        {
            OnCheckGameStateEvent(obj);
        }

        public void UpdateMiniGameState()
        {
            if (isPass)
            {
                GetComponent<Collider2D>().enabled = false;
                finishGame?.Invoke();
                gameObject.SetActive(false);
            }
        }

#region virtual

        protected virtual void OnInitMiniGame()
        {
        }

        protected virtual void OnResetMiniGame()
        {
        }

        protected virtual void OnChooseGameData()
        {
        }

        protected virtual void OnCheckGameStateEvent(object obj)
        {
        }

#endregion
    }
}