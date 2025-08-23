using Unity.Entities;
using UnityEngine;

namespace Project.Scripts.Authoring
{
    public class BallAuthoring : MonoBehaviour
    {
        class Baker : Baker<BallAuthoring>
        {
            public override void Bake(BallAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<Ball>(entity);
            }
        }
    }
    
    public struct Ball : IComponentData {}
}
