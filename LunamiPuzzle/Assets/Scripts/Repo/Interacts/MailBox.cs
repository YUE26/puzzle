using GamePlay.Bag;
using GamePlay.Interaction;

namespace Repo.Interacts
{
    public class MailBox : Interaction
    {
        protected override void OnItemClick()
        {
            base.OnItemClick();
            if (Csv.InteractionCfgStore.TryGetValue(id, out var interactObj))
            {
                ItemManager.Instance.AddItemToBag(interactObj.result);
            }
        }
    }
}