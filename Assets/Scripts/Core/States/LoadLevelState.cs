using Core.Services;
using Cysharp.Threading.Tasks;
using Data;

namespace Core.States
{
    public class LoadLevelState : LoadSceneStateBase, IState
    {
        private readonly IGameService _gameService;

        public LoadLevelState(IGameService gameService)
        {
            _gameService = gameService;
        }
        
        public void OnEnter()
        {
            var activeLevel = _gameService.ActiveLevel;
            LoadLevel(activeLevel).Forget();
        }

        public void OnExit()
        {
            
        }
        
        private async UniTaskVoid LoadLevel(LevelDefinition levelDefinition)
        {
            await LoadScene(levelDefinition.Scene);
            _gameService.Fire(GameTrigger.Play);
        }
    }
}