using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spiderling : MonoBehaviour
{
    public Transform GFX;
    public float heightFromGround;
    public float liftForce;
    public float damping;
    public float legDist;
    public List<Transform> legs = new List<Transform>();
    public List<Transform> legData = new List<Transform>();
    public Vector3[] legPos = new Vector3[4];
    Rigidbody2D RB;
    public bool flip;
    public LayerMask flipLayers;

    // Start is called before the first frame update
    void Start()
    {
        RB = GetComponent<Rigidbody2D>();
        if (flip) {
            legs.Reverse();
            legData.Reverse();
        }
            
    }


    private void Update() {
        if(GFX != null) {
            if (flip)
                GFX.localScale = new Vector3(-1, 1, 1);
            else
                GFX.localScale = new Vector3(1, 1, 1);

        }
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        int dir = -1;
        if (flip)
            dir = 1;


        RB.AddForce(transform.right * dir, ForceMode2D.Force);

        RaycastHit2D hit = Physics2D.Raycast(transform.position-(transform.up*.2f), -Vector2.up, 2f);

        RaycastHit2D front = Physics2D.Raycast(transform.position - (transform.right * 1f), -transform.right, .2f);
        if(flip)
            front = Physics2D.Raycast(transform.position + (transform.right * 1f), transform.right, 2f);
       
        if(front.collider != null && front.collider.transform.parent != this) {
            if(flipLayers == (flipLayers | (1 << front.transform.gameObject.layer))) {
                print(front.transform.name);
                Flip();
            }
            
        }

        // If it hits something...
        if (hit.collider != null) {
            // Calculate the distance from the surface and the "error" relative
            // to the floating height.
            float distance = Mathf.Abs(hit.point.y - transform.position.y);
            float heightError = heightFromGround - distance;

            float force = liftForce * heightError - RB.velocity.y * damping;

            RB.AddForce(Vector2.up * force);

            if (distance < .6f && legs.Count > 0) {
                for (int i = 0; i < legs.Count; i++) {
                    legs[i].position = Vector2.Lerp(legs[i].position, legPos[i], .18f);

                    if (Vector2.Distance(legs[i].position, legData[i].position) > legDist + (i*.025f)) {
                        float randomStep = Random.Range(0.15f, 0.2f);
                        float offset = .6f;
                        if (flip)
                            offset = 0f;
                        legPos[i] = hit.point -(Vector2)(transform.right* offset) + (Vector2)(transform.right* (randomStep * i));
                    }
                }
                    
                }
            }
        
    }

    void Flip() {
        legs.Reverse();
        legData.Reverse();
        flip = !flip;
        RB.velocity = new Vector2(-RB.velocity.x, RB.velocity.y);
    }
}
