using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieSpawner : MonoBehaviour {
    
    public int numZombies = 10;
    public ZombieController zombiePrefab;
    public GameObject player;
    public float maxRange = 10;
    
    void Update()
    {
        if (ZombieController.zombieCount < numZombies)
        {
            ZombieController zombie = Instantiate(zombiePrefab, transform.position, Quaternion.identity);
            Vector2 range = Random.insideUnitCircle * maxRange;
            zombie.transform.position += new Vector3(range.x, 0, range.y);
            zombie.player = player.transform;
        }
    }
}
