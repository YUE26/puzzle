using System.Collections.Generic;
using Repo.Localization;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LangTmp))]
public class LangTmpEditor : Editor
{
    private SerializedProperty keyProperty;
    private string[] optionLabels;
    private int[] optionValues;

    private void OnEnable()
    {
        keyProperty = serializedObject.FindProperty("key");
        RefreshOptions();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if (optionValues.Length == 0)
        {
            EditorGUILayout.PropertyField(keyProperty);
            EditorGUILayout.HelpBox("LocalizationUICfgStore is empty. Generate/load CSV first.", MessageType.Warning);
        }
        else
        {
            int currentValue = keyProperty.intValue;
            int selectedValue = EditorGUILayout.IntPopup("key", currentValue, optionLabels, optionValues);
            keyProperty.intValue = selectedValue;

            if (GUILayout.Button("Refresh LocalizationUICfg"))
            {
                RefreshOptions();
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void RefreshOptions()
    {
        EnsureStoreLoaded();

        if (Csv.LocalizationUICfgStore == null || Csv.LocalizationUICfgStore.Count == 0)
        {
            optionLabels = new string[0];
            optionValues = new int[0];
            return;
        }

        var ids = new List<int>(Csv.LocalizationUICfgStore.Keys);
        ids.Sort();

        optionLabels = new string[ids.Count];
        optionValues = new int[ids.Count];

        for (int i = 0; i < ids.Count; i++)
        {
            int id = ids[i];
            var row = Csv.LocalizationUICfgStore[id];
            string ch = row != null && row.ch != null ? row.ch : string.Empty;
            optionLabels[i] = $"{id}-{ch}";
            optionValues[i] = id;
        }
    }

    private static void EnsureStoreLoaded()
    {
        if (Csv.LocalizationUICfgStore != null && Csv.LocalizationUICfgStore.Count > 0)
        {
            return;
        }

        var csv = new Csv();
        csv.InitInEditor();
    }
}
