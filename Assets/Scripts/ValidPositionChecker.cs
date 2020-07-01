using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValidPositionChecker : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public bool IsValidPosition()
    {
	    return !isColliding;
    }

    void OnTriggerEnter2D(Collider2D collisionInfo)
    {
	    isColliding = true;
    }

    void OnTriggerExit2D(Collider2D collisionInfo)
    {
	    isColliding = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private bool isColliding = false;
}
