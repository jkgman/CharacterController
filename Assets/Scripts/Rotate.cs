using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour {
    Vector3 Startpos;
	// Use this for initialization
	void Start () {
        Startpos = transform.position;
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        transform.position = Startpos + new Vector3(Mathf.Sin(Time.time), Mathf.Cos(Time.time), 0);
    }
}
