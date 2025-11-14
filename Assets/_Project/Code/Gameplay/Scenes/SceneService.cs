using UnityEngine;
using UnityEngine.SceneManagement;
using _Project.Code.Core.ServiceLocator;
using _Project.Code.Core.Events;

namespace _Project.Code.Gameplay.Scenes
{
    public class SceneService : MonoBehaviourService
    {
        public string CurrentSceneName { get; private set; }

        public override void Initialize()
        {
            CurrentSceneName = SceneManager.GetActiveScene().name;
            Debug.Log($"[SceneService] Initialized. Current scene: {CurrentSceneName}");
        }

        public void LoadScene(string sceneName)
        {
            EventBus.Instance.Publish(new SceneLoadStartedEvent { SceneName = sceneName });
            SceneManager.LoadScene(sceneName);
            CurrentSceneName = sceneName;
        }

        public async void LoadSceneAsync(string sceneName)
        {
            EventBus.Instance.Publish(new SceneLoadStartedEvent { SceneName = sceneName });

            var operation = SceneManager.LoadSceneAsync(sceneName);

            while (!operation.isDone)
            {
                await System.Threading.Tasks.Task.Yield();
            }

            CurrentSceneName = sceneName;
            EventBus.Instance.Publish(new SceneLoadedEvent { SceneName = sceneName });
        }

        public void ReloadCurrentScene()
        {
            LoadScene(CurrentSceneName);
        }

        public override void Dispose()
        {
            // Cleanup if needed
        }
    }
}
