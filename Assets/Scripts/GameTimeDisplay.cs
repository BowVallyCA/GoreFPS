using _Project.Code.Core.Events;
using _Project.Code.Core.ServiceLocator;
using _Project.Code.Gameplay.GameManagement;
using _Project.Code.Gameplay.Scenes;
using TMPro;
using UnityEngine;

namespace DefaultNamespace
{
    public class GameTimeDisplay : MonoBehaviour
    {
        private TMP_Text _timeText;
        [SerializeField] private string _sceneToLoad = "[Game_Over]";

        private void Start()
        {
            _timeText = GetComponent<TMP_Text>();
            EventBus.Instance.Subscribe<GameTimerTickEvent>(this, OnTimerTick);
            EventBus.Instance.Subscribe<GameTimerFinishedEvent>(this, OnTimerFinished);
        }

        private void OnTimerTick(GameTimerTickEvent evt)
        {
            if (_timeText != null)
            {
                _timeText.text = evt.GetFormattedTime();
            }
        }

        private void OnTimerFinished(GameTimerFinishedEvent evt)
        {
            ServiceLocator.Get<SceneService>().LoadScene(_sceneToLoad);
        }
        

        private void OnDestroy()
        {
            EventBus.Instance.Unsubscribe<GameTimerTickEvent>(this);
        }
    }
}