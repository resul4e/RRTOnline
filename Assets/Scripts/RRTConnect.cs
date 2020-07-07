using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public enum ExtendStatus
{
	Reached,
	Trapped,
	Advanced
}

public class Tree : IDisposable
{
	public GameObject NodePrefab;
	public GameObject PathPrefab;
	public Node Root;

	public Tree(Vector3 _position, GameObject _nodePrefab, GameObject _pathPrefab)
	{
		NodePrefab = _nodePrefab;
		PathPrefab = _pathPrefab;

		Root = new Node(_position, NodePrefab, _pathPrefab);
		m_nodes.Add(Root);
		
	}

	public void Dispose()
	{
		Root.Dispose();
	}

	public Node GetLatestNode()
	{
		return m_new;
	}

	public void AddNode(Vector3 _position)
	{
		var node = new Node(_position, NodePrefab, PathPrefab);
		AddNode(node);
	}

	public void AddNode(Node _node)
	{
		var nearestNode = NearestNeighbour(_node.Position);
		nearestNode.AddChild(_node);
		_node.Parent = nearestNode;
		m_nodes.Add(_node);
		m_new = _node;
	}

	public Node NearestNeighbour(Vector3 _nodePos)
	{
		var dist = float.MaxValue;
		int index = -1;
		for (int i = 0; i < m_nodes.Count; i++)
		{
			var otherNode = m_nodes[i];
			var newDist = Vector3.Distance(_nodePos, otherNode.Position);
			if (newDist < dist)
			{
				dist = newDist;
				index = i;
			}
		}

		return m_nodes[index];
	}

	private Node m_new;
	private List<Node> m_nodes = new List<Node>();
}

public class Node : IDisposable
{
	public static GameObject ConnectionPrefab;

	private GameObject m_goPrefab;
	private GameObject m_pathPrefab;

	public Vector3 Position;
	public Node Parent;
	public List<Node> Children = new List<Node>();

	public Node(Vector3 _pos, GameObject _nodeprefab, GameObject _pathPrefab)
	{
		m_goPrefab = _nodeprefab;
		m_pathPrefab = _pathPrefab;
		Position = _pos;
		m_nodeGO = GameObject.Instantiate(m_goPrefab, Position, Quaternion.identity);
	}

	public void AddChild(Node _child)
	{
		Children.Add(_child);
		var diff = _child.Position - Position;
		var center = diff / 2.0f;

		var angle = Vector3.Dot(diff.normalized, Vector3.right);

		m_connectionGO = GameObject.Instantiate(ConnectionPrefab, Position + center, Quaternion.Euler(0,0,Mathf.Rad2Deg * Mathf.Acos(angle)));
	}

	public void Draw()
	{
		foreach (var child in Children)
		{
			Gizmos.DrawLine(Position, child.Position);
			child.Draw();
		}

		Gizmos.DrawSphere(Position, 0.3f);
	}

	public void SetAsPath()
	{
		Object.Destroy(m_nodeGO);
		m_nodeGO = GameObject.Instantiate(m_pathPrefab, Position, Quaternion.identity);
	}

	public void Dispose()
	{
		Object.Destroy(m_nodeGO);
		foreach (var child in Children)
		{
			child.Dispose();
		}
	}

	private GameObject m_nodeGO;
	private GameObject m_connectionGO;
}

public class RRTConnect : RRTBase
{
	/// <summary>
	/// The rectangle that defines the area where new random points can be spawned
	/// </summary>
	public Vector2 Range;
	/// <summary>
	/// The maximum distance between the new point and the closest neighbour.
	/// This dampens the outward growth of the tree.
	/// </summary>
	public float MaxDist = 10f;
	/// <summary>
	/// How many steps we simulate per step. A step is executed every time the space key is pressed.
	/// </summary>
	public int IterationsPerStep = 10;
	/// <summary>
	/// The prefab used to spawn obstacles.
	/// </summary>
	public GameObject ObstaclePrefab;

	public GameObject Strt;
	/// <summary>
	/// The <see cref="GameObject"/> that represents the goal which the start node is working towards.
	/// </summary>
	public GameObject Goal;

	public GameObject StartPrefab;
	public GameObject GoalPrefab;
	public GameObject PathPrefab;
	public GameObject ConnectionPrefab;

	public ObstacleSpawner ObstacleSpawner;

	public Button ExecuteButton;
	public Slider IterationSlider;

	void Start()
	{
		IterationSlider.value = IterationsPerStep;
	}

	public override void Restart()
	{
		Node.ConnectionPrefab = ConnectionPrefab;
		m_start?.Dispose();
		m_end?.Dispose();

		m_start = new Tree(Strt.transform.position, StartPrefab, PathPrefab);
		m_end = new Tree(Goal.transform.position, GoalPrefab, PathPrefab);
		m_done = false;
	}

	/// <summary>
	/// Every time space is pressed we add an <see cref="IterationsPerStep"/> amount of points to the graph.
	/// </summary>
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.R))
		{
			Restart();
		}
	}

	/// <summary>
	/// Iterate for the amount of steps chosen by <see cref="IterationsPerStep"/>
	/// </summary>
	public void Iterate()
	{
		if (m_done)
		{
			return;
		}

		for (int k = 0; k < IterationsPerStep; k++)
		{
			var qRand = RandomState();

			if (Extend(m_start, qRand) != ExtendStatus.Trapped)
			{
				if (Connect(m_start.GetLatestNode().Position) == ExtendStatus.Reached)
				{
					m_done = true;
					ExecuteButton.interactable = false;
					ShowPath();
					break;
				}
			}

			var temp = m_start;
			m_start = m_end;
			m_end = temp;
		}
	}

	void ShowPath()
	{
		Node parent = m_start.GetLatestNode();
		while (parent != null)
		{
			parent.SetAsPath();
			parent = parent.Parent;
		}

		parent = m_end.GetLatestNode();
		while (parent != null)
		{
			parent.SetAsPath();
			parent = parent.Parent;
		}

	}

	public void ChangeIterationsPerStep(Single _newSteps)
	{
		IterationsPerStep = (int)_newSteps;
	}

	ExtendStatus Connect(Vector3 _q)
	{
		int iter = 0;
		ExtendStatus status = Extend(m_end, _q);
		while (m_maxIter != iter && status == ExtendStatus.Advanced)
		{
			status = Extend(m_end, _q);
			iter++;
		}

		return status;
	}

	public ExtendStatus Extend(Tree _t, Vector3 _q)
	{
		var nearNode = _t.NearestNeighbour(_q);
		Vector3 qNew;
		if (NewConfig(_q, nearNode, out qNew))
		{
			_t.AddNode(qNew);

			if (qNew == _q)
			{
				return ExtendStatus.Reached;
			}
			return ExtendStatus.Advanced;
		}
		return ExtendStatus.Trapped;
	}

	bool NewConfig(Vector3 _q, Node _qNear, out Vector3 _qNew)
	{
		var dist = Vector3.Distance(_q, _qNear.Position);
		if (dist > MaxDist)
		{
			var diff = _q - _qNear.Position;
			var norm = diff.normalized;
			_qNew = _qNear.Position + norm * MaxDist;
		}
		else
		{
			_qNew = _q;
		}

		var oldQNew = _qNew;
		foreach (var obs in ObstacleSpawner.Obstacles)
		{
			//If it does xNew will be this intersection.
			_qNew = BoxLineIntersect(obs, _qNear.Position, _qNew);
		}

		return _qNew == oldQNew;
	}

	/// <summary>
	/// Tests if a line intersects a box.
	/// </summary>
	/// <param name="_box">The box to test.</param>
	/// <param name="_start">The start position of the line</param>
	/// <param name="_end">The end position of the line</param>
	/// <returns><paramref name="_end"/> if nothing is hit. Otherwise the point just before intersection.</returns>
	Vector3 BoxLineIntersect(GameObject _box, Vector3 _start, Vector3 _end)
	{
		var maxdist = Vector3.Distance(_start, _end);
		var dir = (_end - _start).normalized;
		float dist;
		if (_box.GetComponent<Collider2D>().bounds.IntersectRay(new Ray(_start, dir), out dist))
		{
			if (dist < maxdist)
			{
				return _start + (dist - 1) * dir;
			}
		}

		return _end;
	}

	/// <summary>
	/// Get a new random point within <see cref="Range"/>
	/// </summary>
	/// <returns>A new random point.</returns>
	Vector3 RandomState()
	{
		return new Vector3(Random.Range(-Range.x, Range.x), Random.Range(-Range.y, Range.y), 0);
	}

	private bool m_done;
	private Tree m_start;
	private Tree m_end;

	private int m_maxIter = 100;
}
