using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(CharacterPhysicsController))]
public class CharacterMotor : MonoBehaviour {
    CharacterPhysicsController charPhysics;
    public float movementSpeed;
    public float rotSpeed;
    public float gravity = 9.8f;
    public float jumpForce = 1;

	void Start () {
        charPhysics = GetComponent<CharacterPhysicsController>();
        if(charPhysics == null)
        {
            Debug.LogWarning("Couldnt find CharacterPhysicsController on character");
        }
    }


    private void FixedUpdate()
    {
        if(!charPhysics.isGrounded)
        {
            float acceleration = charPhysics.currentMomentum.y - gravity * Time.fixedDeltaTime;
            charPhysics.Move(new Vector3(0,acceleration,0));
        }
    }
    

    void Update () {
        Vector3 moveVec = Vector3.zero;
        Vector3 eulsRot = Vector3.zero;
        if(Input.GetKey(KeyCode.W))
        {
            moveVec += transform.forward;
        }
        if(Input.GetKey(KeyCode.S))
        {
            moveVec -= transform.forward;
        }
        if(Input.GetKey(KeyCode.A))
        {
            moveVec -= transform.right;
        }
        if(Input.GetKey(KeyCode.D))
        {
            moveVec += transform.right;
        }
        if(charPhysics.isGrounded && Input.GetKey(KeyCode.Space))
        {
            charPhysics.Move(new Vector3(0,jumpForce,0));
        }
        eulsRot += new Vector3(0,Input.GetAxis("Mouse X") * Time.deltaTime * rotSpeed, 0);
        charPhysics.Move(moveVec.normalized * Time.deltaTime * movementSpeed);
        charPhysics.Rotate(eulsRot);
    }
}
