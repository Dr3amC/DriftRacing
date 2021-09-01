using Data;

namespace Core.Services
{
    public interface IDatabaseService
    {
        LevelDefinition[] Levels { get; }
    }
}