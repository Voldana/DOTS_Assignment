using Unity.Entities;
using UnityEngine;

namespace Project.Scripts.Authoring
{
    public class BumperAuthoring : MonoBehaviour
    {
        [SerializeField] private float distance;
        [SerializeField] private float speed;

        class Baker : Baker<BumperAuthoring>
        {
            public override void Bake(BumperAuthoring authoring)
            {
                var e = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(e,
                    new Bumper
                    {
                        distance = authoring.distance, speed = authoring.speed, phase = 0f,
                        startY = authoring.transform.position.y
                    });
            }
        }
    }

    public struct Bumper : IComponentData
    {
        public float distance, speed, phase, startY;
    }
}