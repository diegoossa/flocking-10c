using System;
using Unity.Entities;
using UnityEngine;

[Serializable]
public struct BoidAgentSettings
{
    public GameObject prefab;
    public float acceleration;
    public float drag;
    public int teamId;
}

public class BoidSpawnerAuthoring : MonoBehaviour
{
    public BoidAgentSettings[] boidAgents;
}

public class BoidSpawnerAuthoringBaker : Baker<BoidSpawnerAuthoring>
{
    public override void Bake(BoidSpawnerAuthoring authoring)
    {
        Entity entity = GetEntity(authoring.gameObject, TransformUsageFlags.WorldSpace);
        AddComponent(entity, new BoidSpawner());
        var agentBuffer = AddBuffer<BoidAgentData>(entity);
        foreach (var boidAgent in authoring.boidAgents)
        {
            agentBuffer.Add(new BoidAgentData
            {
                BoidAgentEntity = GetEntity(boidAgent.prefab, TransformUsageFlags.Dynamic),
                Acceleration = boidAgent.acceleration, 
                Drag = boidAgent.drag,
                TeamId = boidAgent.teamId
            });
        }
    }
}