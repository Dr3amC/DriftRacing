using Data;

namespace Core.Services
{
    public interface IGameService
    {
        void Fire(GameTrigger gameTrigger);
        LevelDefinition ActiveLevel { get; set; }
    }

    public enum GameTrigger
    {
        Play,
        MainMenu
    }
}