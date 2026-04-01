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


        private List<ItemDetail> bag = new List<ItemDetail>();
        public int Capacity { get; private set; } = 0;

        private void OnEnable()
        {
            EventModule.AddListener(EventName.EvtItemUse, UseItem);
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
            EventModule.RemoveListener(EventName.EvtItemUse, UseItem);
            EventModule.RemoveListener(EventName.EvtAfterLoadScene, OnAfterLoadScene);
            EventModule.RemoveListener(EventName.EvtStartGameEvent, OnStartGameEvent);
        }

        private void OnStartGameEvent(object obj)
        {
            bag.Clear();
            Capacity = 0;
            itemInHand = null;
            EventModule.Dispatch(EventName.EvtRefreshBag);
        }

        private void OnAfterLoadScene(object obj)
        {
            EventModule.Dispatch(EventName.EvtRefreshBag);
        }

        private bool IsBagContain(int itemId)
        {
            if (bag == null || bag.Count == 0) return false;
            foreach (var itemDetail in bag)
            {
                if (itemDetail == null) continue;
                if (itemDetail.itemId == itemId)
                {
                    if (itemDetail.countable == Countable.UnCountable || itemDetail.count != 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private int GetFirstEmptySlotInBag()
        {
            for (int i = 0; i < bag.Count; i++)
            {
                if (bag[i] == null)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// 背包增加物品
        /// </summary>
        /// <param name="itemId"></param>
        public void AddItemToBag(int itemId)
        {
            if (Csv.ItemCfgStore.TryGetValue(itemId, out var itemCfg) == false) return;
            if (IsBagContain(itemId) == false)
            {
                var newDetail = new ItemDetail()
                {
                    itemId = itemId,
                    itemSprite = ResourceManager<Sprite>.Load(itemCfg.sprite),
                    countable = (Countable)itemCfg.countable,
                    count = 1
                };
                var newIndex = GetFirstEmptySlotInBag();
                if (newIndex == -1)
                {
                    bag.Add(newDetail);
                }
                else
                {
                    bag[newIndex] = newDetail;
                }

                Capacity = bag.Count;
            }
            else
            {
                foreach (var itemDetail in bag)
                {
                    if (itemDetail == null) continue;
                    if (itemDetail.itemId != itemId) continue;
                    if (itemDetail.countable == Countable.Countable)
                    {
                        itemDetail.count++;
                    }
                }
            }

            EventModule.Dispatch(EventName.EvtRefreshBag);
        }

        /// <summary>
        /// 使用物品后删除
        /// </summary>
        /// <param name="itemName"></param>
        private void UseItem(object obj)
        {
            if (obj is not int itemId) return;
            for (int i = 0; i < bag.Count; i++)
            {
                var detail = bag[i];
                if (detail == null || detail.itemId != itemId) continue;

                if (detail.countable == Countable.Countable)
                {
                    detail.count--;
                    if (detail.count <= 0)
                    {
                        bag[i] = null;
                    }
                }
                else
                {
                    bag[i] = null;
                }

                break;
            }

            TrimBagTail();
            Capacity = bag.Count;
            EventModule.Dispatch(EventName.EvtRefreshBag);
        }

        public void SelectItemInHand(ItemDetail detail)
        {
            itemInHand = detail;
        }

        public void ReleaseHand()
        {
            itemInHand = null;
        }

        public List<ItemDetail> GetBag()
        {
            return bag;
        }


        public SaveData.SaveData GenerateSaveData()
        {
            SaveData.SaveData data = new SaveData.SaveData();
            data.bag = bag;
            data.Capacity = Capacity;
            return data;
        }

        public void ReadGameData(SaveData.SaveData gameData)
        {
            bag = gameData.bag ?? new List<ItemDetail>();
            TrimBagTail();
            Capacity = gameData.Capacity > 0 ? gameData.Capacity : bag.Count;
            itemInHand = null;
        }

        private void TrimBagTail()
        {
            for (int i = bag.Count - 1; i >= 0; i--)
            {
                if (bag[i] != null) break;
                bag.RemoveAt(i);
            }
        }
    }
}
