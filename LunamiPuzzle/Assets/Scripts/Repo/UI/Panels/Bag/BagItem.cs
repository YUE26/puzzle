using GamePlay.Bag;
using GamePlay.Bag.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Repo.UI.Panels.Bag
{
    public class BagItem : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        private Image item;

        private bool isClick;
        private ItemDetail currentDetail;

        public bool IsEmpty { get; private set; } = false;

        public void SetItem(ItemDetail itemDetail)
        {
            IsEmpty = false;
            isClick = false;
            currentDetail = itemDetail;
            if (currentDetail == null)
            {
                item.gameObject.SetActive(false);
            }
            else
            {
                item.gameObject.SetActive(true);
                item.sprite = currentDetail.itemSprite;
                item.SetNativeSize();
            }
        }

        public void SetEmpty()
        {
            IsEmpty = true;
            isClick = false;
            currentDetail = null;
            item.gameObject.SetActive(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (currentDetail == null || IsEmpty)
            {
                ItemManager.Instance.ReleaseHand();
                isClick = false;
                return;
            }

            var inHand = ItemManager.Instance.itemInHand;
            if (ReferenceEquals(inHand, currentDetail))
            {
                isClick = false;
                ItemManager.Instance.ReleaseHand();
            }
            else
            {
                isClick = true;
                ItemManager.Instance.SelectItemInHand(currentDetail);
            }
        }

        public void OnRelease()
        {
            IsEmpty = true;
            isClick = false;
            currentDetail = null;
            item.sprite = null;
            gameObject.SetActive(false);
        }
    }
}
