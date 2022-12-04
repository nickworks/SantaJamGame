using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ReindeerTeam : MonoBehaviour
{
    public ReindeerController prefabReindeer;
    public float spaceSide = 2;
    public float spaceFront = 3;
    public float spaceBehind = 1;
    
    private List<ReindeerController> deer = new List<ReindeerController>();
    public ReindeerTeam _nextRow;
    public ReindeerTeam nextRow { get { return _nextRow; } }

    private Rigidbody body;

    void PositionDeer(){
        int deerInRow = deer.Count;
        for(int ii = 0; ii < deerInRow; ii++){
            float x = 0;
            if(deerInRow > 1){
                float w = (deerInRow - 1) * spaceSide;
                x += spaceSide * ii - w / 2;
            }
            deer[ii].transform.localPosition = new Vector3(x, 0, 0);
        }
    }
    public void AddDeer(int num){
        for(int i = 0; i < num; i++){
            deer.Add(Instantiate(prefabReindeer, this.transform));
        }
        PositionDeer();
    }
    public ReindeerTeam Last(){
        return _nextRow?.Last() ?? this;
    }
    public void AddTeam(ReindeerTeam newTeam){
        if(_nextRow == null){
            _nextRow = newTeam;
            ConfigurableJoint joint = newTeam.GetComponent<ConfigurableJoint>();
            joint.connectedBody = body;
            body.isKinematic = false;
            pawn.enabled = false;
            newTeam.GetComponent<CharacterController>().enabled = true;
        } else {
            _nextRow.AddTeam(newTeam);
        }
    }
    public void RemoveLastRow(){
        if(_nextRow.nextRow == null){
            Destroy(_nextRow.gameObject);
            _nextRow = null;
            body.isKinematic = true;
            pawn.enabled = true;
        } else if(_nextRow == null){
            print("memory leak?");
            Destroy(gameObject);
        } else {
            _nextRow.RemoveLastRow();
        }
    }

    CharacterController pawn;
    ConfigurableJoint joint;
    public float speed = 4;
    public float maxSpeed = 10;
    private float verticalVelocity = 0;
    public float gravityMultiplier = 10;
    private Vector3 inputDirection;
    private Vector3 moveDirection;

    void Start(){
        pawn = GetComponent<CharacterController>();
        joint = GetComponent<ConfigurableJoint>();
        body = GetComponent<Rigidbody>();
    }
    public void Update(){
        verticalVelocity += gravityMultiplier * Time.deltaTime;
        UpdateAnimations();
    }
    public void UpdateFromDriver(Vector3 inputDir){
        if(nextRow){
            nextRow?.UpdateFromDriver(inputDir);
        } else {
            inputDirection = inputDir;
        }
    }
    public void FixedUpdate(){
        //if(joint.connectedBody){
        //    // rotate
        //    Vector3 toNextRow = transform.TransformPoint(joint.anchor) - joint.connectedBody.transform.position;
        //    Quaternion rot = Quaternion.FromToRotation(Vector3.forward, toNextRow);
        //    joint.connectedBody.AddTorque(rot.eulerAngles * 100);
        //}
        if(pawn.enabled == false) return;

        Vector3 steerForce = inputDirection * maxSpeed - moveDirection;
        moveDirection += steerForce * Time.fixedDeltaTime;

        if(pawn.enabled) pawn.Move((moveDirection * speed + verticalVelocity * Vector3.down)  * Time.fixedDeltaTime);
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
[CustomEditor(typeof(ReindeerTeam))]
public class ReindeerTeamInspector : Editor {
    public override void OnInspectorGUI()
    {
        ReindeerTeam team = target as ReindeerTeam;
        base.OnInspectorGUI();
        if(GUILayout.Button("Add 1 Reindeer")){
            team.AddDeer(1);
        }
        if(GUILayout.Button("Remove Last Row")){
            team.RemoveLastRow();
        }
    }
}
