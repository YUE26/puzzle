using System.Collections.Generic;

namespace Core.UI
{
    public static class PanelPath
    {
        public static readonly Dictionary<PanelName, string> path = new Dictionary<PanelName, string>()
        {
            
        };
    }

    public enum PanelName
    {
        Null,
        TestPanel,
        PopPanel,
        HudPanel,
    }
}