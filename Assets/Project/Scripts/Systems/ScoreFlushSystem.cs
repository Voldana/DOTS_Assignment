using Project.Scripts.Mono;
using Unity.Entities;

namespace Project.Scripts.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(FixedStepSimulationSystemGroup))]
    public partial struct ScoreFlushSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ScoreAccumulatorTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var scoreEntity = SystemAPI.GetSingletonEntity<ScoreAccumulatorTag>();
            var buffer = state.EntityManager.GetBuffer<ScoreEvent>(scoreEntity);

            var total = 0;
            for (var i = 0; i < buffer.Length; i++)
                total += buffer[i].points;

            if (total != 0)
                ScoreManager.Instance?.AddScore(total);

            buffer.Clear();
        }
    }
}