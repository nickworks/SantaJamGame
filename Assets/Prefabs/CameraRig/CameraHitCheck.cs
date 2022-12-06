using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CameraOrbit))]
public class CameraHitCheck : MonoBehaviour
{
    public LayerMask thingsThatBlockCamera;
    public float desiredDistance = 10;

    Camera cam;

    void Start() {
        cam = GetComponentInChildren<Camera>();
    }

    void FixedUpdate() {
        if(!cam) return;

        // TODO: animate

        float actualDistance = desiredDistance;

        // do a sphere-cast
        // from target(this) to the camera

        Vector3 pos = transform.position;
        Vector3 dir = cam.transform.position - pos;

        if(Physics.SphereCast(
            pos,
            0.5f,
            dir,
            out RaycastHit hit,
            desiredDistance,
            thingsThatBlockCamera
        )){
            actualDistance = hit.distance;
        }
        cam.transform.localPosition = new Vector3(0,0,-actualDistance);
    }
}
