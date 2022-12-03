using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SleighController : MonoBehaviour
{
    private Camera cam;
    private BoxCollider sleigh_collider;
    private Rigidbody sleigh_body;

    public float jumpImpulse = 5;

    public List<ReindeerController> attachedReindeer = new List<ReindeerController>();
    
    ///<summary>the input, in world space</summary>
    private Vector3 inputDirection = new Vector3();

    /// <summary>
    /// How fast the player is currently moving vertically (y-axis), in meters/second.
    /// </summary>
    private float verticalVelocity = 0;

    void Start() {
        cam = Camera.main;
        sleigh_body = GetComponent<Rigidbody>();
        sleigh_collider = GetComponentInChildren<BoxCollider>();
    }
    void Update(){
        if(Input.GetButtonDown("Teleport")){
            PlayerController pc = FindObjectOfType<PlayerController>();
            if(pc.mode != SantaMode.SleighDriver){
                pc.Mount(this);
            }
        }
    }
    public void UpdateFromDriver(Vector3 inputDir) {

        inputDirection = inputDir;

        foreach(ReindeerController deer in attachedReindeer){
            deer.UpdateFromDriver(inputDirection);
        }

        float amount = Vector3.Dot(transform.up, Vector3.up);
        amount = AnimMath.Map(amount, -1, 1, 500, 0);
        Vector3 axis = Vector3.Cross(transform.up, Vector3.up);
        sleigh_body.AddTorque(axis * amount * Time.fixedDeltaTime);
    }
    public void LaunchUp(float jumpMultiplier = 1.5f){
        verticalVelocity = -jumpImpulse * jumpMultiplier;
    }
    public void Possess(PlayerController player){
        player.Mount(this);
    }
}
