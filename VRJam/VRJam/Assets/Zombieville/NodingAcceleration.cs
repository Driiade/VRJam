using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class NodingAcceleration : MonoBehaviour {
    
    public float maxMovementVelocity = .3f;
    public Transform cameraTransform;
    public bool useMouseMovement = true;
    public Transform katanaOrientation;
    public Transform katana;
    public float katanaWhenRotationVelocAbove = 20;
    public Animator katanaAnim;
    
    [Range(0, .6f)]
    public float minAccelerationToMove = .3f;
    [Range(0, 3)]
    public float velocityIncreaseOnNod = 1;
    [Range(0, 10)]
    public float velocityDecrease = 4;
    
    CharacterController ctrl;
    
    public float curVelocity;
    public float curVerticalVelocity;
    
    public float hpGainPerSecond = 5;
    
    public BloodRainCameraController bloodCtrl;
    
    public AudioSource[] katanaSwingSounds;

    public AudioSource deathScream;
    public AudioSource breath;

    float mouseMvtRotX = 0;
    Camera cam;
    float mouseMvtRotY = 0;
    bool lockInput;
    float lastX = 0;
    float lastY = 0;
    Vector3 spawnPoint;
    bool dead = false;
    
    void Start () {
        ctrl = GetComponent<CharacterController>();
        spawnPoint = ctrl.transform.position;
        cam = cameraTransform.GetComponent<Camera>();
        SetCursorLock(useMouseMovement);
        StartCoroutine(regainHp());
    }
    
    void SetCursorLock(bool lockCursor)
    {
        if (lockCursor && !lockInput)
        {
            lockInput = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (!lockCursor && lockInput)
        {
            lockInput = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    
    void Update () {
        if (useMouseMovement && lockInput)
            mouseHack();
        
        if (dead)
            return;
        
        Vector3 direction = cameraTransform.forward;
        direction.y = 0;
        direction = direction.normalized;
        
        curVerticalVelocity += 9.81f * Time.deltaTime * Time.deltaTime;
        
        curVelocity += Mathf.Abs(lastX - cameraTransform.eulerAngles.x) * Time.deltaTime * velocityIncreaseOnNod;
        curVelocity -= Time.deltaTime*velocityDecrease;
        curVelocity = Mathf.Clamp(curVelocity, 0, 1);
        
        CollisionFlags collisionFlags = ctrl.Move(direction * Mathf.Max(curVelocity-minAccelerationToMove, 0) * (1-minAccelerationToMove) * maxMovementVelocity - Vector3.up * curVerticalVelocity);
        if (collisionFlags == CollisionFlags.Below)
        {
            curVerticalVelocity = 0;
        }
        
        
        if (Input.GetKeyUp(KeyCode.Escape))
            SetCursorLock(false);
        else if(Input.GetMouseButtonUp(0))
            SetCursorLock(true);
        
        lastX = cameraTransform.eulerAngles.x;
        
        float oldRot = katanaOrientation.eulerAngles.y;
        katanaOrientation.rotation = Quaternion.Lerp(katanaOrientation.rotation, Quaternion.LookRotation(direction, Vector3.up), Time.deltaTime*10);
        
        if (Mathf.Abs(cameraTransform.eulerAngles.y - lastY) * Time.deltaTime > katanaWhenRotationVelocAbove)
            cutWithKatana();
        
        lastY = cameraTransform.eulerAngles.y;
    }
    
    void cutWithKatana()
    {
        katanaAnim.SetTrigger("cut");
        katanaSwingSounds[(int)(Random.value * katanaSwingSounds.Length)].Play();
    }
    
    public void kill()
    {
        if (dead)
            return;
        print("KILL");
        dead = true;
        // cam.enabled = false;

        deathScream.Play();
        breath.Stop();

        StartCoroutine(respawn());
    }
    
    public void damage(int hp)
    {
        if (dead)
            return;
        bloodCtrl.HP -= hp;
        if (bloodCtrl.HP <= 0)
            kill();
    }
    
    IEnumerator regainHp()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            bloodCtrl.HP = (int)Mathf.Min(100, bloodCtrl.HP+hpGainPerSecond);
        }
    }
    
    IEnumerator respawn()
    {
        yield return new WaitForSeconds(5);
        ctrl.transform.position = spawnPoint;
        curVerticalVelocity = curVelocity = 0;
        dead = false;
        bloodCtrl.HP = 100;
        breath.Play();
        // cam.enabled = true;
    }
    
    void mouseHack()
    {
        mouseMvtRotX -= Input.GetAxis("Mouse Y") * .03f;
        mouseMvtRotY += Input.GetAxis("Mouse X") * .03f;
        cameraTransform.rotation = Quaternion.EulerAngles(0, mouseMvtRotY, 0) * Quaternion.EulerAngles(mouseMvtRotX, 0, 0);
    }
}
