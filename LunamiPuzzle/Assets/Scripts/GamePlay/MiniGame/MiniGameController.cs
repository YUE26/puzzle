using System.Collections.Generic;
using Core;
using Core.Event;
using Core.SaveLoad;
using Repo.Event;
using UnityEngine;

namespace GamePlay.MiniGame
{
    public class MiniGameController : SingletonMono<MiniGameController>, ISaveable
    {
        private Dictionary<string, bool> miniGameStateDic = new Dictionary<string, bool>();

        private void Start()
        {
            ISaveable saveable = this;
            saveable.Register();
        }

        private void OnEnable()
        {
            EventModule.AddListener(EventName.EvtPassGameEvent, OnPassGameEvent);
        }

        private void OnDisable()
        {
            EventModule.RemoveListener(EventName.EvtPassGameEvent, OnPassGameEvent);
        }
        
        public void SetMiniGameStateInScene(int gameWeek)
        {
            foreach (var miniGame in FindObjectsByType<MiniGame>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {
                if (miniGameStateDic.TryGetValue(miniGame.gameScene, out var isPass))
                {
                    miniGame.isPass = isPass;
                    miniGame.UpdateMiniGameState();
                    miniGame.ChooseGameData(gameWeek);
                }
            }
        }

        public void ClearMiniGameState()
        {
            miniGameStateDic.Clear();
        }

        public SaveData.SaveData GenerateSaveData()
        {
            SaveData.SaveData data = new SaveData.SaveData();
            data.miniGameStateDic = miniGameStateDic;
            return data;
        }

        public void ReadGameData(SaveData.SaveData gameData)
        {
            this.miniGameStateDic = gameData.miniGameStateDic;
        }

        
#region AddListener

        private void OnPassGameEvent(object obj)
        {
            if (obj is not string gameName) return;
            miniGameStateDic[gameName] = true;
        }

#endregion
    }
}