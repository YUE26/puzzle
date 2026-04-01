using System.Collections.Generic;
using GamePlay.Bag.Data;

namespace GamePlay.SaveData
{
    public partial class SaveData
    {
        public List<ItemDetail> bag = new List<ItemDetail>();
        public int Capacity;
    }
}