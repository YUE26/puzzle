using System.Collections.Generic;
using System.IO;
using Core.Event;
using GamePlay.SaveData;
using Newtonsoft.Json;
using Repo.Event;
using UnityEngine;

namespace Core.SaveLoad
{
    public class SaveLoadManager : SingletonMono<SaveLoadManager>
{
    private string folderPath;

    public List<ISaveable> dataList = new List<ISaveable>();

    public Dictionary<string, SaveData> saveDataDic = new Dictionary<string, SaveData>();

    protected override void OnAwake()
    {
        folderPath = Application.persistentDataPath + "/SAVE/";
    }

    private void OnEnable()
    {
        EventModule.AddListener(EventName.EvtStartGameEvent, OnStartGameEvent);
    }

    private void OnDisable()
    {
        EventModule.RemoveListener(EventName.EvtStartGameEvent, OnStartGameEvent);
    }

    public void DoRegister(ISaveable saveable)
    {
        dataList.Add(saveable);
    }


    public void OnStartGameEvent(object obj)
    {
        if (obj is not int) return;
        var resultPath = folderPath + "data.sav";
        if (File.Exists(resultPath))
        {
            File.Delete(resultPath);
        }
    }

    /// <summary>
    /// 序列化存储
    /// </summary>
    public void Serialize()
    {
        saveDataDic.Clear();

        foreach (var saveable in dataList)
        {
            saveDataDic.Add(saveable.GetType().Name, saveable.GenerateSaveData());
        }

        var resultPath = folderPath + "data.sav";
        var jsonData = JsonConvert.SerializeObject(saveDataDic);

        //创建文件夹
        if (File.Exists(resultPath) == false)
        {
            Directory.CreateDirectory(folderPath);
        }

        File.WriteAllText(resultPath, jsonData);
    }

    /// <summary>
    /// 反序列化
    /// </summary>
    public void AntiSerializeObject()
    {
        var resultPath = folderPath + "data.sav";

        if (File.Exists(resultPath) == false) return;

        var stringData = File.ReadAllText(resultPath);
        var jsonData = JsonConvert.DeserializeObject<Dictionary<string, SaveData>>(stringData);

        foreach (var saveable in dataList)
        {
            saveable.ReadGameData(jsonData[saveable.GetType().Name]);
        }
    }
}
}