using GamePlay.SaveData;

namespace Core.SaveLoad
{
    public interface ISaveable:ICore
    {
        public void Register()
        {
            SaveLoadManager.Instance.DoRegister(this);
        }

        /// <summary>
        /// 生成对应的数据保存类
        /// </summary>
        /// <returns></returns>
        public SaveData GenerateSaveData();

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="gameData"></param>
        public void ReadGameData(SaveData gameData);
    }
}