using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SmoothFollow : MonoBehaviour
{
    public Transform target;
    public Vector2 offset;
    [Range(.001f, 1f)]
    public float smooth;

        private void Update ( ) {
        if(target != null) {
            Vector3 newPos = new Vector3(target.position.x + offset.x, target.position.y + offset.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, newPos, smooth) ;



        }
    }
}
