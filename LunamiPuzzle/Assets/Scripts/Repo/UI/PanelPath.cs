using System.Collections.Generic;

namespace Repo.UI
{
    public static class PanelPath
    {
        public static readonly Dictionary<PanelName, string> path = new Dictionary<PanelName, string>()
        {
            { PanelName.MenuPanel , "uiPrefab/Menu/MenuPanel" },
            { PanelName.HudPanel , "uiPrefab/HudPanel" },
            { PanelName.BagPanel , "uiPrefab/Bag/BagPanel" },
        };
    }

    public enum PanelName
    {
        Null,
        MenuPanel,
        HudPanel,
        BagPanel,
    }
}