using System.Collections.Generic;
using Core;
using Core.Event;
using Core.SaveLoad;
using GamePlay.Bag.Data;
using Repo.Event;
using UnityEngine;

namespace GamePlay.Bag
{
    /// <summary>
    /// 物品背包
    /// </summary>
    public class ItemManager : SingletonMono<ItemManager>, ISaveable
    {
        public ItemDetail itemInHand { get; private set; } = null;

        private List<int> bag = new List<int>();

        private void OnEnable()
        {
            EventModule.AddListener(EventName.EvtItemUse, DeleteItemFromBag);
            EventModule.AddListener(EventName.EvtAfterLoadScene, OnAfterLoadScene);
            EventModule.AddListener(EventName.EvtStartGameEvent, OnStartGameEvent);
        }

        private void Start()
        {
            ISaveable saveable = this;
            saveable.Register();
        }

        private void OnDisable()
        {
            EventModule.RemoveListener(EventName.EvtItemUse, DeleteItemFromBag);
            EventModule.RemoveListener(EventName.EvtAfterLoadScene, OnAfterLoadScene);
            EventModule.RemoveListener(EventName.EvtStartGameEvent, OnStartGameEvent);
        }

        private void OnStartGameEvent(object obj)
        {
            bag.Clear();
        }

        private void OnAfterLoadScene(object obj)
        {
            if (bag.Count == 0)
            {
                EventModule.Dispatch(EventName.EvtUpdateItem, new EvtItemUpdateData() { itemDetail = null, index = -1 });
            }
            else
            {
                for (int i = 0, count = bag.Count; i < count; i++)
                {
                    EventModule.Dispatch(EventName.EvtUpdateItem, new EvtItemUpdateData() { itemDetail = SelectItemFromIndex(i), index = i });
                }
            }
        }

        /// <summary>
        /// 背包增加物品
        /// </summary>
        /// <param name="itemId"></param>
        public void AddItemToBag(int itemId)
        {
            if (bag.Contains(itemId) == false)
            {
                bag.Add(itemId);
            }

            if (Csv.ItemCfgStore.TryGetValue(itemId, out ItemCfg cfg))
            {
                var itemDetail = new ItemDetail() { itemId = itemId, itemSprite = ResourceManager<Sprite>.Load(cfg.sprite) };
                EventModule.Dispatch(EventName.EvtUpdateItem, new EvtItemUpdateData() { itemDetail = itemDetail, index = bag.Count - 1 });
            }
        }

        /// <summary>
        /// 使用物品后删除
        /// </summary>
        /// <param name="itemName"></param>
        private void DeleteItemFromBag(object obj)
        {
            if (obj is not int itemName) return;
            bag.Remove(itemName);
            //Todo:删除单一物品
            EventModule.Dispatch(EventName.EvtUpdateItem, new EvtItemUpdateData() { itemDetail = null, index = -1 });
        }

        /// <summary>
        /// todo: select item from index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ItemDetail SelectItemFromIndex(int index)
        {
            // itemInHand = itemData.GetItemDetail(bag[index]);
            // return itemData.GetItemDetail(bag[index]);
            return null;
        }

        public void ReleaseHand()
        {
            itemInHand = null;
        }


        public SaveData.SaveData GenerateSaveData()
        {
            SaveData.SaveData data = new SaveData.SaveData();
            data.bag = bag;
            return data;
        }

        public void ReadGameData(SaveData.SaveData gameData)
        {
            bag = gameData.bag;
        }
    }
}