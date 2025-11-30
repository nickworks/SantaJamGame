using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SlopeBehavior : MonoBehaviour
{
    public LayerMask terrainLayers;
    public float leaveGroundDistanceThreshold = 1f;
    public float gravityMultiplier = 20;
    public float jumpImpulse = 10;
    CharacterController pawn;
    /// <summary>
    /// How fast the pawn is currently moving vertically (y-axis), in meters/second.
    /// </summary>
    public float verticalVelocity { get; set; }
    private bool _begin_jump = false;

    float halfHeight {
        get => pawn ? (pawn.radius * 2 > pawn.height) ? pawn.radius : pawn.height/2 : 0.5f;
    }
    private bool _isGrounded = false;
    public bool isGrounded { get => _isGrounded; }
    void Start(){
        pawn = GetComponent<CharacterController>();
    }
    public void Move(Vector3 moveDelta){
        if(_begin_jump == false){
            // gravity
            verticalVelocity += gravityMultiplier * Time.fixedDeltaTime;
        } else {
            _begin_jump = false;
        }

        bool wasGrounded = pawn.isGrounded;

        
        pawn.Move((moveDelta + verticalVelocity * Vector3.down) * Time.fixedDeltaTime);
        _isGrounded = pawn.isGrounded;
        if (wasGrounded && !_isGrounded) {
            // slid off the ground
            if(RayCastForGround(out RaycastHit hit, out float distanceToGround)){
                if(distanceToGround < 0) {
                    print("?");
                    _isGrounded = true;
                    //pawn.Move(Vector3.down * distanceToGround);
                }
                if(distanceToGround < leaveGroundDistanceThreshold) {
                    pawn.Move(Vector3.down * distanceToGround);
                }
            }
        }
        if (_isGrounded) {
            verticalVelocity = 0;
        }
    }
    
    public bool RayCastForGround(
        out RaycastHit hit,
        out float distanceToGround,
        float raycastDistance = 0f
    ){
        if(raycastDistance == 0) raycastDistance = leaveGroundDistanceThreshold;
        print("raycasting..");
        Debug.DrawRay(transform.position, Vector3.down * (raycastDistance + halfHeight));
        if(Physics.SphereCast(transform.position, 0.1f, Vector3.down, out hit, raycastDistance + halfHeight, terrainLayers)){
            distanceToGround = hit.distance - halfHeight;
            return true;
        }
        distanceToGround = float.PositiveInfinity;
        return false;
    }
    public void Jump(float multiplier = 1f){
        verticalVelocity = -jumpImpulse * multiplier;
        _begin_jump = true;
    }
    public void TurnOff(){
        this.pawn.enabled = false;
    }
    public void TurnOn(){
        this.pawn.enabled = true;
    }
}
