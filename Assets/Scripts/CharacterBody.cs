using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBody : MonoBehaviour {

    public float charSpeed;
    public Vector2 rotateSpeed;
    [Range(0, 90)]
    public float maxSlope;
    public float jumpVelocity;
    public float gravity;
    public float airFriction = 1;
    public float maxVelocity;

    public int maximumPhysicsLoops = 10;
    public LayerMask raycastLayerMask;

    public Camera cam;
    public CapsuleCollider capsule;
    public MoveHandler mover;

    private bool isGrounded = false;
    private Vector3 input;
    private Vector3 velocity;
    private float acceleration;
    private RaycastHit lasthit;
    private Collider[] m_colliders = new Collider[15];
    private Vector3 Momentum;


    private void FixedUpdate()
    {
        //Rotate
        transform.Rotate(Input.GetAxis("Mouse X") * Vector3.up * rotateSpeed.x);
        cam.transform.Rotate(Input.GetAxis("Mouse Y") * Vector3.left * rotateSpeed.y);

        //Normalize input
        Vector3 inXZ = new Vector3(input.x, 0, input.z) * Time.deltaTime * charSpeed;
        //Gravity
        acceleration += input.y;
        acceleration = (acceleration - gravity * Time.deltaTime);
        velocity.y = acceleration;
        if(!isGrounded)
        {
            //Add input without modification
            velocity += transform.forward * inXZ.x;
            velocity += Vector3.Cross(transform.forward, Vector3.up) * inXZ.z;
            //Calculate in air momentum
            Momentum = Momentum.normalized * (Momentum.magnitude - airFriction * Time.deltaTime);
            velocity += Momentum;
        } else
        {
            //Add input mapped to the surface of platform
            inXZ = transform.forward * inXZ.x + Vector3.Cross(transform.forward, Vector3.up) * inXZ.z;
            Vector3 perpPlaneDir = Vector3.Cross(lasthit.normal, inXZ);
            Vector3 planeDir = Vector3.Cross(perpPlaneDir, lasthit.normal);
            planeDir = planeDir * Mathf.Sign(Vector3.Dot(planeDir, inXZ));
            velocity += planeDir.normalized * inXZ.magnitude;
        }

        //Clamp Velocity
        velocity = velocity * Mathf.Clamp(velocity.magnitude, -maxVelocity, maxVelocity);

        //pushout if in object at start
        Pushout();

        isGrounded = false;

        //Collisions variables
        RaycastHit hit;
        int maxLoops = 0;
        Vector3 loopStartVec;
        //Top and bottom sphere position of player capsule
        Vector3 TopSphere = transform.position + new Vector3(0, capsule.height / 2 - capsule.radius, 0) + capsule.center;
        Vector3 BotSphere = transform.position - new Vector3(0, capsule.height / 2 - capsule.radius, 0) + capsule.center;
        /*Main Collision Detection*/
        while(Physics.CapsuleCast(
                    TopSphere, BotSphere, capsule.radius,
                    velocity.normalized,//Direction vector
                    out hit,
                    Mathf.Abs(velocity.magnitude),//Direction Length
                    raycastLayerMask))
        {
            maxLoops++;
            loopStartVec = velocity;
            Vector3 XZ = new Vector3(velocity.x, 0, velocity.z);
            if((-hit.normal.y + 1) * 90 < maxSlope)//hit normal is 1 for flat and 0 for wall so invert that then times it by 90 to get the proper degree then compare it
            {
                /*Traversable slope*/
                //currently will step over anything less than 1/4th the radius of capsule
                Vector3 perpPlaneDir = Vector3.Cross(hit.normal, XZ);//this is a vector that will be parrallel to the slope but it will be perpindicular to the direction we want to go
                Vector3 planeDir = Vector3.Cross(perpPlaneDir, hit.normal);//This will be the an axis line of were we are walking, but we dont know if its forwards or backwards right now
                planeDir = planeDir * Mathf.Sign(Vector3.Dot(planeDir, XZ));//dot returns pos if they are headed in the same direction. so multiplying the planedir by its sign will give us the correct direction on the vector
                velocity = velocity.normalized * (hit.distance);//this will set velocity to go the un obstructed amount of the cast
                XZ -= new Vector3(velocity.x, 0, velocity.z);//this makes xv the remainder xv distance
                velocity += planeDir.normalized * XZ.magnitude;
                velocity.y += Mathf.Sign(hit.normal.y) * .001f;//sets the final position slightly offset from the ground so we dont clip through next frame
                acceleration = 0;
                isGrounded = true;
                lasthit = hit;
            } else
            {
                /*Walls and Ceilings*/
                //get the vectors of the plane we hit
                Vector3 parallelSide = Vector3.Cross(Vector3.up, hit.normal);
                Vector3 parallelDown = Vector3.Cross(parallelSide, hit.normal);
                //Correct the direction of the xz vector on our plane
                parallelSide = parallelSide * Mathf.Sign(Vector3.Dot(parallelSide, XZ));
                float angle = Vector3.Angle(-new Vector3(velocity.x, 0, velocity.z).normalized, hit.normal);//get our angle between our direction and the colliders normal
                float VerticalRemainder = velocity.y;
                float XZPlaneRemainder = XZ.magnitude;
                velocity = velocity.normalized * (hit.distance);//set our vector to the hit
                VerticalRemainder -= velocity.y;//Get the acurate remainder of y from our origional y
                XZPlaneRemainder -= new Vector3(velocity.x, 0, velocity.z).magnitude;//Get our xz remainder from initial xz vector
                if(velocity.y <= 0)
                {
                    //Slopes slide
                    velocity += parallelDown.normalized * Mathf.Abs(VerticalRemainder);
                } else if((-hit.normal.y + 1) * 90 > 90)
                {
                    //ceilings stop our acceleration
                    acceleration = 0;
                } else
                {
                    velocity.y += VerticalRemainder;
                }
                velocity += parallelSide.normalized * (XZPlaneRemainder * (angle / 90));//get our xzremainder and add it reduced by the angle we hit the collider at
                velocity += hit.normal * .001f;//offset our velocity a tad so it doesnt jitter or clip through next frame
            }


            /*Loop Breaks*/
            //if we loop too much just exit and set velocity to just before collision and exit
            if(maxLoops == maximumPhysicsLoops)
            {
                Debug.Log("max loops");
                velocity = velocity.normalized * (hit.distance) + hit.normal * .001f;
                break;
            }
            //if last position velocity was the same as the new calculation set velocity to just before collision and exit
            if(loopStartVec == velocity)
            {
                Debug.Log("Same vector");
                velocity = velocity.normalized * (hit.distance) + hit.normal * .001f;
                break;
            }

        }

        /*Fall Back Fill Check*/
        if(Physics.CheckCapsule(TopSphere + velocity, BotSphere + velocity, capsule.radius, raycastLayerMask))
        {
            Debug.Log("End Capsule Fail");
            //Ditch Velocity
            acceleration = 0;
            velocity = Vector3.zero;
        }

        if(isGrounded)
        {
            Momentum = Vector3.zero;
            mover.SetCollisionInfo(lasthit);
            mover.IsMoveObject(lasthit.collider);
        } else
        {
            mover.Unhook();
        }

        //Apply velocity and clear containers
        transform.position += velocity;

        //clear input
        input = Vector3.zero;
        velocity = Vector3.zero;
    }


    private void Pushout()
    {
        /*Checks that the player doesnt start in something*/
        int numOverlaps = 1;
        numOverlaps = Physics.OverlapCapsuleNonAlloc(transform.position + new Vector3(0, capsule.height / 2 - capsule.radius, 0) + capsule.center, transform.position - new Vector3(0, capsule.height / 2 - capsule.radius, 0) + capsule.center, capsule.radius, m_colliders, raycastLayerMask, QueryTriggerInteraction.UseGlobal);
        for(int i = 0; i < numOverlaps; i++)
        {
            Debug.Log("Start Capsule Fail");
            Vector3 direction;
            float distance;
            if(Physics.ComputePenetration(capsule, transform.position, transform.rotation, m_colliders[i], m_colliders[i].transform.position, m_colliders[i].transform.rotation, out direction, out distance))
            {
                Vector3 penetrationVector = direction * distance;
                Vector3 velocityProjected = Vector3.Project(velocity, -direction);
                transform.position = transform.position + penetrationVector;
                velocity -= velocityProjected;
            }
        }
    }

    public void ForceMove(Vector3 moveVec)
    {
        transform.position += moveVec;
        Momentum = moveVec;
    }

    public void Jump()
    {
        if(isGrounded)
        {
            input.y += jumpVelocity;
            isGrounded = false;
        }
    }

    public void XZInput(Vector2 xz)
    {
        input.x += xz.x;
        input.z += xz.y;
    }
}
