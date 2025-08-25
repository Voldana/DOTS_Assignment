using Unity.Entities;
using UnityEngine;

namespace Project.Scripts.Authoring
{
    public class CannonAuthoring: MonoBehaviour
    {
        [SerializeField] private GameObject ballPrefab;
        [SerializeField] private float shootSpeed;

        class Baker : Baker<CannonAuthoring>
        {
            public override void Bake(CannonAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent(entity, new Cannon
                {
                    ballPrefab = GetEntity(authoring.ballPrefab, TransformUsageFlags.Dynamic),
                    shootSpeed = authoring.shootSpeed
                });
            }
        }
    }
    
    public struct Cannon : IComponentData
    {
        public Entity ballPrefab;
        public float shootSpeed;
    }
}