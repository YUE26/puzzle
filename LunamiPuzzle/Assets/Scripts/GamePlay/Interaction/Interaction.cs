using GamePlay.Bag;
using GamePlay.Interfaces;
using Core.Event;
using Repo.Event;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GamePlay.Interaction
{
    /// <summary>
    /// 不能被收到背包里的
    /// </summary>
    public class Interaction : MonoBehaviour, IInteraction
    {
        [SerializeField]
        protected int id;

        [HideInInspector]
        public bool isDone;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Csv.InteractionCfgStore.TryGetValue(id, out var interactionCfg) == false) return;
            var targetId = interactionCfg.target;
            if (ItemManager.Instance.itemInHand == null) return;
            var inHandId = ItemManager.Instance.itemInHand.itemId;
            if (inHandId == targetId)
            {
                isDone = true;
                ItemClick();
                EventModule.Dispatch(EventName.EvtItemUse, inHandId);
                ItemManager.Instance.ReleaseHand();
            }
        }

        public void ItemClick()
        {
            OnItemClick();
        }

        protected virtual void OnItemClick()
        {
        }
    }
}
