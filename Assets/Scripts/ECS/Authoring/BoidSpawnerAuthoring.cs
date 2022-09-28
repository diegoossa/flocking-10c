using System;
using Unity.Entities;
using UnityEngine;

[Serializable]
public struct BoidAgentSettings
{
    public GameObject prefab;
    public float acceleration;
    public float drag;
}

public class BoidSpawnerAuthoring : MonoBehaviour
{
    [Header("Spawner Settings")] 
    public float initialVelocity = 2.0f;
    public float boidDensity = 4.0f;
    public int roundWorldSizeToMultiplesOf = 5;
    
    [Header("Boid Agents")]
    public BoidAgentSettings[] boidAgents;
}

public class BoidSpawnerAuthoringBaker : Baker<BoidSpawnerAuthoring>
{
    public override void Bake(BoidSpawnerAuthoring authoring)
    {
        AddComponent(new BoidSpawner
        {
            InitialVelocity = authoring.initialVelocity,
            BoidDensity = authoring.boidDensity,
            RoundWorldSizeToMultiplesOf = authoring.roundWorldSizeToMultiplesOf
        });
        var agentBuffer = AddBuffer<BoidAgentData>();
        foreach (var boidAgent in authoring.boidAgents)
        {
            agentBuffer.Add(new BoidAgentData
            {
                BoidAgentEntity = GetEntity(boidAgent.prefab),
                Acceleration = boidAgent.acceleration, 
                Drag = boidAgent.drag
            });
        }
    }
}