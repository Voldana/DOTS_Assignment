using Project.Scripts.Authoring;
using Unity.Burst;
using Unity.Entities;

namespace Project.Scripts.Systems
{
    [BurstCompile]
    public partial struct BrickDespawnSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<BrickAuthoring.BrickHit>(); 
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
            var deltaTime = SystemAPI.Time.DeltaTime;

            var job = new BrickDespawnJob
            {
                deltaTime = deltaTime,
                writer       = ecb.AsParallelWriter()
            };

            state.Dependency = job.ScheduleParallel(state.Dependency);
        }
    }

    [BurstCompile]
    public partial struct BrickDespawnJob : IJobEntity
    {
        public float deltaTime;
        public EntityCommandBuffer.ParallelWriter writer;
        
        private void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, ref BrickAuthoring.BrickHit hit)
        {
            hit.timeLeft -= deltaTime;
            if (hit.timeLeft <= 0f)
            {
                writer.DestroyEntity(chunkIndex, entity);
            }
        }
    }
}