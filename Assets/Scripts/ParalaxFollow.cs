using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParalaxFollow : MonoBehaviour
{
    public Transform target;
    [Range(.000001f, 100f)]
    public float smooth;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(target != null) {
            transform.position = Vector3.Lerp(transform.position, new Vector2(target.position.x * .95f, transform.position.y), smooth);

        }
    }
}
