using Project.Scripts.Authoring;
using Project.Scripts.Mono;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace Project.Scripts.Systems
{
    [UpdateInGroup(typeof(TransformSystemGroup))]
    public partial struct ShootSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (cannon, transform) in
                     SystemAPI.Query<RefRW<Cannon>, RefRW<LocalTransform>>())
            {
                if (!Aim(ref transform.ValueRW, transform.ValueRO.Position, out var dir2))
                    continue;

                if (!MouseInput.ShootDown)
                    continue;

                Shoot(ecb, in cannon.ValueRO, in transform.ValueRO, in dir2);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        private bool Aim(ref LocalTransform cannonXform, float3 cannonPos, out float2 dir2)
        {
            var target = MouseInput.TargetWorld;
            target.z = cannonPos.z;

            var delta = new float2(target.x - cannonPos.x, target.y - cannonPos.y);
            var len = math.length(delta);
            if (!(len > 1e-4f))
            {
                dir2 = default;
                return false;
            }

            dir2 = delta / math.max(len, 1e-6f);
            var angle = math.atan2(dir2.y, dir2.x) + math.radians(90f);
            cannonXform.Rotation = quaternion.AxisAngle(new float3(0, 0, 1), angle);
            return true;
        }

        private void Shoot(EntityCommandBuffer ecb, in Cannon cannon, in LocalTransform cannonXformRO, in float2 dir2)
        {
            var ball = ecb.Instantiate(cannon.ballPrefab);

            ecb.SetComponent(ball, LocalTransform.FromPositionRotationScale(
                cannonXformRO.Position, quaternion.identity, 1f));

            ecb.SetComponent(ball, new PhysicsVelocity
            {
                Linear = new float3(dir2.x, dir2.y, 0f) * cannon.shootSpeed
            });
        }
    }
}
