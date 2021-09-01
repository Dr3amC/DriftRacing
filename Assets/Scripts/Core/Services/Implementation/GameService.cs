using Core.States;
using Data;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Core.Services.Implementation
{
    public class GameService : StateMachine<GameTrigger>, IGameService, IInitializable
    {
        public LevelDefinition ActiveLevel { get; set; }

        public GameService(IDatabaseService databaseService)
        {
            DefineState(() => new MainMenuState(this, databaseService));
            DefineState(() => new LoadLevelState(this));
            DefineState(() => new LevelState(this));
            DefineState(() => new LoadMainMenuState(this));
            
            DefineStartTransition<MainMenuState>(GameTrigger.MainMenu);
            
            DefineTransition<MainMenuState, LoadLevelState>(GameTrigger.Play);
            DefineTransition<LoadLevelState, LevelState>(GameTrigger.Play);
            DefineTransition<LevelState, LoadMainMenuState>(GameTrigger.MainMenu);
            DefineTransition<LoadMainMenuState, MainMenuState>(GameTrigger.MainMenu);
        }

        public void Initialize()
        {
            var activeScene = SceneManager.GetActiveScene();
            if (activeScene.IsValid() && activeScene.name == "MainMenu")
            {
                Fire(GameTrigger.MainMenu);
            }
            else
            {
                Debug.LogError("Unsupported first scene");
            }
        }
    }
}