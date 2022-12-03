using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ReindeerController : MonoBehaviour
{
    CharacterController pawn;
    public float speed = 4;
    private float verticalVelocity = 0;
    public float gravityMultiplier = 10;
    private Vector3 inputDirection;

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
        pawn.Move((inputDirection * speed + verticalVelocity * Vector3.down)  * Time.fixedDeltaTime);
        if (pawn.isGrounded) {
            verticalVelocity = 0;
        }
    }
    void UpdateAnimations(){
        // if TRYING TO MOVE
        if (inputDirection.sqrMagnitude > .15f) {
            // turn to face the correct direction
            transform.rotation = AnimMath.Slide(transform.rotation, Quaternion.LookRotation(inputDirection, Vector3.up), .001f);
        }
    }
}
