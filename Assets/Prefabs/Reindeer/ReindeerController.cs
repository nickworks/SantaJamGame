using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ReindeerController : MonoBehaviour
{
    CharacterController pawn;
    public float speed = 4;

    public float maxSpeed = 10;

    private float verticalVelocity = 0;
    public float gravityMultiplier = 10;
    private Vector3 inputDirection;
    private Vector3 moveDirection;

    void Start(){
        pawn = GetComponent<CharacterController>();
    }
    public void Update(){
        verticalVelocity += gravityMultiplier * Time.deltaTime;
        UpdateAnimations();
    }
    public void UpdateFromDriver(Vector3 inputDir){
        inputDirection = inputDir;
    }
    public void FixedUpdate(){

        Vector3 steerForce = inputDirection * maxSpeed - moveDirection;
        moveDirection += steerForce * Time.fixedDeltaTime;

        //pawn.Move((moveDirection * speed + verticalVelocity * Vector3.down)  * Time.fixedDeltaTime);
        if (pawn.isGrounded) {
            verticalVelocity = 0;
        }
    }
    void UpdateAnimations(){
        // if TRYING TO MOVE
        if (inputDirection.sqrMagnitude > .15f) {
            // turn to face the correct direction
            transform.rotation = AnimMath.Slide(transform.rotation, Quaternion.LookRotation(moveDirection, Vector3.up), .001f);
        }
    }
}
