using System.Collections;
using Core.Event;
using Core.SaveLoad;
using GamePlay.Bag;
using GamePlay.SaveData;
using Repo.Event;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.Transition
{
    public class TransitionManager : SingletonMono<TransitionManager>, ISaveable
    {
        public CanvasGroup fadeCanvasGroup;
        public float fadeDuration;

        private bool isFade;
        private string curScene;
        [SceneName]
        [SerializeField]
        private string startScene;
        private bool canTransition;

        private void Start()
        {
            ISaveable saveable = this;
            saveable.Register();
        }

        private void OnEnable()
        {
            EventModule.AddListener(EventName.EvtUpdateGameState, OnUpdateGameStateEvent);
            EventModule.AddListener(EventName.EvtStartGameEvent, OnStartNewGameEvent);
        }

        private void OnDisable()
        {
            EventModule.RemoveListener(EventName.EvtUpdateGameState, OnUpdateGameStateEvent);
            EventModule.RemoveListener(EventName.EvtStartGameEvent, OnStartNewGameEvent);
        }

        /// <summary>
        /// 新游戏进入的场景
        /// </summary>
        /// <param name="week"></param>
        private void OnStartNewGameEvent(object obj)
        {
            if (obj is not int) return;
            StartCoroutine(TransitionScene("Menu", startScene));
        }

        private void OnUpdateGameStateEvent(object obj)
        {
            if (obj is not GameState gameState) return;
            canTransition = gameState == GameState.GamePlay;
        }

        public void Transition(string from, string to)
        {
            if (isFade == false && canTransition)
            {
                StartCoroutine(TransitionScene(from, to));
            }
        }

        private IEnumerator TransitionScene(string from, string to)
        {
            if (from != string.Empty)
            {
                EventModule.Dispatch(EventName.EvtBeforeUnloadScene);
                yield return Fade(1.0f);
                yield return SceneManager.UnloadSceneAsync(from);
            }

            yield return SceneManager.LoadSceneAsync(to, LoadSceneMode.Additive);

            var scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            SceneManager.SetActiveScene(scene);
            EventModule.Dispatch(EventName.EvtAfterLoadScene);
            yield return Fade(0f);
        }

        private IEnumerator Fade(float targetAlpha)
        {
            isFade = true;
            fadeCanvasGroup.blocksRaycasts = true;

            var speed = Mathf.Abs(fadeCanvasGroup.alpha - targetAlpha) / fadeDuration;

            while (Mathf.Approximately(fadeCanvasGroup.alpha, targetAlpha) == false)
            {
                fadeCanvasGroup.alpha = Mathf.MoveTowards(fadeCanvasGroup.alpha, targetAlpha, speed * Time.deltaTime);
                yield return null;
            }

            isFade = false;
            fadeCanvasGroup.blocksRaycasts = false;
        }

        public SaveData GenerateSaveData()
        {
            SaveData data = new SaveData();
            data.curScene = SceneManager.GetActiveScene().name;
            return data;
        }

        public void ReadGameData(SaveData gameData)
        {
            curScene = gameData.curScene;

            Transition("Menu", curScene);
        }
    }
}