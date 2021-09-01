using Core.Services;
using UI;
using UnityEngine;

namespace Core.States
{
    public class MainMenuState : IState
    {
        private MainMenuView _mainMenuView;
        private readonly IGameService _gameService;
        private readonly IDatabaseService _databaseService;

        public MainMenuState(IGameService gameService, IDatabaseService databaseService)
        {
            _gameService = gameService;
            _databaseService = databaseService;
        }

        public void OnEnter()
        {
            _mainMenuView = Object.FindObjectOfType<MainMenuView>();
            _mainMenuView.Play += OnPlay;
        }

        public void OnExit()
        {
            _mainMenuView.Play -= OnPlay;
        }
        
        private void OnPlay()
        {
            _gameService.ActiveLevel = _databaseService.Levels[0];
            _gameService.Fire(GameTrigger.Play);
        }
    }
}