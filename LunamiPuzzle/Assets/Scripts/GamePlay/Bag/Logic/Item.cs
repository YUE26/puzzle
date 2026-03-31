using System.Collections.Generic;
using CsvModule;
using GamePlay.Bag.Data;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GamePlay.Bag.Logic
{
    public class Item : MonoBehaviour, IPointerClickHandler
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
                OnItemClick();
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
        protected virtual void OnItemClick()
        {
        }
    }
}