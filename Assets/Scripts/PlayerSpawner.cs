using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public ObstacleSpawner ObstacleSpawner;

    public GameObject Strt;
    public GameObject Goal;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void Respawn()
    {
	    float size = Camera.main.orthographicSize;
	    float aspect = Camera.main.aspect;

	    m_range = new Vector2((size * aspect) - Strt.transform.localScale.x, size - Goal.transform.localScale.y);

		switch (ObstacleSpawner.ObstacleConfig)
	    {
		    case ObstacleSpawner.ObstaclesConfiguration.Random:
		    case ObstacleSpawner.ObstaclesConfiguration.Narrow:
			    SpawnRandomly();
			    break;
		    case ObstacleSpawner.ObstaclesConfiguration.U:
			    SpawnInU();
			    break;
	    }
	}

    void SpawnRandomly()
    {
	    //position the start and end somewhere valid
	    bool colliding = false;
	    do
	    {
		    Strt.transform.position = new Vector3(Random.Range(-m_range.x, m_range.x), Random.Range(-m_range.y, m_range.y));
		    foreach (var obs in ObstacleSpawner.Obstacles)
		    {
			    colliding = Box2BoxIntersect(obs.GetComponent<BoxCollider2D>(), Strt.GetComponent<BoxCollider2D>());
			    if (colliding)
			    {
				    break;
			    }
		    }
	    } while (colliding);
	    do
	    {
		    Goal.transform.position = new Vector3(Random.Range(-m_range.x, m_range.x), Random.Range(-m_range.y, m_range.y));
		    foreach (var obs in ObstacleSpawner.Obstacles)
		    {
			    colliding = Box2BoxIntersect(obs.GetComponent<BoxCollider2D>(), Goal.GetComponent<BoxCollider2D>());
			    if (colliding)
			    {
				    break;
			    }
		    }
	    } while (colliding);
	}

    void SpawnInU()
    {
		Strt.transform.position = new Vector3(0,0);
		Goal.transform.position = new Vector3(m_range.x / 2.0f, 0);
	}

    bool Box2BoxIntersect(BoxCollider2D _first, BoxCollider2D _second)
    {
	    float fLeft = _first.transform.position.x + _first.offset.x - (_first.transform.localScale.x / 2.0f);
	    float fRight = _first.transform.position.x + _first.offset.x + (_first.transform.localScale.x / 2.0f);
	    float fTop = _first.transform.position.y + _first.offset.y - (_first.transform.localScale.y / 2.0f);
	    float fBottom = _first.transform.position.y + _first.offset.y + (_first.transform.localScale.y / 2.0f);

	    float sLeft = _second.transform.position.x + _second.offset.x - (_second.transform.localScale.x / 2.0f);
	    float sRight = _second.transform.position.x + _second.offset.x + (_second.transform.localScale.x / 2.0f);
	    float sTop = _second.transform.position.y + _second.offset.y - (_second.transform.localScale.y / 2.0f);
	    float sBottom = _second.transform.position.y + _second.offset.y + (_second.transform.localScale.y / 2.0f);

	    return fLeft < sRight && fRight > sLeft && fTop < sBottom && fBottom > sTop;
    }

    private Vector2 m_range;
}
