using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour {
    private Camera cam;
    private CharacterController pawn;
    private Animator animator;
    public float walkSpeed = 5;

    public float gravityMultiplier = 10;
    public float jumpImpulse = 5;

    ///<summary>the input, in world space</summary>
    private Vector3 inputDirection = new Vector3();

    private float timeLeftGrounded = 0;

    public bool isGrounded {
        get { // return true if pawn is on ground OR "coyote-time" isn't zero
            return pawn.isGrounded || timeLeftGrounded > 0;
        }
    }

    /// <summary>
    /// How fast the player is currently moving vertically (y-axis), in meters/second.
    /// </summary>
    private float verticalVelocity = 0;

    void Start() {
        cam = Camera.main;
        pawn = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }
    void UpdateHorizontalInput(){

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        
        // if looking from above, should move the player up the screen (along the ground)
        // if looking from side, should move player into the screen (along the ground)
        float alignedHorizontal = Mathf.Abs(Vector3.Dot(cam.transform.up, Vector3.up));
        // 1 camera is horizontal
        // 0 camera is vertical
        // -1 camera is upside down ??
        Vector3 forward = Vector3.Lerp(cam.transform.up, cam.transform.forward, alignedHorizontal);
        inputDirection = forward * v + cam.transform.right * h;
        // the above input should be close to lateral (assuming no camera-roll)
        // but we can force-flatten it here
        inputDirection.y = 0;
        // if this changes, the player's rotation can get messed up
        // normalize the input
        if (inputDirection.sqrMagnitude > 1) inputDirection.Normalize();
    }
    void UpdateVerticalVelocity(){
        bool isJumpHeld = Input.GetButton("Jump");
        bool onJumpPress = Input.GetButtonDown("Jump");
        if (isGrounded && onJumpPress) {
            verticalVelocity = -jumpImpulse;
            timeLeftGrounded = 0; // not on ground (for animation's sake)
            // beginning a jump!
        } else {
            // apply gravity
            verticalVelocity += gravityMultiplier * Time.deltaTime;
        }
    }
    void Update() {

        UpdateHorizontalInput();
        UpdateVerticalVelocity();

        // move pawn
        Vector3 moveDelta = inputDirection * walkSpeed + verticalVelocity * Vector3.down;
        CollisionFlags flags = pawn.Move(moveDelta * Time.deltaTime);
        if (pawn.isGrounded) {
            verticalVelocity = 0;
            timeLeftGrounded = .2f;
        }

        UpdateAnimations();
    }
    void UpdateAnimations(){
        // if TRYING TO MOVE
        if (inputDirection.sqrMagnitude > .25f) {
            // turn to face the correct direction
            transform.rotation = AnimMath.Slide(transform.rotation, Quaternion.LookRotation(inputDirection, Vector3.up), .01f);
        }
        animator?.SetBool("in air", !isGrounded);
        animator?.SetFloat("vertical speed", Mathf.Clamp(verticalVelocity / -8, -1, 1));
        animator?.SetFloat("horizontal input", inputDirection.sqrMagnitude);
    }
    public void LaunchUp(float jumpMultiplier = 1.5f){
        verticalVelocity = -jumpImpulse * jumpMultiplier;
        timeLeftGrounded = 0;

    }
}
