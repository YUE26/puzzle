using System;
using Core;
using Core.Event;
using Core.Localization;
using Core.SaveLoad;
using CsvModule;
using GamePlay.Bag;
using GamePlay.MiniGame;
using Repo.Event;
using UnityEngine;

namespace GamePlay
{
    public class GameManager : SingletonMono<GameManager>, ISaveable
    {
        private int _gameWeek;
        private Csv _csv;

        private void OnEnable()
        {
            EventModule.AddListener(EventName.EvtAfterLoadScene, OnAfterLoadScene);
            EventModule.AddListener(EventName.EvtStartGameEvent, OnStartGameEvent);
            EventModule.Dispatch(EventName.EvtUpdateGameState, GameState.GamePlay);
        }

        private void OnDisable()
        {
            EventModule.RemoveListener(EventName.EvtAfterLoadScene, OnAfterLoadScene);
            EventModule.RemoveListener(EventName.EvtStartGameEvent, OnStartGameEvent);
        }

        protected override void OnAwake()
        {
            base.OnAwake();
            _csv = new Csv();
            _csv.Init();
        }

        private void Start()
        {
            //注册存储
            ISaveable saveable = this;
            saveable.Register();
        }

        private void OnStartGameEvent(object obj)
        {
            if (obj is not int week) return;
            _gameWeek = week;
            MiniGameController.Instance.ClearMiniGameState();
        }

        private void OnAfterLoadScene(object obj)
        {
            MiniGameController.Instance.SetMiniGameStateInScene(_gameWeek);
        }
        
        
        /// <summary>
        /// 生成一份存储数据
        /// </summary>
        /// <returns></returns>
        public SaveData.SaveData GenerateSaveData()
        {
            SaveData.SaveData data = new SaveData.SaveData();
            data.gameWeek = _gameWeek;
            data.langState = Localization._language;
            return data;
        }

        public void ReadGameData(SaveData.SaveData gameData)
        {
            this._gameWeek = gameData.gameWeek;
            Localization._language = gameData.langState;
        }
    }
}