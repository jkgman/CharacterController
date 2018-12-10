using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveHandler : MonoBehaviour {

    private bool grounded;
    private Vector3 normal;
    private Vector3 pos;
    private Quaternion quat;
    public MovingObject[] sceneMovers;
    public CharacterBody player;
    private int currentHook;
    private void Start()
    {
       // sceneMovers = GameObject.FindObjectsOfType<MovingObject>();
    }
    public void SetCollisionInfo(RaycastHit hit) {
        pos = hit.collider.transform.position;
        quat = hit.collider.transform.rotation;
        normal = hit.normal;
    }
    public void IsMoveObject(Collider col) {
        if(sceneMovers[currentHook].isHooked && sceneMovers[currentHook].transform.position == col.transform.position)
        {

        } else
        {
            sceneMovers[currentHook].Unhook();
            for(int i = 0; i < sceneMovers.Length; i++)
            {
                if(sceneMovers[i].transform.position == col.transform.position)
                {
                    sceneMovers[i].Hook();
                    currentHook = i;
                }
            }
        }
        
    }

    public void Move(Collider col) {
        
        //Translate
        Vector3 moveVector = col.transform.position - pos;
        //Rotate
        Quaternion relative = Quaternion.Inverse(quat) * col.transform.rotation;
        Vector3 groundingpoint = player.transform.position - new Vector3(0, player.capsule.height / 2 - player.capsule.radius, 0) - normal.normalized * player.capsule.radius - pos;
        Vector3 newNormal = relative * normal;
        Vector3 trans = (relative * (groundingpoint)) + newNormal.normalized * player.capsule.radius + new Vector3(0, player.capsule.height / 2 - player.capsule.radius, 0) + pos;
        moveVector += trans - player.transform.position;
        player.ForceMove(moveVector);
    }

    public void Unhook() {
        sceneMovers[currentHook].Unhook();
    }
}
