using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerKiller : MonoBehaviour {
    
    void Start () {
        
    }
    
    void OnTriggerEnter(Collider col)
    {
        if (!enabled)
            return;
        NodingAcceleration ctrl = col.GetComponent<NodingAcceleration>();
        if (ctrl != null)
            ctrl.kill();
    }
}
