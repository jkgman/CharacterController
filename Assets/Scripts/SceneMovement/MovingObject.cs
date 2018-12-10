using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObject : MonoBehaviour {

    private Collider col;
    public bool isHooked = false;
    private MoveHandler mover;

    private void Start()
    {
        mover = FindObjectOfType<MoveHandler>();
        col = GetComponent<Collider>();
        Init();
    }

    public void Hook() {
        isHooked = true;
    }
    public void Unhook()
    {
        isHooked = false;
    }

    public void Move() {
        
        if(isHooked)
        {
            mover.Move(col);
        }
    }
    public virtual void Init() {}
}
