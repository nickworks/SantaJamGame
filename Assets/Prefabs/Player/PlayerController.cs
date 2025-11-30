using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum SantaMode {
    Jumpy,
    SleighDriver,
}
[RequireComponent(typeof(CharacterController), typeof(SlopeBehavior))]
public class PlayerController : MonoBehaviour {
    private Camera cam;
    private SlopeBehavior pawn;
    private HudController hud;
    private Animator animator;
    private SleighController _vehicle;
    public SleighController vehicle { get { return _vehicle; } }
    public float walkSpeed = 5;

    ///<summary>the input, in world space</summary>
    private Vector3 inputDirection = new Vector3();

    private float timeLeftGrounded = 0;

    public bool isGrounded {
        // return true if pawn is on ground OR player has "coyote-time" left
        get => pawn.isGrounded;// || timeLeftGrounded > 0;
    }

    private SantaMode _mode = SantaMode.Jumpy;
    public SantaMode mode { get => _mode; }

    public List<InteractiveZone> interactiveZones = new List<InteractiveZone>();

    void Start() {
        cam = Camera.main;
        hud = FindObjectOfType<HudController>();
        pawn = GetComponent<SlopeBehavior>();
        animator = GetComponentInChildren<Animator>();
    }
    void UpdateLateralInput(){

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
    void Update(){
        UpdateLateralInput();
        switch(mode){
            case SantaMode.Jumpy:
                bool isJumpHeld = Input.GetButton("Jump");
                bool onJumpPress = Input.GetButtonDown("Jump");
                if (isGrounded && onJumpPress) {
                    // beginning a jump!
                    pawn.Jump();
                    timeLeftGrounded = 0; // not on ground (for animation's sake)
                }
                cam.fieldOfView = AnimMath.Slide(cam.fieldOfView, 60, .001f);
                break;
            case SantaMode.SleighDriver:
                cam.fieldOfView = AnimMath.Slide(cam.fieldOfView, 100, .001f);
                if(_vehicle){
                    if(Input.GetButtonDown("Interact")) Dismount();
                    else _vehicle.UpdateFromDriver(inputDirection);
                } else {
                    Dismount();
                }
                break;
        }

        UpdateAnimations();
        UpdateUI();
    }
    void FixedUpdate(){
        switch(mode){
            case SantaMode.Jumpy:
                // move pawn
                pawn.Move(inputDirection * walkSpeed);

                break;
            case SantaMode.SleighDriver:
                break;
        }
    }
    void UpdateAnimations(){
        switch(mode){
            case SantaMode.Jumpy:
                // if TRYING TO MOVE
                if (inputDirection.sqrMagnitude > .15f) {
                    // turn to face the correct direction
                    transform.rotation = AnimMath.Slide(transform.rotation, Quaternion.LookRotation(inputDirection, Vector3.up), .001f, Time.fixedDeltaTime);
                }
                animator?.SetBool("in air", !isGrounded);
                animator?.SetFloat("vertical speed", Mathf.Clamp(pawn.verticalVelocity / -8, -1, 1));
                animator?.SetFloat("horizontal input", inputDirection.sqrMagnitude);
                break;
            case SantaMode.SleighDriver:
                animator?.SetBool("in air", false);
                animator?.SetFloat("vertical speed", 0);
                animator?.SetFloat("horizontal input", 0);
            break;
        }

    }
    void UpdateUI(){

        if(hud) {
            string prompt = "";
            if(mode == SantaMode.Jumpy && interactiveZones.Count > 0){
                InteractiveZone zone = interactiveZones
                    .Where(z => z.playerShouldFaceCenter == false || Vector3.Dot(z.transform.position - this.transform.position, this.transform.forward) >.25f)
                    .OrderBy(z => z.interactPriority)
                    .FirstOrDefault();
                if(zone){
                    prompt = zone.interactPrompt;
                    if(Input.GetButtonDown("Interact")) zone.Interact(this);
                }
            }
            hud.textfield_interactprompt.text = prompt;
        }
    }
    void OnTriggerEnter(Collider trigger){
        InteractiveZone zone = trigger.GetComponent<InteractiveZone>();
        if(zone) interactiveZones.Add(zone);
    }
    void OnTriggerExit(Collider trigger){
        InteractiveZone zone = trigger.GetComponent<InteractiveZone>();
        if(zone) interactiveZones.Remove(zone);
    }
    public void LaunchUp(float jumpMultiplier = 1.5f){
        pawn.Jump(jumpMultiplier);
        timeLeftGrounded = 0;
    }
    public void Mount(SleighController vehicle){
        _mode = SantaMode.SleighDriver;
        this._vehicle = vehicle;
        this.pawn.TurnOff();
        this.transform.parent = _vehicle.transform;
        this.transform.localPosition = Vector3.up;
        this.transform.localRotation = Quaternion.identity;
        this.interactiveZones.Clear();
    }
    public void Dismount(){
        _mode = SantaMode.Jumpy;
        _vehicle.UpdateFromDriver(Vector3.zero);
        float yaw = this.transform.eulerAngles.y;
        this.transform.parent = null;
        this.transform.position += Vector3.up;
        this.transform.rotation = Quaternion.Euler(0, yaw, 0);
        this.pawn.TurnOn();
        LaunchUp(1.25f);
    }
}
