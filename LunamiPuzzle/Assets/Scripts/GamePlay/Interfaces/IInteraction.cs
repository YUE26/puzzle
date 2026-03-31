using UnityEngine.EventSystems;

namespace GamePlay.Interfaces
{
    public interface IInteraction: IPointerClickHandler
    {
        public void ItemClick();
    }
}