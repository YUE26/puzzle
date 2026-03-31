namespace GamePlay.MiniGame
{
    public interface IMiniGame
    {
        public void InitMiniGame();
        public void ResetMiniGame();
        public void ChooseGameData(int week);
        public void CheckGameStateEvent(object obj);
    }
}