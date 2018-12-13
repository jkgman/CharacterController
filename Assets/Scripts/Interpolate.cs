using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interpolate : MonoBehaviour {
    Vector3 v0, v1;
    Quaternion q0, q1;
    //ad scale
    float t0, t1;
    public bool interpolate = true;

	void Start () {
        t0 = t1 = Time.time;
        v0 = v1 = transform.position;
        q0 = q1 = transform.rotation;
	}
    private void FixedUpdate()
    {
        t0 = t1;
        v0 = v1;
        q0 = q1;
        t1 = Time.time;
        v1 = transform.position;
        q1 = transform.rotation;
    }
    // Update is called once per frame
    void Update () {
        if(interpolate)
        {
            transform.position = Vector3.Lerp(v0, v1, (Time.time - t1)/Time.fixedDeltaTime);
            transform.rotation = Quaternion.Slerp(q0, q1, (Time.time - t1) / Time.fixedDeltaTime);
        }
	}
}
