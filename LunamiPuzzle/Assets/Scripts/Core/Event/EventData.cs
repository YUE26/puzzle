using Core.Bag.Data;

namespace Core.Event
{
    public class EvtItemUpdateData
    {
        public ItemDetail itemDetail;
        public int index;
    }

    public class EvtItemClickData
    {
        public ItemDetail itemDetail;
        public bool isSelect;
    }
}