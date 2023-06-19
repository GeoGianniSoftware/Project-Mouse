using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum PlayerMovementState {
    Idle,
    Walking,
    Running,
    Jumping,
    Falling,
    Landing,
    Climbing
}

[RequireComponent(typeof(Rigidbody2D))]

public class PlayerMovement : MonoBehaviour
{
    Rigidbody2D RB;
    Collider2D COL;
    PlayerFXManager FXM;

    public float maxStamina;
    public float staminaRegainRate;
    float currentStamina;

    public PlayerMovementState currentState;
    GameObject GFX; 
    public LayerMask climbableLayers;
    public LayerMask groundLayers;
    public float moveSpeed;
    public float runModifier;
    public float jumpForce;
    public float climbSpeed;

    public float idle;
    public float stun;

    public float runEffort;

    bool running;

    public bool canControl = true;
    bool canClimb = true;
    bool canRun = true;
    bool canJump = true;


    [System.NonSerialized]
    public bool isGrounded;
    [System.NonSerialized]
    public bool isTouchingWall;
    [System.NonSerialized]
    public bool isClimbing;
    int jumpDirection;
    [System.NonSerialized]
    public int gfxDirection = 1;

    Vector2 input;


    List<GameObject> ignoredObjects = new List<GameObject>();

    [Range(0.1f,5f)]
    public float fallModifier;

    public Vector2 sensorDistance;

    // Start is called before the first frame update
    void Start()
    {
        RB = GetComponent<Rigidbody2D>();
        COL = GetComponent<Collider2D>();
        FXM = GetComponent<PlayerFXManager>();
        GFX = transform.GetChild(0).gameObject;
        currentState = PlayerMovementState.Idle;
        currentStamina = maxStamina;
    }

    float fall1, fall2;
    // Update is called once per frame
    private void FixedUpdate ( ) {
        //Physics States
        if (isClimbing) {
            if (wallCleared) {
                RB.AddForce(transform.up * 2f, ForceMode2D.Impulse);
                RB.AddForce(transform.right * wallDirection * 1.5f, ForceMode2D.Impulse);

            }
            RB.gravityScale = 0;
            RB.sharedMaterial.friction = 1;
        }
        else {
            RB.gravityScale = 1;
            RB.sharedMaterial.friction = .6f;

        }

        //Add Fast Fall Force;
        if (!isGrounded && !isTouchingWall && RB.velocity.y < 0) {
            RB.AddForce(-transform.up * jumpForce * fallModifier, ForceMode2D.Force);
            if(currentState != PlayerMovementState.Falling) {
                fall1 = transform.position.y;

            }
            currentState = PlayerMovementState.Falling;

        }



        //Movement Finalize
        float finalY = input.y;
        if(!canClimb || !isTouchingWall) {
            finalY = 0;

        }
        if (Mathf.Abs(input.x) > 0 || Mathf.Abs(finalY) > 0) {

            Vector2 pow = new Vector2(input.x * jumpDirection, finalY);
            if (!DoGroundCheck()) {
                pow *= .5f;

            }
            Move(pow);

        }else if(isGrounded) {
            RB.velocity = new Vector2(0, RB.velocity.y);
        }else if (isClimbing) {
            RB.velocity = new Vector2(RB.velocity.x, 0);


        }

        //Wall Grabbing
        if (!isGrounded && isTouchingWall && canClimb) {
            isClimbing = true;
            currentState = PlayerMovementState.Climbing;
            //Wall Walk speed
            if (isTouchingWall && canClimb)
                input.y = Input.GetAxisRaw("Vertical") * climbSpeed;
            else
                input.y = 0;

            float yinputModded = input.y;
            if (yinputModded < 0)
                yinputModded = 0.25f;
            float climbEffort = (yinputModded * 1.3f);
            if(climbEffort <= 0) {
                climbEffort = 0.4f;

            }

            currentStamina -= Time.deltaTime * climbEffort;

        }
        else {
            isClimbing = false;


        }
    }


    bool firstButtonPressed;
    float timeOfFirstButton;
    bool reset;

    void Update()
    {
        //Ground Check Variable
        if (isGrounded != DoGroundCheck()) {
            Land();
            isGrounded = DoGroundCheck();
            if (isGrounded) {
                foreach(GameObject G in ignoredObjects) {
                    if(G.GetComponent<VerticalPlatform>())
                        G.GetComponent<VerticalPlatform>().Solidify();
                    else if(G.GetComponent<Collider2D>()) {
                        Physics2D.IgnoreCollision(COL, G.GetComponent<Collider2D>(), false);
                    }
                }
                ignoredObjects.Clear();

            }


        }
        //Wall Check Variable
       if(DoWallCheck(0) || DoWallCheck(1)) {
            if (!isTouchingWall) {
                RB.velocity = Vector3.zero;
                RB.angularVelocity = 0f;
                RB.Sleep();
                isTouchingWall = true;
                RB.WakeUp();


            }

        }
        else {
            isTouchingWall = false;

        }

        //Movement Axis Values and GFX direction
        float x, y;
            x = Input.GetAxisRaw("Horizontal");
            y = Input.GetAxisRaw("Vertical");
        input = new Vector2(x, y);
        if (x > 0){
                jumpDirection = 1;
                gfxDirection = 1;
            } 
            else if (x < 0) {
            if (isClimbing)
                jumpDirection = -1;
            else
                jumpDirection = 1;

            gfxDirection = -1;
        }
        else {
            if (isClimbing)
                jumpDirection = 0;
            else {

                jumpDirection = 1;
            }

        }
        
        
        if(isClimbing && isDangling && RB.velocity.y < 0) {
            RB.velocity = Vector3.zero;
            RB.angularVelocity = 0f;

        }


        //Bottom Out Stamina
        if (currentStamina < 0) currentStamina = 0;

        //Stamina Checks
        if(currentStamina <= 0) {
            canClimb = false;
            canJump = false;
            canRun = false;

        }
        else if(canControl) {
            canClimb = true;
            canJump = true;
            canRun = true;
        }


        //Jump Input
        if (Input.GetKeyDown(KeyCode.Space) && (isGrounded||(isTouchingWall&&!isGrounded)) && idle <= 0) {
            if(Mathf.Abs(input.x) == 0) {

                Jump(0);
            }
            else {

                Jump(input.x);
            }
           
        }

        //Player is Almost Idle
        float totalMove = input.x + input.y;
        if((currentState == PlayerMovementState.Walking|| currentState == PlayerMovementState.Running)&& Mathf.Approximately(totalMove, 0f)) {
            currentState = PlayerMovementState.Idle;
        }
        //Player is Idle
        if (currentState == PlayerMovementState.Idle && Mathf.Approximately(totalMove, 0f)) {
            RegainStamina(1f);
        }

        //Drop when Dangling
        if (isDangling && Drop()) {
            isClimbing = false;

        }

        //Stun
        if(stun > 0) {
            canControl = false;
            stun -= Time.deltaTime;
        }
        else {
            canControl = true;

        }

    }

    bool Drop(){

        if (Input.GetKeyDown(KeyCode.S) && firstButtonPressed) {
            if (Time.time - timeOfFirstButton < 0.5f) {
                return true;
            }
           

            reset = true;
        }

        if (Input.GetKeyDown(KeyCode.S) && !firstButtonPressed) {
            firstButtonPressed = true;
            timeOfFirstButton = Time.time;
        }

        if (reset) {
            firstButtonPressed = false;
            reset = false;
        }

        return false;
    }

    void Move (Vector2 dir) {
        if (!canControl)
            return;

        int buffer = 1;
        if (isClimbing)
            buffer = 0;
        else
            if (isGrounded) {
            currentState = PlayerMovementState.Walking;
            RegainStamina(.50f);

        }

        float runMod = 1;
        if (Input.GetKey(KeyCode.LeftShift) && canRun) {
            running = true;
            if (currentState == PlayerMovementState.Walking)
                currentState = PlayerMovementState.Running;
            runMod = runModifier;
            currentStamina -= Time.deltaTime * runEffort;
        }
        else {
            running = false;

        }

        RB.AddForce(new Vector2(dir.x * buffer, dir.y) * (moveSpeed*runMod) * Time.deltaTime, ForceMode2D.Impulse);

       


    }

    void Land ( ) {
        if (DoGroundCheck() == true) {
            if (currentState == PlayerMovementState.Falling) {
                GameObject g = Instantiate(Resources.Load("FX/SoftLand") as GameObject, transform.position - transform.up, Quaternion.identity);
                FXM.xStretchSquash(.35f);
                FXM.yStretchSquash(-.15f);
                currentState = PlayerMovementState.Landing;
                fall2 = transform.position.y;
                float dist = fall1 - fall2;
           
                if (dist > 5f) {
                    stun += .5f;
                    FXM.xStretchSquash(.55f);


                }
            }


            currentState = PlayerMovementState.Idle;
        }



    }

    public float getCurrentStamina(){ return currentStamina; }

    void Jump(float xForce){
        print("Jumping");
        if (!canControl || !canJump)
            return;

        if ((isClimbing && xForce == 0))
            return;


        Vector2 finalForce = new Vector2(xForce,1);
        float staminaMod = 1;

        if (xForce == 0)
            xForce *= 1f;
        else if (running) {
            staminaMod = 1.25f;
            xForce *= .45f;
        }
        else {
            xForce *= .40f;
        }


        print(finalForce*jumpForce);
        RB.AddForce(finalForce* jumpForce, ForceMode2D.Impulse);
        if(isClimbing || running)
         currentStamina -= 4f * staminaMod;

        currentState = PlayerMovementState.Jumping;


    }

    void WallGrab(){
    }

    public Vector2 skinWidth = new Vector2(.55f,.55f);
    public bool isDangling = false;

    bool wallCleared = false;
    int wallDirection = 0;
    bool DoWallCheck (int side) {
        //0 is left
        //1 is right
        List<RaycastHit2D> hits = new List<RaycastHit2D>();
        //Right Side
        hits.Add(Physics2D.Raycast((transform.position + (transform.up * .5f) + (transform.right * skinWidth.x)), transform.right, sensorDistance.x));
        hits.Add(Physics2D.Raycast((transform.position + (-transform.up * .25f) + (transform.right * skinWidth.x)), transform.right, sensorDistance.x));

        Debug.DrawRay((transform.position + (transform.up * .5f) + (transform.right * skinWidth.x)), transform.right * sensorDistance.x);
        Debug.DrawRay((transform.position + (-transform.up * .25f) + (transform.right * skinWidth.x)), transform.right * sensorDistance.x);

        //Left Side
        hits.Add(Physics2D.Raycast((transform.position + (transform.up * .5f) + (-transform.right * skinWidth.x)), -transform.right, sensorDistance.x));
        hits.Add(Physics2D.Raycast((transform.position + (-transform.up * .25f) + (-transform.right * skinWidth.x)), -transform.right, sensorDistance.x));

        Debug.DrawRay((transform.position + (transform.up * .5f) + (-transform.right * skinWidth.x)), -transform.right * sensorDistance.x);
        Debug.DrawRay((transform.position + (-transform.up * .25f) + (-transform.right * skinWidth.x)), -transform.right * sensorDistance.x);






        int triggerCount = 0;
        foreach(RaycastHit2D hit in hits) {
            if (hit.collider != null && climbableLayers == (climbableLayers | (1 << hit.transform.gameObject.layer))) {
                triggerCount++;
                if (hit.point.x < transform.position.x)
                    wallDirection = -1;
                if (hit.point.x > transform.position.x)
                    wallDirection = 1;
            }


        }

        if (triggerCount == 1 && (hits[1].collider != null || hits[3].collider != null)) {
            wallCleared = true;
        }
        else {
            wallCleared = false;

        }

        if (triggerCount == 1 && (hits[0].collider != null || hits[2].collider != null)) {
            isDangling = true;
        }
        else {
            isDangling = false;

        }

        if (triggerCount > 0)
            return true;
            else
            return false;
    }


    bool DoGroundCheck(){
        List<RaycastHit2D> hits = new List<RaycastHit2D>();

        Ray2D centerL = new Ray2D((transform.position + (-transform.up * skinWidth.y) + transform.right * -.15f), -transform.up);
        Ray2D centerR = new Ray2D((transform.position + (-transform.up * skinWidth.y) + transform.right*.15f), -transform.up);
        Ray2D left = new Ray2D((transform.position + (transform.right * skinWidth.x)) + (-transform.up * skinWidth.y), -transform.up);
        Ray2D right = new Ray2D((transform.position + (-transform.right * skinWidth.x)) + (-transform.up * skinWidth.y), -transform.up);

        hits.Add(Physics2D.Raycast((transform.position + (-transform.up * skinWidth.y) + transform.right * -.15f), -transform.up, sensorDistance.y));
        hits.Add(Physics2D.Raycast((transform.position + (-transform.up * skinWidth.y) + transform.right *  .15f), -transform.up, sensorDistance.y));


        hits.Add(Physics2D.Raycast((transform.position + (transform.right * skinWidth.x)) + (-transform.up * skinWidth.y), -transform.up, sensorDistance.y));
        hits.Add(Physics2D.Raycast((transform.position + (-transform.right * skinWidth.x)) + (-transform.up * skinWidth.y), -transform.up, sensorDistance.y));

        Debug.DrawRay(centerL.origin, centerL.direction * sensorDistance.y, Color.green);
        Debug.DrawRay(centerR.origin, centerR.direction * sensorDistance.y, Color.green);
        Debug.DrawRay(left.origin, left.direction * sensorDistance.y, Color.green);
        Debug.DrawRay(right.origin, right.direction * sensorDistance.y, Color.green);


        int triggerCount = 0;
        foreach (RaycastHit2D hit in hits) {
            if(hit.collider != null && groundLayers == (groundLayers | (1 << hit.transform.gameObject.layer))) {

                triggerCount++;

            }

        }
        if (triggerCount > 1) {
            return true;

        }

        return false;
    }

    void RegainStamina(float rateMod){
        if (currentStamina < maxStamina)
            currentStamina += Time.deltaTime * staminaRegainRate * rateMod;
        else if (currentStamina > maxStamina)
            currentStamina = maxStamina;
    }



}
