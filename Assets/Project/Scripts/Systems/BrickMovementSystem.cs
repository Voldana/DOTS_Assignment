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
        public void OnUpdate(ref SystemState state)
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
                        const float tau = 6.28318530718f;
                        var u = math.frac((speed * timeElapsed + phase) / tau);

                        var seg = (int)math.floor(u * 8f);
                        var s01 = math.frac(u * 8f);

                        var ay = amplitude * 0.6f;

                        ay = -ay;

                        var p0 = new float2(-amplitude, +ay);
                        var p1 = new float2(-amplitude * 0.5f, -ay);
                        var p2 = new float2(0f, +ay);
                        var p3 = new float2(+amplitude * 0.5f, -ay);
                        var p4 = new float2(+amplitude, +ay);

                        float2 a, b;
                        switch (seg)
                        {
                            case 0:
                                a = p0;
                                b = p1;
                                break;
                            case 1:
                                a = p1;
                                b = p2;
                                break;
                            case 2:
                                a = p2;
                                b = p3;
                                break;
                            case 3:
                                a = p3;
                                b = p4;
                                break;
                            case 4:
                                a = p4;
                                b = p3;
                                break;
                            case 5:
                                a = p3;
                                b = p2;
                                break;
                            case 6:
                                a = p2;
                                b = p1;
                                break;
                            default:
                                a = p1;
                                b = p0;
                                break;
                        }

                        newXY = startPoint + math.lerp(a, b, s01);
                    }
                        break;

                    case BrickAuthoring.BrickPath.Z:
                    {
                        const float tau = 6.28318530718f;
                        var u = math.frac((speed * timeElapsed + phase) / tau);
                        var seg = (int)math.floor(u * 6f);
                        var s01 = math.frac(u * 6f);

                        var ay = amplitude * 0.6f;

                        var tl = new float2(-amplitude, +ay);
                        var tr = new float2(+amplitude, +ay);
                        var bl = new float2(-amplitude, -ay);
                        var br = new float2(+amplitude, -ay);

                        float2 a, b;
                        switch (seg)
                        {
                            case 0:
                                a = tl;
                                b = tr;
                                break;
                            case 1:
                                a = tr;
                                b = bl;
                                break;
                            case 2:
                                a = bl;
                                b = br;
                                break;
                            case 3:
                                a = br;
                                b = bl;
                                break;
                            case 4:
                                a = bl;
                                b = tr;
                                break;
                            default:
                                a = tr;
                                b = tl;
                                break;
                        }

                        newXY = startPoint + math.lerp(a, b, s01);
                        break;
                    }

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