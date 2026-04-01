using System.Collections.Generic;
using Core.Event;
using GamePlay.Bag.Data;
using GamePlay.Interfaces;
using Repo.Event;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GamePlay.Bag.Logic
{
    /// <summary>
    /// 可以放到背包里的
    /// </summary>
    public class Item : MonoBehaviour, IInteraction
    {
        public int id;
        public List<ItemInteract> interacts = new List<ItemInteract>();


        private void Awake()
        {
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (ItemManager.Instance.itemInHand == null || ItemManager.Instance.itemInHand.itemId == 0)
            {
                ItemClick();
            }
            else
            {
                var inHand = ItemManager.Instance.itemInHand.itemId;
                TryCompositeItem(inHand);
            }
        }

        private void TryCompositeItem(int inHand)
        {
            if (Csv.ItemCfgStore.TryGetValue(id, out ItemCfg itemCfg) == false) return;
            if (itemCfg.target == inHand)
            {
                DoCompositeItem(itemCfg.result);
            }
        }
        
        
        public void ItemClick()
        {
            gameObject.SetActive(false);
            ItemManager.Instance.AddItemToBag(id);
            EventModule.Dispatch(EventName.EvtUpdateItem, id);
            OnInteractClick();
        }

#region virtual

        /// <summary>
        /// click and change
        /// </summary>
        /// <param name="id"></param>
        protected virtual void DoCompositeItem(int id)
        {
        }
        
        /// <summary>
        /// pure click, no state change
        /// </summary>
        protected virtual void OnInteractClick()
        {
        }

#endregion
        
    }
}