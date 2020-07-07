using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSystem : MonoBehaviour
{
	public ObstacleSpawner ObstacleSpawner;
	public PlayerSpawner PlayerSpawner;
	public RRTBase RRT;

	public void Start()
	{
		Restart();
	}

	public void Restart()
	{
		ObstacleSpawner.Respawn();
		PlayerSpawner.Respawn();
		RRT.Restart();
	}

	public void ChangeObstacleConfig(Int32 _config)
	{
		ObstacleSpawner.ChangeObstacleConfig(_config);
		Restart();
	}
}
