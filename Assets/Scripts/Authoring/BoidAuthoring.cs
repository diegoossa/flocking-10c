using Unity.Entities;
using UnityEngine;

public class BoidAuthoring : MonoBehaviour
{
}

public class BoidAuthoringBaker : Baker<BoidAuthoring>
{
    public override void Bake(BoidAuthoring authoring)
    {
        Entity entity = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic);
        AddComponent<Boid>(entity);
        AddBuffer<AllNeighbourData>(entity);
        AddBuffer<TeamNeighbourData>(entity);
    }
}