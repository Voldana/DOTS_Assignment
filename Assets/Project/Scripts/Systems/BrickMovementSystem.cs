using System;
using Project.Scripts.Authoring;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace Project.Scripts.Systems
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(PhysicsSimulationGroup))]
    public partial struct BrickMovementSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState s)
        {
            var timeElapsed = (float)SystemAPI.Time.ElapsedTime;

            foreach (var (lt, brick, path, entity) in
                     SystemAPI
                         .Query<RefRW<LocalTransform>, RefRO<BrickAuthoring.Brick>, RefRO<BrickAuthoring.PathMotion>>()
                         .WithEntityAccess())
            {
                var startPoint = path.ValueRO.originXY;
                var amplitude = path.ValueRO.amplitude;
                var speed = path.ValueRO.speed;
                var phase = path.ValueRO.phase;

                float2 newXY;
                switch (brick.ValueRO.path)
                {
                    case BrickAuthoring.BrickPath.Infinity:
                        newXY = startPoint + new float2(
                            amplitude * math.sin(speed * timeElapsed + phase),
                            (amplitude * 0.6f) * math.sin(2f * speed * timeElapsed + phase));
                        break;

                    case BrickAuthoring.BrickPath.M:
                    {
                        var m = math.sin(speed * timeElapsed + phase);
                        newXY = startPoint + new float2(
                            amplitude * math.sign(m) * math.pow(math.abs(m), 0.5f),
                            (amplitude * 0.4f) * (1f - math.abs(m)));
                    }
                        break;

                    case BrickAuthoring.BrickPath.Z:
                    {
                        var saw = 2f * (timeElapsed * speed - math.floor(0.5f + timeElapsed * speed));
                        newXY = startPoint + new float2(
                            amplitude * saw,
                            amplitude * 0.2f * math.sin(2f * speed * timeElapsed + phase));
                    }
                        break;

                    case BrickAuthoring.BrickPath.None:
                        newXY = new float2(lt.ValueRO.Position.x, lt.ValueRO.Position.y);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                var pos = lt.ValueRO.Position;
                pos.x = newXY.x;
                pos.y = newXY.y; 
                lt.ValueRW.Position = pos;
            }
        }
    }
}