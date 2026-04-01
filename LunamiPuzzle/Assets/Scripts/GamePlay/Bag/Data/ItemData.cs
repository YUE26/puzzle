using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Bag.Data
{
    [System.Serializable]
    public class ItemDetail
    {
        public int itemId;
        public Sprite itemSprite;
        public int count;
        public Countable countable;
    }

    public enum Countable
    {
        UnCountable,
        Countable
    }
}