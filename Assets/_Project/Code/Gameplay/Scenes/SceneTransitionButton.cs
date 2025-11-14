using UnityEngine;
using UnityEngine.UI;
using _Project.Code.Core.ServiceLocator;

namespace _Project.Code.Gameplay.Scenes
{
    [RequireComponent(typeof(Button))]
    public class SceneTransitionButton : MonoBehaviour
    {
        [SerializeField] private string _sceneToLoad;
        [SerializeField] private bool _useAsync = false;

        private Button _button;
        private SceneService _sceneService;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnButtonClicked);
        }

        private void Start()
        {
            _sceneService = ServiceLocator.Get<SceneService>();
        }

        private void OnButtonClicked()
        {
            if (_sceneService == null)
            {
                Debug.LogError("[SceneTransitionButton] SceneService not found!");
                return;
            }

            if (string.IsNullOrEmpty(_sceneToLoad))
            {
                Debug.LogError("[SceneTransitionButton] Scene name is empty!");
                return;
            }

            if (_useAsync)
            {
                _sceneService.LoadSceneAsync(_sceneToLoad);
            }
            else
            {
                _sceneService.LoadScene(_sceneToLoad);
            }
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(OnButtonClicked);
            }
        }
    }
}
