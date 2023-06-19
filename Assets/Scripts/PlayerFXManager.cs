using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFXManager : MonoBehaviour
{
    
    GameObject GFXroot;
    public float xScale, yScale;
    public float squashTimer = 0.2f;
    PlayerMovement playerMove;

    // Start is called before the first frame update
    void Start()
    {
        GFXroot = transform.GetChild(0).gameObject;
        playerMove = GetComponent<PlayerMovement>();
        xScale = 1; yScale = 1;
    }

    // Update is called once per frame
    void Update()
    {
        switch (playerMove.currentState) {
            case PlayerMovementState.Landing:
                //Handled in PlayerMovement Land();
                break;
            case PlayerMovementState.Running:
                xStretchSquash(.15f);
                break;


        }


        //Maths
        xScale = Mathf.Lerp(xScale, 1f , squashTimer);
        yScale = Mathf.Lerp(yScale, 1f, squashTimer);
        GFXroot.transform.localScale = new Vector3(xScale*playerMove.gfxDirection, yScale);


    }

    public void xStretchSquash(float amt ) {
        xScale = 1+amt;
        GFXroot.transform.localScale = new Vector3(xScale * playerMove.gfxDirection, yScale);
    }

    public void yStretchSquash(float amt) {
        yScale = 1+amt;
        GFXroot.transform.localScale = new Vector3(xScale * playerMove.gfxDirection, yScale);
    }
}
