using System.Collections.Generic;
using GamePlay;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LangTmp))]
public class LangTmpEditor : Editor
{
    private SerializedProperty keyProperty;
    private string[] optionLabels;
    private int[] optionValues;
    private string searchText = string.Empty;

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
            searchText = EditorGUILayout.TextField("Search", searchText);

            BuildFilteredOptions(searchText, out string[] filteredLabels, out int[] filteredValues);

            int currentValue = keyProperty.intValue;
            if (filteredValues.Length == 0)
            {
                EditorGUILayout.HelpBox("No LocalizationUICfg matches current search.", MessageType.Info);
                EditorGUILayout.PropertyField(keyProperty);
            }
            else
            {
                int selectedValue = EditorGUILayout.IntPopup("key", currentValue, filteredLabels, filteredValues);
                keyProperty.intValue = selectedValue;
            }

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

    private void BuildFilteredOptions(string keyword, out string[] filteredLabels, out int[] filteredValues)
    {
        if (optionValues == null || optionLabels == null || optionValues.Length == 0)
        {
            filteredLabels = new string[0];
            filteredValues = new int[0];
            return;
        }

        if (string.IsNullOrWhiteSpace(keyword))
        {
            filteredLabels = optionLabels;
            filteredValues = optionValues;
            return;
        }

        string lowerKeyword = keyword.Trim().ToLowerInvariant();
        var labels = new List<string>();
        var values = new List<int>();

        for (int i = 0; i < optionValues.Length; i++)
        {
            if (optionLabels[i].ToLowerInvariant().Contains(lowerKeyword))
            {
                labels.Add(optionLabels[i]);
                values.Add(optionValues[i]);
            }
        }

        filteredLabels = labels.ToArray();
        filteredValues = values.ToArray();
    }
}
