using UnityEngine;
using _Project.Code.Core.ServiceLocator;
using _Project.Code.Core.Events;
using _Project.Code.Gameplay.PlayerControllers.Base;

namespace _Project.Code.Gameplay.Player
{
    public class PlayerRegisteredEvent : IEvent
    {
        public Transform Player { get; set; }
        public BasePlayerController Controller { get; set; }
    }
    

    public class PlayerService : MonoBehaviourService
    {
        private Transform _activePlayer;
        private BasePlayerController _activeController;

        public Transform ActivePlayer => _activePlayer;
        public BasePlayerController ActiveController => _activeController;

        public override void Initialize()
        {
            Debug.Log("PlayerService initialized");
        }

        public void RegisterPlayer(BasePlayerController controller)
        {
            if (controller == null) return;

            _activePlayer = controller.transform;
            _activeController = controller;

            Debug.Log($"Player registered: {controller.name}");

            // Notify other systems that player changed
            EventBus.Instance.Publish(new PlayerRegisteredEvent
            {
                Player = _activePlayer,
                Controller = _activeController
            });
        }

        public void UnregisterPlayer(BasePlayerController controller)
        {
            if (_activeController == controller)
            {
                _activePlayer = null;
                _activeController = null;
                Debug.Log($"Player unregistered: {controller.name}");
            }
        }

        public Transform GetPlayerTransform() => _activePlayer;

        public Vector3 GetPlayerPosition() => _activePlayer != null ? _activePlayer.position : Vector3.zero;

        public Vector3 GetPlayerForward() => _activePlayer != null ? _activePlayer.forward : Vector3.forward;

        public Vector3 GetPlayerRight() => _activePlayer != null ? _activePlayer.right : Vector3.right;

        public override void Dispose()
        {
            _activePlayer = null;
            _activeController = null;
        }
    }
}