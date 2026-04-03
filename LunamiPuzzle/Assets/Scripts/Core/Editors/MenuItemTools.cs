using System.IO;
using System.Net;
using Core.SaveLoad;
using UnityEditor;
using UnityEngine;

namespace Core.Editor
{
    public class MenuItemTools:EditorWindow
    {
        [MenuItem("Assets/SelectPath")]
        public static void GetSelectPath()
        {
            if (Selection.objects.Length <= 0) return;
            var path = AssetDatabase.GetAssetPath(Selection.objects[0]);
            path = path.Replace("Assets/Resources/", "");
            string[] paths = path.Split(".");
            GUIUtility.systemCopyBuffer = paths[0];
        }

        [MenuItem("Tools/SaveLoad/Clear Save Data")]
        public static void ClearSaveData()
        {
            var savePath =  Application.persistentDataPath + "/SAVE/data.sav";
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }
        }

        [MenuItem("Tools/SaveLoad/Print SaveDt Path")]
        public static void OpenSaveData()
        {
            Debug.Log(Application.persistentDataPath+"/SAVE/");
        }
    }
}