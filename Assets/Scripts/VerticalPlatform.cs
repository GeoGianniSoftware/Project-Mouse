using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Effector2D))]
public class VerticalPlatform : MonoBehaviour
{
    public LayerMask playerMask;
    //Minus the player
    public LayerMask allowedCollisions;

    LayerMask finalMask;
    PlayerMovement playerM;
    PlatformEffector2D effector;

    // Start is called before the first frame update
    void Start()
    {
        finalMask = (int)(playerMask | allowedCollisions);
        playerM = FindObjectOfType<PlayerMovement>();
        effector = GetComponent<PlatformEffector2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.S) && !playerM.isClimbing) {
           

        }
        else {


        }
    }

    public void Desolidify ( ) {
        effector.colliderMask = allowedCollisions;

    }

    public void Solidify ( ) {

        effector.colliderMask = playerMask;
    }
}
