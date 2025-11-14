using UnityEngine;
using _Project.Code.Core.ServiceLocator;

namespace _Project.Code.Gameplay.GameManagement
{
    /// <summary>
    /// Place this in a scene to specify which GameState it should start in.
    /// If not present, defaults to GameplayState.
    /// </summary>
    public class SceneGameState : MonoBehaviour
    {
        [SerializeField] private GameStateType _initialState = GameStateType.Gameplay;

        private void Start()
        {
            var gameManagement = ServiceLocator.Get<GameManagementService>();

            switch (_initialState)
            {
                case GameStateType.Gameplay:
                    gameManagement.TransitionToGameplay();
                    break;
                case GameStateType.Menu:
                    gameManagement.TransitionToMenu();
                    break;
                case GameStateType.Paused:
                    gameManagement.TransitionToPaused();
                    break;
            }
        }
    }

    public enum GameStateType
    {
        Gameplay,
        Menu,
        Paused
    }
}
