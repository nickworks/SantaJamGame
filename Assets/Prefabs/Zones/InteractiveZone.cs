using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractiveZone : MonoBehaviour
{
    Collider collider;
    public bool playerShouldFaceCenter = true;
    //[Tooltip()]
    public int interactPriority = 1;

    public string interactPrompt = "Interact";

    public UnityEvent<PlayerController> onPlayerInteract;

    void Start()
    {
        collider= GetComponent<Collider>();
    }
    public void Interact(PlayerController player){
        onPlayerInteract.Invoke(player);
    }
}
