using Project.Scripts.Authoring;
using Project.Scripts.Mono;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace Project.Scripts.Systems
{
    public partial struct ShootSystem:ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            var entityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (cannon, transform) in
                     SystemAPI
                         .Query<RefRW<Cannon>, RefRW<LocalTransform>>())
            {
                var cannonPos = transform.ValueRO.Position;

                var target = MouseInput.TargetWorld;
                target.z = cannonPos.z;

                var distance = new float2(target.x - cannonPos.x, target.y - cannonPos.y);
                var length = math.length(distance);

                cannon.ValueRW.cooldown = math.max(0f, cannon.ValueRO.cooldown - deltaTime);

                if (!(length > 1e-4f)) continue;
                var dir2 = distance / math.max(length, 1e-6f);
                var angle = math.atan2(dir2.y, dir2.x);
                transform.ValueRW.Rotation = quaternion.AxisAngle(new float3(0, 0, 1), angle);


                if (!MouseInput.ShootDown || !(cannon.ValueRO.cooldown <= 0f)) continue;
                cannon.ValueRW.cooldown = cannon.ValueRO.cooldown;

                var ball = entityCommandBuffer.Instantiate(cannon.ValueRO.ballPrefab);
                entityCommandBuffer.SetComponent(ball, LocalTransform.FromPositionRotationScale(
                    cannonPos, quaternion.identity, 1f));

                entityCommandBuffer.SetComponent(ball, new PhysicsVelocity
                {
                    Linear = new float3(dir2.x, dir2.y, 0f) * cannon.ValueRO.shootSpeed
                });
            }

            entityCommandBuffer.Playback(state.EntityManager);
            entityCommandBuffer.Dispose();
        }
    }
}