using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(CapsuleCollider))]
public class CharacterPhysicsController : MonoBehaviour {
    [Range(1, 179)]
    public float slopeLimit = 45;
    public float stepOffset = .3f;
    public float skinWidth = .08f;
    public float minMoveDistance = .001f;
    public int maximumPhysicsLoops = 10;
    public LayerMask raycastLayerMask;
   
    private CapsuleCollider capsule;
    [HideInInspector]
    public bool isGrounded = false;
    private RaycastHit lasthit;
    private Collider[] m_colliders = new Collider[15];

    private Vector3 nextPosition;
    private Vector3 nextScale;
    private Quaternion nextRotation;
    public Vector3 currentMomentum;

    Vector3 standingOnNormal;
    private void Start()
    {
        capsule = GetComponent<CapsuleCollider>();
        if(capsule == null)
        {
            Debug.LogWarning("Failed to find capsul on characterPhysicsController object");
        }
    }

    private void FixedUpdate()
    {
        
        PhysicsMove();
    }

    private void PhysicsMove() {

        bool hitTraversable = false;
        currentMomentum = nextPosition;//set momentum to current projeccted movement vec
        //Rotate and Scale
        transform.rotation *= nextRotation;
        transform.localScale += nextScale;


        PushOut();

        if(standingOnNormal != Vector3.zero && isGrounded)
        {
            Vector3 perpPlaneDir = Vector3.Cross(standingOnNormal, new Vector3(nextPosition.x, 0, nextPosition.z));
            Vector3 planeDir = Vector3.Cross(perpPlaneDir, standingOnNormal);
            nextPosition = planeDir.normalized * nextPosition.magnitude;
            nextPosition += Vector3.down * skinWidth;
        }

        /*Variables needed for capsule cast*/
        RaycastHit hit;
        int currentLoop = 0;
        
        //Positions of origin for the top and bottom sphere of players capsule
        Vector3 TopSphere = transform.position + new Vector3(0, capsule.height / 2 - capsule.radius, 0) + capsule.center;
        Vector3 BotSphere = transform.position - new Vector3(0, capsule.height / 2 - capsule.radius, 0) + capsule.center;
        //Sphere cast
        while(Physics.CapsuleCast(TopSphere, BotSphere, capsule.radius, nextPosition.normalized, out hit, Mathf.Abs(nextPosition.magnitude), raycastLayerMask))
        {
            currentLoop++;
            Vector3 XZ = new Vector3(nextPosition.x, 0, nextPosition.z);


            //If hit is in our skinWidth we set distance to zero
            //this makes it so the entire magnitude will get mapped to the redirection instead of only the remainder
            if(hit.distance <= skinWidth)
            {
                hit.distance = 0;
            } else
            {
                hit.distance -=  skinWidth / 2;
            }
            

            /*Traversable Slope*/
            //turn hit normal y to degree between 0 floor to 180 ceiling and check its under slopelimit
            if((-hit.normal.y + 1) * 90 < slopeLimit)
            {
                hitTraversable = true;
                standingOnNormal = hit.normal;
                Vector3 perpPlaneDir = Vector3.Cross(hit.normal, XZ);       //this is a vector that will be parrallel to the slope but it will be perpindicular to the direction we want to go
                Vector3 planeDir = Vector3.Cross(perpPlaneDir, hit.normal); //This will be the an axis line of were we are walking, but we dont know if its forwards or backwards right now
                //planeDir = planeDir * Mathf.Sign(Vector3.Dot(planeDir, XZ));//dot returns pos if they are headed in the same direction. so multiplying the planedir by its sign will give us the correct direction on the vector
                nextPosition = nextPosition.normalized * (hit.distance);    //this will set nextPosition to go the un obstructed amount of the cast
                XZ -= new Vector3(nextPosition.x, 0, nextPosition.z);       //this makes xv the remainder xv distance
                nextPosition += planeDir.normalized * XZ.magnitude;
                currentMomentum = new Vector3(currentMomentum.x, 0, currentMomentum.z);
            }


            /*Non Traversible Slope*/
            else
            {
                
                //get the vectors of the plane we hit
                Vector3 parallelSide = Vector3.Cross(Vector3.up, hit.normal);
                Vector3 parallelDown = Vector3.Cross(parallelSide, hit.normal);
                //Correct the direction of the xz vector on our plane
                parallelSide = parallelSide * Mathf.Sign(Vector3.Dot(parallelSide, XZ));
                float angle = Vector3.Angle(-new Vector3(nextPosition.x, 0, nextPosition.z).normalized, hit.normal);//get our angle between our direction and the colliders normal
                float VerticalRemainder = nextPosition.y;
                float XZPlaneRemainder = XZ.magnitude;
                nextPosition = nextPosition.normalized * (hit.distance);//set our vector to the hit
                VerticalRemainder -= nextPosition.y;//Get the acurate remainder of y from our origional y
                XZPlaneRemainder -= new Vector3(nextPosition.x, 0, nextPosition.z).magnitude;//Get our xz remainder from initial xz vector
                if((-hit.normal.y + 1) * 90 < 90 && VerticalRemainder < 0)
                {
                    nextPosition -= parallelDown.normalized * VerticalRemainder;
                } else
                {
                    nextPosition -= parallelDown.normalized * VerticalRemainder;
                }
                nextPosition += parallelSide.normalized * (XZPlaneRemainder * (angle / 90));//get our xzremainder and add it reduced by the angle we hit the collider at
                currentMomentum = nextPosition;
            }


            if(currentLoop == maximumPhysicsLoops)
            {
                Debug.Log("max loops");
                nextPosition = nextPosition.normalized * (hit.distance) + hit.normal * .001f;
                break;
            }
        }

        if(Physics.CheckCapsule(TopSphere + nextPosition, BotSphere + nextPosition, capsule.radius, raycastLayerMask))
        {
            Debug.Log("End Capsule Fail");
            currentMomentum = Vector3.zero;
            nextPosition = Vector3.zero;
        }

        if(!hitTraversable)
        {
            standingOnNormal = Vector3.zero;
            isGrounded = false;
        } else
        {
            currentMomentum = Vector3.zero;
            isGrounded = true;
        }


        transform.position += nextPosition;

        nextPosition = Vector3.zero;
        nextScale = Vector3.zero;
        nextRotation = Quaternion.identity;
    }
 

    public void Move(Vector3 moveVec) {
        nextPosition += moveVec;
        if(moveVec.y > 0)
        {
            isGrounded = false;
        }
    }
    public void Scale(Vector3 scaleVec) {
        nextScale += nextScale;
    }
    public void Rotate(Quaternion rotateQuat) {
        nextRotation *= rotateQuat;
    }
    public void Rotate(Vector3 rotateEul)
    {
        nextRotation *= Quaternion.Euler(rotateEul);
    }

    private void PushOut() {
        /*Checks that the player doesnt start in something*/
        int numOverlaps = 1;
        numOverlaps = Physics.OverlapCapsuleNonAlloc(
                transform.position + new Vector3(0, capsule.height / 2 - capsule.radius, 0) + capsule.center,
                transform.position - new Vector3(0, capsule.height / 2 - capsule.radius, 0) + capsule.center,
                capsule.radius, m_colliders, raycastLayerMask,
                QueryTriggerInteraction.UseGlobal);
        for(int i = 0; i < numOverlaps; i++)
        {
            Debug.Log("Start Capsule Fail");
            Vector3 direction;
            float distance;
            if(Physics.ComputePenetration(capsule, transform.position, transform.rotation, m_colliders[i], m_colliders[i].transform.position, m_colliders[i].transform.rotation, out direction, out distance))
            {
                Vector3 penetrationVector = direction * distance;
                transform.position = transform.position + penetrationVector;
            }
        }
    }
}
