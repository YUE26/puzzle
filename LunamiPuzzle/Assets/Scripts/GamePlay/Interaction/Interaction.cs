using GamePlay.Bag;
using GamePlay.Interfaces;
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
            Csv.InteractionCfgStore.TryGetValue(id, out var interactionCfg);
            var targetId = interactionCfg.target;
            if (ItemManager.Instance.itemInHand == null) return;
            if (ItemManager.Instance.itemInHand.itemId == targetId)
            {
                isDone = true;
                ItemClick();
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