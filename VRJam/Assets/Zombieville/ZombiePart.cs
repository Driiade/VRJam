using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombiePart : MonoBehaviour {
    
    bool detached = false;
    
    void Start () {
        
    }
    
    // void Update () {
        
    // }
    
    public void damage()
    {
        if (detached)
            return;
        print("DAMAGE");
        transform.parent = null;
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;
        GetComponent<Collider>().isTrigger = false;
        detached = true;
    }
}
