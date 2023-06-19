using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnoreColls : MonoBehaviour
{
    public LayerMask ignoreLayer;
    public LayerMask layersIgnored;
    LayerMask oldLayer;

    //void Awake()
    //{
    //    oldLayer = (oldLayer | (1 << transform.gameObject.layer));
    //    gameObject.layer = ignoreLayer;
    //    Physics2D.ig

    //}

    //public void Desolidify ( ) {
    //    effector.colliderMask = allowedCollisions;

    //}

    //public void Solidify ( ) {

    //    effector.colliderMask = playerMask;
    //}

}
