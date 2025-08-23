using Project.Scripts.Mono;
using Unity.Burst;
using Unity.Entities;

namespace Project.Scripts.Systems
{
    [BurstCompile]
    public partial struct ScoreSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            var entity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponentData(entity, new ScoreAccumulatorTag());
            state.EntityManager.AddBuffer<ScoreEvent>(entity);
        }

    }

    public struct ScoreAccumulatorTag : IComponentData
    {
    }
}