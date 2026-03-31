using System.Collections.Generic;
using GamePlay.Bag.Logic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Item),true)]
public class ItemEditor : Editor
{
    private SerializedProperty idProperty;
    private string[] optionLabels;
    private int[] optionValues;
    private string searchText = string.Empty;

    private void OnEnable()
    {
        idProperty = serializedObject.FindProperty("id");
        RefreshOptions();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if (optionValues.Length == 0)
        {
            EditorGUILayout.PropertyField(idProperty);
            EditorGUILayout.HelpBox("ItemCfgStore is empty. Generate/load CSV first.", MessageType.Warning);
        }
        else
        {
            searchText = EditorGUILayout.TextField("Search", searchText);

            BuildFilteredOptions(searchText, out string[] filteredLabels, out int[] filteredValues);

            int currentValue = idProperty.intValue;
            if (filteredValues.Length == 0)
            {
                EditorGUILayout.HelpBox("No ItemCfg matches current search.", MessageType.Info);
                EditorGUILayout.PropertyField(idProperty);
            }
            else
            {
                int selectedValue = EditorGUILayout.IntPopup("id", currentValue, filteredLabels, filteredValues);
                idProperty.intValue = selectedValue;
            }

            if (GUILayout.Button("Refresh ItemCfg"))
            {
                RefreshOptions();
            }
        }

        DrawPropertiesExcluding(serializedObject, "m_Script", "id");
        serializedObject.ApplyModifiedProperties();
    }

    private void RefreshOptions()
    {
        EnsureStoreLoaded();

        if (Csv.ItemCfgStore == null || Csv.ItemCfgStore.Count == 0)
        {
            optionLabels = new string[0];
            optionValues = new int[0];
            return;
        }

        var ids = new List<int>(Csv.ItemCfgStore.Keys);
        ids.Sort();

        optionLabels = new string[ids.Count];
        optionValues = new int[ids.Count];

        for (int i = 0; i < ids.Count; i++)
        {
            int id = ids[i];
            var row = Csv.ItemCfgStore[id];
            string comment = row != null && row.comment != null ? row.comment : string.Empty;
            optionLabels[i] = $"{id}-{comment}";
            optionValues[i] = id;
        }
    }

    private static void EnsureStoreLoaded()
    {
        if (Csv.ItemCfgStore != null && Csv.ItemCfgStore.Count > 0)
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
