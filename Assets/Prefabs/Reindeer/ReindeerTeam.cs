using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(SlopeBehavior),typeof(ConfigurableJoint))]
public class ReindeerTeam : MonoBehaviour
{
    static public float space_between_rows = 6;
    SlopeBehavior pawn;
    ConfigurableJoint joint;
    private ReindeerTeam _nextRow;
    private ReindeerTeam _prevRow;
    public ReindeerTeam nextRow { get => _nextRow; }
    public ReindeerTeam prevRow { get => _prevRow; }
    private List<ReindeerController> deer = new List<ReindeerController>();
    private Vector3 inputDirection;
    private Vector3 moveDirection;
    private Vector3 faceDirection;
    private Vector3 up;
    //private float verticalVelocity = 0;
    [Header("Generation")]
    public ReindeerController prefabReindeer;
    public float spaceBetweenDeer = 2;
    [Header("Physics")]
    public float accelerationFromInput = 4;
    public float speedMultiplier = 4;
    [Header("Flight Settings")]
    public AnimationCurve hoverFalloff;
    public float hoverHeight = 2;
    public float hoverMaxSpeed = 10;
    public float hoverForce = 20;
    public float gravityMultiplier = 10;
    public LayerMask terrainMask;

    void Start(){
        pawn = GetComponent<SlopeBehavior>();
        joint = GetComponent<ConfigurableJoint>();
    }
    void OnValidate(){
        PositionDeer();
    }
    public void InitAtEnd(ReindeerTeam prevRow, Rigidbody body){
        this._prevRow = prevRow;

        joint = GetComponent<ConfigurableJoint>();
        joint.connectedBody = body;

        pawn = GetComponent<SlopeBehavior>();
    }
    void PositionDeer(){
        int deerInRow = deer.Count;
        for(int ii = 0; ii < deerInRow; ii++){
            float x = 0;
            if(deerInRow > 1){
                float w = (deerInRow - 1) * spaceBetweenDeer;
                x += spaceBetweenDeer * ii - w / 2;
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
    public void AddTeam(ReindeerTeam newTeam, ReindeerTeam prevTeam = null){
        if(_nextRow == null){
            _nextRow = newTeam;
            newTeam.InitAtEnd(this, null);
        } else {
            _nextRow.AddTeam(newTeam, this);
        }
    }
    public void RemoveLastRow(){
        if(_nextRow.nextRow == null){
            Destroy(_nextRow.gameObject);
            _nextRow = null;
        } else if(_nextRow == null){
            print("memory leak?");
            Destroy(gameObject);
        } else {
            _nextRow.RemoveLastRow();
        }
    }
    public void PhysicsFromDeerCount(int totalDeer){
        float p = Mathf.Clamp(totalDeer / 9f, 0, 1);
        hoverForce = Mathf.Lerp(0, 50, p);
        gravityMultiplier = Mathf.Lerp(20, 5, p);
        speedMultiplier = Mathf.Lerp(10, 50, p);
    }
    // pass the input command up to the front
    public void UpdateFromDriver(Vector3 inputDir){
        if(nextRow){
            nextRow?.UpdateFromDriver(inputDir);
        } else {
            inputDirection = inputDir;
        }
    }
    public void FixedUpdate(){
        Move();
        Rotate();
    }
    void Hover(){
        if(pawn.RayCastForGround(out RaycastHit hit1, out float distanceToGround, hoverHeight)){
            float percent1 = Mathf.Clamp(-pawn.verticalVelocity / hoverMaxSpeed, 0, 1);
            float percent2 = Mathf.Clamp(distanceToGround / hoverHeight, 0, 1);
            float percent = (percent1 + percent2)/2;
            float acceleration = hoverForce * hoverFalloff.Evaluate(percent);
            pawn.verticalVelocity -= acceleration * Time.deltaTime;

            up = Vector3.Lerp(hit1.normal, Vector3.up, Mathf.Clamp(distanceToGround/2,0,1));
        } else {
            up = Vector3.up;
        }
    }
    void LookAhead(){
        float speed = moveDirection.magnitude;
        Vector3 offest = transform.forward + moveDirection * (1 + speed) / speed;
        Vector3 origin = transform.position + offest + Vector3.up * 5;
        //Debug.DrawLine(origin, origin + Vector3.down * 10.0f);
        //if(Physics.SphereCast(
        //    origin,
        //    2.0f,
        //    Vector3.down,
        //    out RaycastHit hit2,
        //    10.0f,
        //    terrainMask
        //    )){
        //    if(hit2.distance < 10){
        //        float boost = (10f - hit2.distance) * gravityMultiplier * 1.1f;
        //        verticalVelocity -= (boost) * Time.deltaTime;
        //    }
        //}
    }
    void Rotate(){
        if(nextRow == null) {
            if(moveDirection.sqrMagnitude > .01f) {   
                faceDirection = moveDirection;
            }
        } else {
            faceDirection = (nextRow.transform.position - this.transform.position);
            faceDirection.y = 0;
            faceDirection.Normalize();
        }
        if(up.sqrMagnitude < .1f) up = Vector3.up; // no zero-length up vectors
        Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, up) * Quaternion.LookRotation(faceDirection, Vector3.up);
        transform.rotation = AnimMath.Slide(transform.rotation, targetRotation, .01f, Time.fixedDeltaTime);
    }
    void Move(){
        
        // hover
        //Hover();

        if(nextRow == null) {
            // move the first reindeer
            Vector3 steerForce = inputDirection * accelerationFromInput - moveDirection;
            moveDirection += steerForce * Time.fixedDeltaTime;
            pawn.Move(moveDirection * speedMultiplier);
        } else {
            // slide towards next reindeer
            Vector3 d = nextRow.transform.position - this.transform.position;
            Vector3 moveTo = nextRow.transform.position - ReindeerTeam.space_between_rows * d.normalized;

            moveDirection = moveTo - transform.position;  
            pawn.Move(moveDirection * speedMultiplier);
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
