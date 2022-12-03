using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SleighController : MonoBehaviour
{
    private Camera cam;
    private BoxCollider sleigh_collider;
    private Rigidbody sleigh_body;

    public float acceleration = 5;

    public float gravityMultiplier = 10;
    public float jumpImpulse = 5;

    
    ///<summary>the input, in world space</summary>
    private Vector3 inputDirection = new Vector3();

    private float timeLeftGrounded = 0;

    /// <summary>
    /// How fast the player is currently moving vertically (y-axis), in meters/second.
    /// </summary>
    private float verticalVelocity = 0;

    void Start() {
        cam = Camera.main;
        sleigh_body = GetComponent<Rigidbody>();
        sleigh_collider = GetComponentInChildren<BoxCollider>();
    }
    public void UpdateFromDriver(Vector3 inputDir) {

        inputDirection = inputDir;
        //UpdateVerticalVelocity();

        // move pawn
        Vector3 moveDelta = inputDirection * acceleration;// + verticalVelocity * Vector3.down;
        if(Physics.BoxCast(
            transform.position + moveDelta + Vector3.up*.2f,
            sleigh_collider.transform.localScale,
            Vector3.down,
            out RaycastHit hit,
            sleigh_collider.transform.rotation,
            1
            )){
                
        }
        //this.transform.position += moveDelta * Time.deltaTime;
        sleigh_body.AddForce(moveDelta * Time.deltaTime);
        
        UpdateAnimations();
    }
    void UpdateAnimations(){
        // if TRYING TO MOVE
        if (inputDirection.sqrMagnitude > .15f) {
            // turn to face the correct direction
            transform.rotation = AnimMath.Slide(transform.rotation, Quaternion.LookRotation(inputDirection, Vector3.up), .001f);
        }
    }
    public void LaunchUp(float jumpMultiplier = 1.5f){
        verticalVelocity = -jumpImpulse * jumpMultiplier;
        timeLeftGrounded = 0;

    }
    public void Possess(PlayerController player){
        player.Mount(this);
    }
}
