using UnityEngine;
using System.Collections.Generic;

public class BoidSimulatorOld
{
	public const float INITIAL_VELOCITY = 2.0f;
	public const float MATCH_VELOCITY_RATE = 1.0f;
	public const float AVOIDANCE_RANGE = 2.0f;
	public const float AVOIDANCE_RATE = 5.0f;
	public const float COHERENCE_RATE = 2.0f;
	public const float VIEW_RANGE = 3.0f;

	public struct Boid: BoidBehaviour.IBoid
	{
		public Vector3 Position { get; set; }
		public Vector3 Velocity { get; set; }
		public int Team;
	}

	public struct Team
	{
		public float Acceleration;
		public float Drag;
	}

	public Boid[] Boids => m_boids;

	private Boid[] m_boids;
	private Team[] m_teams;
	private Vector3 m_halfWorldSize;

	
	public void Reset(Vector3 size, uint boidCount, Team[] teams) 
	{
		m_halfWorldSize = size * .5f;

		m_teams = teams;

		// Spawn random boids
		m_boids = new Boid[boidCount];
		var halfSpawnRange = size * .5f - new Vector3(3, 3, 3);
		for (int i = 0; i < boidCount; ++i) 
		{
			m_boids[i] = new Boid
			{
				Position = new Vector3(Random.Range(-halfSpawnRange.x, halfSpawnRange.x), Random.Range(-halfSpawnRange.y, halfSpawnRange.y), Random.Range(-halfSpawnRange.z, halfSpawnRange.z)),
				Velocity = RandomUnitVector3() * INITIAL_VELOCITY,
				Team = Random.Range(0, teams.Length),
			};
		}
	}

	public void StepSimulation(float dt)
	{
		// Calculate updated velocities
		var updatedVelocities = new Vector3[m_boids.Length];
		for (int boidIndex = 0; boidIndex < m_boids.Length; ++boidIndex)
		{
			BoidBehaviour.IBoid boid = m_boids[boidIndex];
			int team = m_boids[boidIndex].Team;

			// Update behaviour
			FindNeighbours(boidIndex, VIEW_RANGE, out List<BoidBehaviour.IBoid> allWithinRadius, out List<BoidBehaviour.IBoid> sameTeamWithinRadius);
			BoidBehaviour.AvoidInsideBoundsOfCube(boid, m_halfWorldSize, VIEW_RANGE, dt);
			BoidBehaviour.MatchVelocity(boid, sameTeamWithinRadius, MATCH_VELOCITY_RATE, dt);
			BoidBehaviour.UpdateCoherence(boid, sameTeamWithinRadius, COHERENCE_RATE, dt);
			BoidBehaviour.AvoidOthers(boid, AVOIDANCE_RANGE, allWithinRadius, AVOIDANCE_RATE, dt);

			// Acceleration and drag
			Vector3 velocity = boid.Velocity;
			velocity += velocity.normalized * (m_teams[team].Acceleration * dt);
			velocity *= 1.0f - 30.0f * m_teams[team].Drag * dt;

			updatedVelocities[boidIndex] = velocity;
		}

		// Apply updated velocities and update positions
		for (int boidIndex = 0; boidIndex < m_boids.Length; ++boidIndex)
		{
			var boid = m_boids[boidIndex];
			boid.Velocity = updatedVelocities[boidIndex];
			boid.Position += boid.Velocity * dt;
			m_boids[boidIndex] = boid;
		}
	}

	private void FindNeighbours(int sourceBoidIndex, float radius, out List<BoidBehaviour.IBoid> allWithinRadius, out List<BoidBehaviour.IBoid> sameTeamWithinRadius)
	{
		allWithinRadius = new List<BoidBehaviour.IBoid>();
		sameTeamWithinRadius = new List<BoidBehaviour.IBoid>();

		var sourceBoid = m_boids[sourceBoidIndex];

		for (int i = 0; i < m_boids.Length; i++)
		{
			if (i != sourceBoidIndex)
			{
				Vector3 dif = m_boids[i].Position - sourceBoid.Position;
				if (dif.magnitude < radius)
				{
					allWithinRadius.Add(m_boids[i]);
					if (sourceBoid.Team == m_boids[i].Team)
					{
						sameTeamWithinRadius.Add(m_boids[i]);
					}
				}
			}
		}
	}

	private static Vector3 RandomUnitVector3()
	{
		float a = Random.Range(0, 2f * Mathf.PI);
		float z = Random.Range(-1, 1);
		float h = Mathf.Sqrt(1f - z * z);
		return new Vector3(h * Mathf.Cos(a), h * Mathf.Sin(a), z);
	}
}