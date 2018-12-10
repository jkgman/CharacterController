using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPhysicsController : MonoBehaviour {
    [Range(1, 179)]
    public float slopeLimit = 45;
    public float stepOffset = .3f;
    public float minMoveDistance = .001f;


    public int maximumPhysicsLoops = 10;
    public LayerMask raycastLayerMask;
    private CapsuleCollider capsule;
    private bool isGrounded = false;


    private RaycastHit lasthit;
    private Collider[] m_colliders = new Collider[15];

    private void FixedUpdate()
    {
        PhysicsMove();
    }

    private void PhysicsMove() {
        //try to scale and rotate, then push out, then try to move nextmovevec
        //if collid, slide, stop
    }

    public void Move(Vector3 moveVec) {
        //add movevec to next calc
    }
    public void Scale(Vector3 scaleVec) {
        //add scaleVec to next calc scale
    }
    public void Rotate(Quaternion rotateQuat) {
        //add rotateQuat to next calc rot quat
    }
    public void Rotate(Vector3 rotateEul)
    {
        //add rotateEul to next calc rot quat
    }

    private void PushOut() {
        //if inside a collider try to push out
    }
}
