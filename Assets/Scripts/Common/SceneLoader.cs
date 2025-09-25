using System;
using Common.ServiceLocator;
using UnityEngine.SceneManagement;

namespace Common.SceneManagement
{
    public class SceneLoader : ILocatableService
    {
        public static Action<Scene> OnSceneLoadedCallback;
        private const string MAIN_SCENE = "MainScene";
        private const string GAME_SCENE = "GameScene";
        private Scene previouseScene;

        private void OnEnable()
        {
            CheckEvent(SceneManager.GetActiveScene());
        }

        private void LoadScene(string sceneName, bool forceLoading)
        {
            SceneManager.LoadScene(sceneName);
        }

        public void StartGameScene(bool forceLoading = false)
        {
            LoadScene(GAME_SCENE, forceLoading);
        }

        public void GoToMainMenu()
        {
            LoadScene(MAIN_SCENE, false);
        }

        private void CheckEvent(Scene scene)
        {
            if (previouseScene != scene)
            {
                OnSceneLoadedCallback?.Invoke(scene);
                previouseScene = scene;
            }
        }
    }
}