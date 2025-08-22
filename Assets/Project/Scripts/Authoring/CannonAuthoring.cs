using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Project.Scripts.Authoring
{
    public class CannonAuthoring: MonoBehaviour
    {
        [SerializeField] private GameObject ballPrefab;
        [SerializeField] private float shootSpeed;
        [SerializeField] private float cooldown;

        class Baker : Baker<CannonAuthoring>
        {
            public override void Bake(CannonAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Cannon
                {
                    shootSpeed = authoring.shootSpeed,
                    cooldown = authoring.cooldown,
                    ballPrefab = GetEntity(authoring.ballPrefab, TransformUsageFlags.Dynamic)
                });
            }
        }
    }
    
    public struct Cannon : IComponentData
    {
        public Entity ballPrefab;
        public float shootSpeed;
        public float cooldown;
    }
}