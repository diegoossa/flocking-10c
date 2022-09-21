using UnityEngine;
using System.Collections.Generic;

public static class BoidBehaviour
{
	public interface IBoid
	{
		Vector3 Position { get; set; }
		Vector3 Velocity { get; set; }
	}

	public static void UpdateCoherence(IBoid boid, List<IBoid> neighbours, float coherenceRate, float dt)
	{
		if (neighbours.Count > 0)
		{
			Vector3 center = neighbours[0].Position;
			for (int i = 1; i < neighbours.Count; ++i)
			{
				center += neighbours[i].Position;
			}
			center *= 1.0f / neighbours.Count;
			boid.Velocity += (center - boid.Position) * coherenceRate * dt;
		}
	}

	public static void AvoidOthers(IBoid boid, float minDist, List<IBoid> neighbours, float avoidanceRate, float dt)
	{
		if (neighbours.Count > 0)
		{
			var myPosition = boid.Position;
			var minDistSqr = minDist * minDist;
			Vector3 step = Vector3.zero;
			for (int i = 0; i < neighbours.Count; ++i)
			{
				var delta = myPosition - neighbours[i].Position;
				var deltaSqr = delta.sqrMagnitude;
				if (deltaSqr > 0 && deltaSqr < minDistSqr)
				{
					step += delta / Mathf.Sqrt(deltaSqr);
				}
			}
			boid.Velocity += step * avoidanceRate * dt;
		}
	}

	public static void MatchVelocity(IBoid boid, List<IBoid> neighbours, float matchRate, float dt)
	{
		if (neighbours.Count > 0)
		{
			Vector3 velocity = Vector3.zero;
			for (int i = 0; i < neighbours.Count; ++i)
			{
				velocity += neighbours[i].Velocity;
			}
			velocity /= neighbours.Count;
			boid.Velocity += (velocity - boid.Velocity) * matchRate * dt;
		}
	}

	public static void AvoidInsideBoundsOfCube(IBoid boid, Vector3 halfCubeSize, float avoidRange, float dt)
	{
		boid.Velocity -= new Vector3(
			Mathf.Max(Mathf.Abs(boid.Position.x) - halfCubeSize.x + avoidRange, 0) * Mathf.Sign(boid.Position.x) * 5f * dt,
			Mathf.Max(Mathf.Abs(boid.Position.y) - halfCubeSize.y + avoidRange, 0) * Mathf.Sign(boid.Position.y) * 5f * dt,
			Mathf.Max(Mathf.Abs(boid.Position.z) - halfCubeSize.z + avoidRange, 0) * Mathf.Sign(boid.Position.z) * 5f * dt);
	}
}