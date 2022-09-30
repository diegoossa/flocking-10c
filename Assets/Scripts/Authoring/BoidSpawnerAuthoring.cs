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
    [Header("Boid Agents")]
    public BoidAgentSettings[] boidAgents;
}

public class BoidSpawnerAuthoringBaker : Baker<BoidSpawnerAuthoring>
{
    public override void Bake(BoidSpawnerAuthoring authoring)
    {
        AddComponent(new BoidSpawner());
        var agentBuffer = AddBuffer<BoidAgentData>();
        foreach (var boidAgent in authoring.boidAgents)
        {
            agentBuffer.Add(new BoidAgentData
            {
                BoidAgentEntity = GetEntity(boidAgent.prefab),
                Acceleration = boidAgent.acceleration, 
                Drag = boidAgent.drag,
                TeamId = boidAgent.teamId
            });
        }
    }
}