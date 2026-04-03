using System.Collections.Generic;
using Core.Input;
using Core.Logger;
using Core.UI;
using Repo.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Cursor
{
    public class SmartCursor : SingletonMono<SmartCursor>
    {
        private RectTransform rectTransform;
        private Canvas parentCanvas;
        private CursorState currentState = CursorState.Null;
        private Dictionary<CursorState, SmartCursorCfgData> cursorDict = new Dictionary<CursorState, SmartCursorCfgData>();

        [SerializeField]
        private SmartCursorConfig cursorConfig;

        [SerializeField]
        private Image cursorImg;

        [SerializeField]
        private RectTransform cursorImgRect;

        protected override void OnAwake()
        {
            rectTransform = GetComponent<RectTransform>();
            parentCanvas = GetComponentInParent<Canvas>();
            InitializeCursorDict();
        }

        private void Start()
        {
            UnityEngine.Cursor.visible = false;
            SetCursorState(CursorState.Normal);
        }

        private void InitializeCursorDict()
        {
            cursorDict.Clear();
            foreach (var data in cursorConfig.smartCursors)
            {
                if (!cursorDict.ContainsKey(data.state))
                {
                    cursorDict.Add(data.state, data);
                }
                else
                {
                    GameLogger.Warning("重复的鼠标状态:{0}", data.state);
                }
            }
        }

        void Update()
        {
            UnityEngine.Cursor.visible = false;
            Vector2 movePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.transform as RectTransform,
                InputModule.Instance.MousePosition,
                null,
                out movePos
            );
            rectTransform.anchoredPosition = movePos;

            CursorDetect();
        }

        private void CursorDetect()
        {
            Vector2 mousePos = CameraControl.Instance.cameraMain.ScreenToWorldPoint(InputModule.Instance.MousePosition);
            RaycastHit2D interactHit =
                Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, 1 << LayerMask.NameToLayer("Interact"));
          
            if (interactHit.collider != null)
            {
                SetCursorState(CursorState.Interact);
            }
            else
            {
                SetCursorState(CursorState.Normal);
            }
        }


        public void SetCursorState(CursorState state)
        {
            if (currentState == state) return;

            if (cursorDict.TryGetValue(state, out SmartCursorCfgData data))
            {
                if (data.isSpine == false)
                {
                    if (data.cursorImg != null)
                    {
                        cursorImg.sprite = data.cursorImg;
                        cursorImgRect.pivot = data.pivot;
                        cursorImgRect.sizeDelta = data.size;
                        currentState = state;
                    }
                    else
                    {
                        GameLogger.Warning("鼠标状态{0}的贴图为空", state);
                        SetDefaultCursor();
                    }
                }
                else
                {
                    // 是spine动画的情况
                }
            }
            else
            {
                GameLogger.Warning("未找到鼠标状态{0}", state);
                SetDefaultCursor();
            }
        }

        public void SetDefaultCursor()
        {
            UnityEngine.Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            currentState = CursorState.Normal;
        }
    }
}