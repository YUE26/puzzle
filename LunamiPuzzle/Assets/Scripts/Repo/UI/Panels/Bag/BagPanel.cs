using System.Collections.Generic;
using Core;
using Core.Event;
using Core.UI;
using GamePlay.Bag;
using Repo.Event;
using Repo.UI.Panels.Bag;
using UnityEngine;

namespace Repo.UI.Panels
{
    public class BagPanel : UIBase
    {
        public override PanelName panelName => PanelName.BagPanel;

        [SerializeField]
        private RectTransform itemRect;

        [SerializeField]
        private GameObject bagItemPrefab;

        private List<BagItem> bagItems;
        private Pool<BagItem> _pool;
        private const int _defaultCapacity=7;

        protected override void DoEnable()
        {
            base.DoEnable();
            EventModule.AddListener(EventName.EvtRefreshBag, RefreshBag);
            if (bagItems != null)
            {
                RefreshBag(null);
            }
        }

        protected override void DoStart(object arg)
        {
            base.DoStart(arg);
            _pool = new Pool<BagItem>(objectGenerator: () =>
                {
                    var go = Instantiate(bagItemPrefab,itemRect);
                    go.SetActive(false);
                    var sc = go.GetComponent<BagItem>();
                    return sc;
                }, onGet: sc =>
                {
                    sc.gameObject.SetActive(true);
                    sc.transform.SetAsLastSibling();
                },
                onRelease: sc => sc.OnRelease(),
                initialCapacity: 0);
            
            Init();
            RefreshBag(null);
        }

        protected override void DoDisable()
        {
            base.DoDisable();
            EventModule.RemoveListener(EventName.EvtRefreshBag, RefreshBag);
        }

        private void Init()
        {
            bagItems = new List<BagItem>();
            EnsureSlotCount(_defaultCapacity);
        }

#region AddListener
        
        private void RefreshBag(object obj)
        {
            if (_pool == null || bagItems == null || ItemManager.Instance == null) return;
            var bag = ItemManager.Instance.GetBag();
            var targetSlots = Mathf.Max(_defaultCapacity, bag.Count);
            EnsureSlotCount(targetSlots);

            for (int i = 0; i < bagItems.Count; i++)
            {
                if (i < bag.Count)
                {
                    bagItems[i].SetItem(bag[i]);
                }
                else
                {
                    bagItems[i].SetEmpty();
                }
            }
        }

        private void EnsureSlotCount(int targetCount)
        {
            while (bagItems.Count < targetCount)
            {
                var go = _pool.Get();
                go.SetEmpty();
                bagItems.Add(go);
            }

            for (int i = bagItems.Count - 1; i >= targetCount; i--)
            {
                var bagItem = bagItems[i];
                _pool.Release(bagItem);
                bagItems.RemoveAt(i);
            }
        }

#endregion
    }
}
