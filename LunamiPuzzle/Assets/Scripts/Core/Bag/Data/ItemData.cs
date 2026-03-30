using System.Collections.Generic;
using UnityEngine;

namespace Core.Bag.Data
{
    [CreateAssetMenu(fileName = "ItemDataList_So",menuName = "Bag/ItemDataList_So")]
    public class ItemData: ScriptableObject
    {
        public List<ItemDetail> itemList;

        public ItemDetail GetItemDetail(ItemEnums itemName)
        {
            return itemList.Find(i => i.itemName == itemName);
        }
    }

    [System.Serializable]
    public class ItemDetail
    {
        public ItemEnums itemName;
        public Sprite itemSprite;
    }
}