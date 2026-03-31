using Core.Event;
using Core.SaveLoad;
using Core.UI;
using Repo.Event;
using UnityEngine;

namespace Repo.UI.Panels
{
    public class MenuPanel: UIBase
    {
        public override PanelName panelName => PanelName.MenuPanel;
        
        public void QuitGame()
        {
            UIModule.Instance.CloseUI();
            SaveLoadManager.Instance.Serialize();
            Application.Quit();
        }

        /// <summary>
        /// 继续游戏
        /// </summary>
        public void Continue()
        {
            SaveLoadManager.Instance.AntiSerializeObject();
            UIModule.Instance.CloseUI();
        }

        /// <summary>
        /// 新游戏
        /// </summary>
        /// <param name="gameWeek"></param>
        public void StartGameWeek(int gameWeek)
        {
            EventModule.Dispatch(EventName.EvtStartGameEvent, gameWeek);
            UIModule.Instance.CloseUI();
        }
    }
}