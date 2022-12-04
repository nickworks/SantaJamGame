using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractiveZone : MonoBehaviour
{
    public bool playerShouldFaceCenter = true;
    //[Tooltip()]
    public int interactPriority = 1;

    public string interactPrompt = "Interact";

    public UnityEvent<PlayerController> onPlayerInteract;

    void Start()
    {
    }
    public void Interact(PlayerController player){
        onPlayerInteract.Invoke(player);
    }
}
