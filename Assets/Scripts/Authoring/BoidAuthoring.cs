using Unity.Entities;
using UnityEngine;

public class BoidAuthoring : MonoBehaviour
{
}

public class BoidAuthoringBaker : Baker<BoidAuthoring>
{
    public override void Bake(BoidAuthoring authoring)
    {
        AddComponent<Boid>();
        AddBuffer<AllNeighbours>();
        AddBuffer<TeamNeighbours>();
    }
}