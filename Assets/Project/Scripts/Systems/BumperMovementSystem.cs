using Project.Scripts.Authoring;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Project.Scripts.Systems
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct BumperMovementSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var job = new BumperMoverJob
            {
                timeElapsed = (float)SystemAPI.Time.ElapsedTime
            };
            job.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct BumperMoverJob : IJobEntity
    {
        public float timeElapsed;

        public void Execute(ref LocalTransform localTransform,
            in Bumper bumper)
        {
            const float tau = 6.28318530718f;
            var u = math.frac((bumper.speed * timeElapsed + bumper.phase) / tau);
            var tri01 = 1f - math.abs(2f * u - 1f);

            var y = bumper.startY + bumper.distance * tri01;

            var p = localTransform.Position;
            p.y = y;
            localTransform.Position = p;
        }
    }
}