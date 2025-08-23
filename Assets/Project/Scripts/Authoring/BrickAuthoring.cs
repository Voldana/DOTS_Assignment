using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Project.Scripts.Authoring
{
    public class BrickAuthoring : MonoBehaviour
    {
        [SerializeField] private BrickKind kind = BrickKind.Normal;
        [SerializeField] private BrickPath path = BrickPath.None;
        [SerializeField] private float despawnDelayAfterHit = 0.6f;
        [SerializeField] private float amplitude = 2f;
        [SerializeField] private float speed = 0.7f;
        [SerializeField] private float score = 10f;
        [SerializeField] private float phase;
        [SerializeField] private int extraBalls;

        class Baker : Baker<BrickAuthoring>
        {
            public override void Bake(BrickAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var score = authoring.kind == BrickKind.Gold ? authoring.score * 10f : authoring.score;
                AddComponent(entity,
                    new Brick
                    {
                        kind = authoring.kind, path = authoring.path,
                        despawnDelayAfterHit = authoring.despawnDelayAfterHit, score = score,
                        extraBalls = authoring.extraBalls
                    });

                if (authoring.path == BrickPath.None) return;
                var pos = authoring.transform.position;
                var origin = new float2(pos.x, pos.z);
                AddComponent(entity,
                    new PathMotion
                    {
                        originXY = origin, amplitude = authoring.amplitude, speed = authoring.speed,
                        phase = authoring.phase
                    });
            }
        }

        public struct Brick : IComponentData
        {
            public BrickKind kind;
            public BrickPath path;
            public int extraBalls;
            public float despawnDelayAfterHit;
            public float score;
        }

        public struct PathMotion : IComponentData
        {
            public float2 originXY;
            public float amplitude;
            public float speed;
            public float phase;
        }
        
        public struct BrickHit : IComponentData
        {
            public float timeLeft;
            public byte  scored;
        }

        public enum BrickKind : byte
        {
            Normal,
            Gold,
            Multiplier
        }

        public enum BrickPath : byte
        {
            None,
            Infinity,
            M,
            Z
        }
    }
}