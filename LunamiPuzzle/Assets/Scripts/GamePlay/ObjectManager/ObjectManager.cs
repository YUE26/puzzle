using System.Collections.Generic;
using Core.Event;
using Core.SaveLoad;
using GamePlay.Bag.Logic;
using Repo.Event;
using UnityEngine;

namespace GamePlay.ObjectManager
{
    public class ObjectManager : MonoBehaviour, ISaveable
    {
        private Dictionary<int, bool> objectDic = new Dictionary<int, bool>();
        private Dictionary<string, bool> interactionDic = new Dictionary<string, bool>();

        private void OnEnable()
        {
            EventModule.AddListener(EventName.EvtBeforeUnloadScene, OnBeforeUnloadScene);
            EventModule.AddListener(EventName.EvtAfterLoadScene, OnAfterLoadScene);
            EventModule.AddListener(EventName.EvtUpdateItem, OnUpdateItemClick);
            EventModule.AddListener(EventName.EvtStartGameEvent, OnStartGameEvent);
        }

        private void Start()
        {
            ISaveable saveable = this;
            saveable.Register();
        }

        private void OnDisable()
        {
            EventModule.RemoveListener(EventName.EvtBeforeUnloadScene, OnBeforeUnloadScene);
            EventModule.RemoveListener(EventName.EvtAfterLoadScene, OnAfterLoadScene);
            EventModule.RemoveListener(EventName.EvtUpdateItem, OnUpdateItemClick);
            EventModule.RemoveListener(EventName.EvtStartGameEvent, OnStartGameEvent);
        }

        private void OnStartGameEvent(object obj)
        {
            objectDic.Clear();
            interactionDic.Clear();
        }

        private void OnBeforeUnloadScene(object obj)
        {
            //Item状态保存
            foreach (var item in FindObjectsByType<Item>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                objectDic.TryAdd(item.id, true);
            }

            //可交互物体状态保存
            foreach (var interaction in FindObjectsByType<Interaction.Interaction>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                interactionDic[interaction.name] = interaction.isDone;
            }
        }

        private void OnAfterLoadScene(object obj)
        {
            //Item状态保存
            foreach (var item in FindObjectsByType<Item>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                if (objectDic.ContainsKey(item.id) == false)
                {
                    objectDic.Add(item.id, true);
                }
                else
                {
                    item.gameObject.SetActive(objectDic[item.id]);
                }
            }

            //可交互物体状态保存
            foreach (var interaction in FindObjectsByType<Interaction.Interaction>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                if (interactionDic.ContainsKey(interaction.name))
                {
                    interaction.isDone = interactionDic[interaction.name];
                }
                else
                {
                    interactionDic.Add(interaction.name, interaction.isDone);
                }
            }
        }

        private void OnUpdateItemClick(object obj)
        {
            if (obj is not EvtItemUpdateData iData) return;
            if (iData.itemDetail != null && objectDic.ContainsKey(iData.itemDetail.itemId))
            {
                objectDic[iData.itemDetail.itemId] = false;
            }
        }

        public SaveData.SaveData GenerateSaveData()
        {
            SaveData.SaveData data = new SaveData.SaveData();
            data.objectDic = objectDic;
            data.interactionDic = interactionDic;
            return data;
        }

        public void ReadGameData(SaveData.SaveData gameData)
        {
            objectDic = gameData.objectDic;
            interactionDic = gameData.interactionDic;
        }
    }
}