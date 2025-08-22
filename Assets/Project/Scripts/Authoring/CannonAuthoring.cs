using Unity.Entities;
using UnityEngine;

namespace Project.Scripts.Authoring
{
    public class CannonAuthoring: MonoBehaviour
    {
        [SerializeField] private GameObject ballPrefab;
        [SerializeField] private GameObject barrelTip;
        [SerializeField] private float shootSpeed;
        [SerializeField] private float cooldown;

        class Baker : Baker<CannonAuthoring>
        {
            public override void Bake(CannonAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent(entity, new Cannon
                {
                    ballPrefab = GetEntity(authoring.ballPrefab, TransformUsageFlags.Dynamic),
                    barrelTip = GetEntity(authoring.barrelTip, TransformUsageFlags.Dynamic),
                    shootSpeed = authoring.shootSpeed,
                    cooldown = authoring.cooldown
                });
            }
        }
    }
    
    public struct Cannon : IComponentData
    {
        public Entity ballPrefab;
        public Entity barrelTip;
        public float shootSpeed;
        public float cooldown;
    }
}