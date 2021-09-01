using Core.Services;
using UI;
using UnityEngine;

namespace Core.States
{
    public class LevelState : IState
    {
        private LevelMenuView _levelMenuView;
        private readonly IGameService _gameService;

        public LevelState(IGameService gameService)
        {
            _gameService = gameService;
        }

        public void OnEnter()
        {
            _levelMenuView = Object.FindObjectOfType<LevelMenuView>();
            _levelMenuView.MainMenu += OnMenu;
        }

        public void OnExit()
        {
            _levelMenuView.MainMenu -= OnMenu;
        }
        
        private void OnMenu()
        {
            _gameService.Fire(GameTrigger.MainMenu);
        }
    }
}