using Core.Services;
using Cysharp.Threading.Tasks;

namespace Core.States
{
    public class LoadMainMenuState : LoadSceneStateBase, IState
    {
        private readonly IGameService _gameService;

        public LoadMainMenuState(IGameService gameService)
        {
            _gameService = gameService;
        }
        
        public void OnEnter()
        {
            LoadMainMenu().Forget();
        }

        public void OnExit()
        {
            
        }
        
        private async UniTaskVoid LoadMainMenu()
        {
            await LoadScene("MainMenu");
            _gameService.Fire(GameTrigger.MainMenu);
        }
    }
}