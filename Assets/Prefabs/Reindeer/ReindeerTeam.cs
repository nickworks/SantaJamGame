using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ReindeerTeam : MonoBehaviour
{
    public ReindeerController prefabReindeer;
    public AnimationCurve boostFalloff;
    public float hoverHeight = 2;
    public float hoverMaxSpeed = 10;
    public float hoverForce = 20;
    public float spaceBetweenDeer = 2;
    public LayerMask terrainMask;
    private List<ReindeerController> deer = new List<ReindeerController>();
    public ReindeerTeam _nextRow;
    public ReindeerTeam nextRow { get { return _nextRow; } }

    private Rigidbody body;

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
            LookAhead();
        }
    }
    public void FixedUpdate(){
        if(pawn.enabled == false) return;

        Vector3 steerForce = inputDirection * speed - moveDirection;
        moveDirection += steerForce * Time.fixedDeltaTime;

        Hover();

        if(pawn.enabled) pawn.Move((moveDirection * speed + verticalVelocity * Vector3.down)  * Time.fixedDeltaTime);
        if (pawn.isGrounded) {
            verticalVelocity = 0;
        }
    }
    void Hover(){
        float dis = hoverHeight;
        Vector3 origin = transform.position;
        Debug.DrawLine(origin, origin + Vector3.down * dis);
        if(Physics.SphereCast(origin, 0.5f, Vector3.down, out RaycastHit hit1, dis, terrainMask)){
            if(body){
                float percent1 = Mathf.Clamp(-verticalVelocity / hoverMaxSpeed, 0, 1);
                float percent2 = Mathf.Clamp(hit1.distance / dis, 0, 1);
                float percent = (percent1 + percent2)/2;
                float acceleration = hoverForce * boostFalloff.Evaluate(percent);
                verticalVelocity -= acceleration * Time.deltaTime;
            }
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
