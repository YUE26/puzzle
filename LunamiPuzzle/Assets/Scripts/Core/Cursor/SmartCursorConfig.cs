using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Cursor
{
    [Serializable]
    [CreateAssetMenu(fileName = "CursorConfig",menuName = "Core/Cursor/SmartCursorConfig")]
    public class SmartCursorConfig : ScriptableObject
    {
        public List<SmartCursorCfgData> smartCursors = new List<SmartCursorCfgData>();
    }

    [Serializable]
    public class SmartCursorCfgData
    {
        public CursorState state;
        public Sprite cursorImg;
        public Vector2 size;
        public Vector2 pivot;
        public bool isSpine;
        public string spinePath;
    }

    public enum CursorState
    {
        Null,
        Normal,        // 普通
        Interact,      // 交互
        Talk,          // 对话
    }
}