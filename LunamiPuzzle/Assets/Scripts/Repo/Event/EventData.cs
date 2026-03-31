using GamePlay.Bag.Data;

namespace Repo.Event
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