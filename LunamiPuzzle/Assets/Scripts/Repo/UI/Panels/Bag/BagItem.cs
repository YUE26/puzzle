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
            isClick = false;
            currentDetail = itemDetail;
            if (currentDetail == null)
            {
                IsEmpty = true;
                item.gameObject.SetActive(false);
            }
            else
            {
                IsEmpty = false;
                item.gameObject.SetActive(true);
                item.sprite = currentDetail.itemSprite;
                item.transform.localPosition = Vector3.zero;
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
                item.color = Color.white;
                return;
            }

            var inHand = ItemManager.Instance.itemInHand;
            if (ReferenceEquals(inHand, currentDetail))
            {
                isClick = false;
                item.color = Color.white;
                ItemManager.Instance.ReleaseHand();
            }
            else
            {
                isClick = true;
                ItemManager.Instance.SelectItemInHand(currentDetail);
                item.color = Color.black;
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
