using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
public class ZombieController : MonoBehaviour {
    
    Animator anim;
    CharacterController ctrl;
    public enum Direction {
        left, right, back
    }
    
    public float moveSpeed = .8f;
    public Transform player;
    public float playerDetectRange = 10;
    
    float attackLength = 1.05f;
    public bool walking = false;
    public bool dead = false;
    
    public float verticalVelocity = 0;
    public float killVelocity = 3;
    
    public static int zombieCount = 0;
    
    public bool playerInRange = false;
    bool attacking = false;
    
    void Start () {
        anim = GetComponent<Animator>();
        ctrl = GetComponent<CharacterController>();
        zombieCount++;
    }
    
    void Update () {
        if (dead)
            return;
        
        if ((player.position - transform.position).magnitude < playerDetectRange)
        {
            if (!walking)
            {
                // print("Joueur détecté");
                walking = true;
                anim.SetBool("running", true);
            }
            
            Vector3 direction = player.position - transform.position;
            direction = direction.normalized;
            CollisionFlags collisionFlags = ctrl.Move(direction * moveSpeed - Vector3.up * verticalVelocity);
            verticalVelocity += 9.81f * Time.deltaTime * Time.deltaTime;
            if (collisionFlags == CollisionFlags.Below)
            {
                if (verticalVelocity > killVelocity)
                    kill(Direction.back);
                verticalVelocity = 0;
            }
            
            Vector3 plyrDirection = player.position - transform.position;
            plyrDirection.y = transform.position.y;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(plyrDirection, Vector3.up), Time.deltaTime);
        }
        else
        {
            if (walking)
            {
                // print("Joueur perdu");
                walking = false;
                anim.SetBool("running", false);
            }
            CollisionFlags collisionFlags = ctrl.Move(-Vector3.up * verticalVelocity);
            verticalVelocity += 9.81f * Time.deltaTime * Time.deltaTime;
            if (collisionFlags == CollisionFlags.Below)
            {
                if (verticalVelocity > killVelocity)
                    kill(Direction.back);
                verticalVelocity = 0;
            }
        }
        
        if (attacking && playerInRange)
        {
            anim.SetTrigger("attack");
            StartCoroutine(reenableAttackCo());
        }
        
        playerDetectRange += Time.deltaTime*8;
    }
    
    public void kill(Direction dir)
    {
        switch (dir)
        {
            case Direction.left:
                anim.SetTrigger("fall_left");
                break;
            case Direction.right:
                anim.SetTrigger("fall_right");
                break;
            case Direction.back:
                anim.SetTrigger("fall_back");
                break;
            default:
                anim.SetTrigger("fall_back");
                break;
        }
        dead = true;
        ctrl.enabled = false;
        StartCoroutine(depop());
    }
    
    IEnumerator depop()
    {
        yield return new WaitForSeconds(10f);
        Destroy(gameObject);
        zombieCount--;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == gameObject)
            return;
        if (other.gameObject == player.gameObject)
            attacking = playerInRange = true;
    }
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == gameObject)
            return;
        if (other.gameObject == player.gameObject)
            playerInRange = false;
    }
    
    IEnumerator reenableAttackCo()
    {
        attacking = false;
        yield return new WaitForSeconds(attackLength);
        attacking = true;
    }
}
