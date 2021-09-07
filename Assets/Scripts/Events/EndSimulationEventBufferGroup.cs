using Unity.Entities;

namespace Events
{
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    public class EndSimulationEventBufferGroup : ComponentSystemGroup
    {
        
    }
}