using Project.Scripts.Authoring;
using Project.Scripts.Mono;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;

namespace Project.Scripts.Systems
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(PhysicsSimulationGroup))]
    public partial struct BallCollisionSystem : ISystem
    {
        private ComponentLookup<BrickAuthoring.BrickHit> brickHitLookupRW;
        private ComponentLookup<BrickAuthoring.Brick> brickLookupRO;
        private ComponentLookup<PhysicsVelocity> velLookupRW;
        private ComponentLookup<Bumper> bumperLookupRO;
        private ComponentLookup<Ball> ballLookupRO;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ScoreAccumulatorTag>();
            state.RequireForUpdate<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate<SimulationSingleton>();
            ballLookupRO = state.GetComponentLookup<Ball>(true);
            brickLookupRO = state.GetComponentLookup<BrickAuthoring.Brick>(true);
            bumperLookupRO = state.GetComponentLookup<Bumper>(true);
            brickHitLookupRW = state.GetComponentLookup<BrickAuthoring.BrickHit>();
            velLookupRW = state.GetComponentLookup<PhysicsVelocity>();
        }

        public void OnUpdate(ref SystemState state)
        {
            ballLookupRO.Update(ref state);
            brickLookupRO.Update(ref state);
            bumperLookupRO.Update(ref state);
            brickHitLookupRW.Update(ref state);
            velLookupRW.Update(ref state);

            var sim = SystemAPI.GetSingleton<SimulationSingleton>();

            var ecbSingleton = SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
            var scoreEntity = SystemAPI.GetSingletonEntity<ScoreAccumulatorTag>();

            var job = new BallCollisionJob
            {
                velLookup = velLookupRW,
                ballLookup = ballLookupRO,
                brickLookup = brickLookupRO,
                bumperLookup = bumperLookupRO,
                brickHitLookup = brickHitLookupRW,
                ecb = ecb.AsParallelWriter(),
                scoreEntity = scoreEntity,
                bumperBoost = 7.5f
            };

            state.Dependency = job.Schedule(sim, state.Dependency);
        }
    }

    [BurstCompile]
    public struct BallCollisionJob : ICollisionEventsJob
    {
        public ComponentLookup<PhysicsVelocity> velLookup;

        [ReadOnly] public ComponentLookup<BrickAuthoring.Brick> brickLookup;
        [ReadOnly] public ComponentLookup<Bumper> bumperLookup;
        [ReadOnly] public ComponentLookup<Ball> ballLookup;
        [ReadOnly] public float bumperBoost;


        public ComponentLookup<BrickAuthoring.BrickHit> brickHitLookup;

        public EntityCommandBuffer.ParallelWriter ecb;
        public Entity scoreEntity;

        public void Execute(CollisionEvent ev)
        {
            var a = ev.EntityA;
            var b = ev.EntityB;

            var aBall = ballLookup.HasComponent(a);
            var bBall = ballLookup.HasComponent(b);
            if (!(aBall ^ bBall)) return;

            var ball = aBall ? a : b;
            var other = aBall ? b : a;

            if (!velLookup.HasComponent(ball)) return;
            var n = ev.Normal;
            var v = velLookup[ball];
            var cur = v.Linear;

            if (bumperLookup.HasComponent(other))
            {
                var outDir = math.normalizesafe(cur);
                if (math.dot(cur, n) < -1e-3f)
                    outDir = math.reflect(outDir, n);

                var newSpeed = bumperBoost * 2;

                v.Linear = outDir * newSpeed;
                velLookup[ball] = v;
                return;
            }

            if (!brickLookup.HasComponent(other)) return;
            if (math.dot(cur, n) < -1e-3f)
            {
                var dir = math.normalizesafe(cur);
                var reflDir = math.reflect(dir, n);

                var speed = math.length(cur);
                // speed = math.clamp(speed, 7, 10);

                v.Linear = reflDir * speed;
                velLookup[ball] = v;
            }

            if (brickHitLookup.HasComponent(other)) return;
            var brick = brickLookup[other];
            ecb.AppendToBuffer(0, scoreEntity, new ScoreEvent { points = (int)math.round(brick.score) });
            ecb.AddComponent(0, other,
                new BrickAuthoring.BrickHit { timeLeft = brick.despawnDelayAfterHit, scored = 1 });
        }
    }
}