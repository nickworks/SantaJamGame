using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(Rigidbody))]
public class SleighController : MonoBehaviour
{
    private Camera cam;
    private BoxCollider sleigh_collider;
    private Rigidbody sleigh_body;

    public float jumpImpulse = 5;

    public ReindeerTeam reindeerTeam;
    public ReindeerTeam reindeerTeamPrefab;
    
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
        if(Input.GetButtonDown("More Deer")) AddReindeerTeam();
        if(Input.GetButtonDown("Less Deer")) RemoveReindeerTeam();
    }
    public void UpdateFromDriver(Vector3 inputDir) {

        inputDirection = inputDir;

        reindeerTeam?.UpdateFromDriver(inputDir);

        //foreach(ReindeerController deer in attachedReindeer){
        //    deer.UpdateFromDriver(inputDirection);
        //}

        // rotate to face up:
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
    public void AddReindeerTeam(){

        Transform xform = reindeerTeam?.Last().transform??transform;
        Vector3 pos = xform.position + xform.forward * 6;
        Quaternion rot = Quaternion.Euler(0,xform.eulerAngles.y,0);

        ReindeerTeam newTeam = Instantiate(reindeerTeamPrefab, pos, rot);
        newTeam.AddDeer(2);

        if(reindeerTeam){
            reindeerTeam.AddTeam(newTeam);
        } else {
            reindeerTeam = newTeam;
            ConfigurableJoint joint = newTeam.GetComponent<ConfigurableJoint>();
            joint.connectedBody = sleigh_body;
        }
    }
    public void RemoveReindeerTeam(){
        if(reindeerTeam.nextRow == null){
            Destroy(reindeerTeam.gameObject);
            reindeerTeam = null;
        } else {
            reindeerTeam.RemoveLastRow();
        }
    }
}

[CustomEditor(typeof(SleighController))]
public class SleighControllerEditor : Editor {
    public override void OnInspectorGUI()
    {
        SleighController sleigh = target as SleighController;
        base.OnInspectorGUI();
        if(GUILayout.Button("Add reindeer row")){
            sleigh.AddReindeerTeam();
        }
        if(GUILayout.Button("Remove last row")){
            sleigh.RemoveReindeerTeam();
        }
    }
}
