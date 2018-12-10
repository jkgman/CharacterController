using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mover : MovingObject {
    public float xDist,yDist,zDist;//, yDist, zDist;
    private Vector3 start;
    // Use this for initialization
    public override void Init()
    {
        start = transform.position;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        Vector3 newPos = start + new Vector3(xDist * Mathf.Sin(Time.time), yDist * Mathf.Sin(Time.time), zDist * Mathf.Sin(Time.time));
        Vector3 Dif = newPos - transform.position;
        transform.position = newPos;
        Move();
    }
}
