using System.Collections.Generic;

namespace Repo.UI
{
    public static class PanelPath
    {
        public static readonly Dictionary<PanelName, string> path = new Dictionary<PanelName, string>()
        {
            { PanelName.MenuPanel , "uiPrefab/MenuPanel" },
            { PanelName.HudPanel , "uiPrefab/HudPanel" },
        };
    }

    public enum PanelName
    {
        Null,
        MenuPanel,
        HudPanel,
    }
}