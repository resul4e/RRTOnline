using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ObstacleSpawner : MonoBehaviour
{
	public enum ObstaclesConfiguration
	{
		Random,
		Narrow,
		U
	}

	/// <summary>
	/// How many obstacles are spawned.
	/// </summary>
	public uint ObstacleAmount = 1500;

	public ObstaclesConfiguration ObstacleConfig;

	public ReadOnlyCollection<GameObject> Obstacles => m_obstacles.AsReadOnly();

	public GameObject ObstaclePrefab;

	// Start is called before the first frame update
	void Start()
	{
		float size = Camera.main.orthographicSize;
		float aspect = Camera.main.aspect;

		m_range = new Vector2((size * aspect) - ObstaclePrefab.transform.localScale.x,
			size - ObstaclePrefab.transform.localScale.y);
	}

	public void Respawn()
	{
		foreach (var obs in m_obstacles)
		{
			Destroy(obs.gameObject);
		}

		m_obstacles.Clear();

		switch (ObstacleConfig)
		{
			case ObstaclesConfiguration.Random:
				RespawnRandom();
				break;
			case ObstaclesConfiguration.Narrow:
				RespawnNarrow();
				break;
			case ObstaclesConfiguration.U:
				RespawnU();
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	private void RespawnU()
	{
		var obs = Instantiate(ObstaclePrefab, new Vector3(0, 6, 0), Quaternion.identity);
		obs.transform.localScale = new Vector3(20, 3, 1);
		m_obstacles.Add(obs);

		obs = Instantiate(ObstaclePrefab, new Vector3(0, -6, 0), Quaternion.identity);
		obs.transform.localScale = new Vector3(20, 3, 1);
		m_obstacles.Add(obs);

		obs = Instantiate(ObstaclePrefab, new Vector3(8.5f, 0, 0), Quaternion.identity);
		obs.transform.localScale = new Vector3(3, 9, 1);
		m_obstacles.Add(obs);
	}

	void RespawnRandom()
	{
		//Spawn 25 obstacles at random location within the given Range.
		for (int i = 0; i < ObstacleAmount; i++)
		{
			var obs = Instantiate(ObstaclePrefab,
				new Vector3(Random.Range(-m_range.x, m_range.x), Random.Range(-m_range.y, m_range.y), 0),
				Quaternion.identity);
			m_obstacles.Add(obs);
		}
	}

	void RespawnNarrow()
	{
		float passageWidth = 3;

		var obs = Instantiate(ObstaclePrefab, new Vector3(m_range.x / 2, m_range.y / 2, 0), Quaternion.identity);
		obs.transform.localScale = new Vector3((m_range.x) - passageWidth, (m_range.y) - passageWidth, 1);
		m_obstacles.Add(obs);

		obs = Instantiate(ObstaclePrefab, new Vector3(-m_range.x / 2, -m_range.y / 2, 0), Quaternion.identity);
		obs.transform.localScale = new Vector3((m_range.x) - passageWidth, (m_range.y) - passageWidth, 1);
		m_obstacles.Add(obs);

		obs = Instantiate(ObstaclePrefab, new Vector3(-m_range.x / 2, m_range.y / 2, 0), Quaternion.identity);
		obs.transform.localScale = new Vector3((m_range.x) - passageWidth, (m_range.y) - passageWidth, 1);
		m_obstacles.Add(obs);

		obs = Instantiate(ObstaclePrefab, new Vector3(m_range.x / 2, -m_range.y / 2, 0), Quaternion.identity);
		obs.transform.localScale = new Vector3((m_range.x) - passageWidth, (m_range.y) - passageWidth, 1);
		m_obstacles.Add(obs);
	}

	public void ChangeObstacleConfig(Int32 _config)
	{
		ObstacleConfig = (ObstaclesConfiguration)_config;
	}

private Vector2 m_range;
    private List<GameObject> m_obstacles = new List<GameObject>();
}
