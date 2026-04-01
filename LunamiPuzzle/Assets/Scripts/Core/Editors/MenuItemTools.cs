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
    }
}