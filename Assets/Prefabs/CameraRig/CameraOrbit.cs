using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOrbit : MonoBehaviour {

    public PlayerController player;
    private Camera cam;

    private float yaw = 0;
    private float pitch = 0;

    public float cameraMouseSensitivityX = 10;
    public float cameraMouseSensitivityY = 10;
    public float cameraStickSensitivityX = 10;
    public float cameraStickSensitivityY = 10;

    private void Start() {
        cam = GetComponentInChildren<Camera>();

    }
    void FixedUpdate(){

        Vector3 easeTowards = player.transform.position;

        // ease towards target:
        transform.position = AnimMath.Slide(transform.position, easeTowards, .001f, Time.fixedDeltaTime);

        // rotate camera towards pitch/yaw:
        transform.rotation = AnimMath.Slide(transform.rotation, Quaternion.Euler(pitch, yaw, 0), .0001f, Time.fixedDeltaTime);
    }
    void Update() {
        RotateRigFromInput();
    }
    private void RotateRigFromInput() {
        float mx1 = Input.GetAxisRaw("Mouse X") * cameraMouseSensitivityX;
        float my1 = Input.GetAxisRaw("Mouse Y") * cameraMouseSensitivityY;

        float mx2 = Input.GetAxisRaw("CameraX") * cameraStickSensitivityX;
        float my2 = Input.GetAxisRaw("CameraY") * cameraStickSensitivityY;

        if(Mathf.Abs(mx1) + Mathf.Abs(my1) > Mathf.Abs(mx2) + Mathf.Abs(my2)){
            yaw += mx1;
            pitch += my1;
        } else {
            yaw += mx2;
            pitch += my2;
        }

        pitch = Mathf.Clamp(pitch, -10, 89);
    }

}
