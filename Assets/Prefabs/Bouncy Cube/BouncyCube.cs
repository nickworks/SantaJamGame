using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncyCube : MonoBehaviour
{
    void Start()
    {
        
    }
    void OnTriggerEnter(Collider collider){
        collider.GetComponent<PlayerController>()?.LaunchUp();
    }
}
