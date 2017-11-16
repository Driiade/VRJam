using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieKiller : MonoBehaviour {
    public bool isLeft = false;
    
    void Start()
    {}
    
    void OnTriggerEnter(Collider col)
    {
        if (!enabled)
            return;
        ZombieController ctrl = col.GetComponent<ZombieController>();
        // ZombiePart part = col.GetComponent<ZombiePart>();
        if (ctrl != null)
            ctrl.kill(isLeft ? ZombieController.Direction.left : ZombieController.Direction.right);
        // if (part != null)
            // part.damage();
    }
}
