using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotater : MovingObject  {
    [Range(0,360)]
    public float angle;//, yDist, zDist;
    public float speed = 1;
    public Vector3 axis;
    private Quaternion start;
    // Use this for initialization
    public override void Init () {
        start = transform.rotation;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        Quaternion quat = Quaternion.AngleAxis(angle * Mathf.Sin(Time.time * speed), axis.normalized);

        transform.rotation = quat;
        Move();
    }
}
