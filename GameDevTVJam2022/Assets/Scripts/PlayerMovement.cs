using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    public enum PlayerState { walking, flat, flaming, headless };
    public PlayerState myState;
    [SerializeField] bool debugPos;
    Vector2 moveVector;
    Vector2 startPos;
    Rigidbody2D myRB;
    bool respawning;
    AudioSource myAS;
    [Header("Walk")]
    public bool canMove;
    [SerializeField] float moveSpeed;
    float standingGravity;
    float inAirGravity;
    [SerializeField] PhysicsMaterial2D fullFriction;
    [SerializeField] PhysicsMaterial2D noFriction;
    [SerializeField] Collider2D walkingCollider;
    [SerializeField] Collider2D flatCollider;
    [SerializeField] AudioClip respawnSFX;
    [Header("Ground Check")]
    [SerializeField] Vector2 groundCheckDims;
    [SerializeField] float groundCheckOffset;
    [SerializeField] LayerMask groundLayer;
    [Header("Jump")]
    [SerializeField] float jumpForce;
    [SerializeField] float walkingJumpForce;
    [SerializeField] float flatJumpForce;
    [SerializeField] float coyoteTime;
    [SerializeField] AudioClip[] jumpSFX;
    float jumpTimer;
    float startingJumpHeight;
    [SerializeField] float halfJumpHeight;
    bool jumpPressed;
    bool hasJumped;
    bool inAir;
    bool canJump;
    [Header("Flat")]
    [SerializeField] AudioClip flatSFX;
    [Header("Flaming")]
    [SerializeField] float flamingRunSpeed;
    [SerializeField] float wallCheckOffset;
    [SerializeField] float wallCheckSize;
    [SerializeField] ParticleSystem flamingParticles;
    int flameWallHitIndex;
    [SerializeField] SpriteRenderer mySR;
    ParticleSystem.EmissionModule flamingEmitter;
    float fullFlameEmitterRate;
    [SerializeField] Color level2FlameColor;
    [SerializeField] Color level3FlameColor;
    [SerializeField] ParticleSystem ashParticles;
    [SerializeField] AudioClip ashSFX;
    [Header("Headless")]
    [SerializeField]GameObject headGO;
    [SerializeField]Transform headHoldSpot;
    [SerializeField] Transform headStartSpot;
    bool headBeingHeld;
    Collider2D headCollider;
    [SerializeField] Vector2 headThrowForce;
    Collider2D myCollider;
    Rigidbody2D headRB;
    [SerializeField] LayerMask headLayer;
    float headTimer;
    [SerializeField] ParticleSystem decapParticles;
    [SerializeField] ParticleSystem neckBloodParticles;
    ParticleSystem.EmissionModule neckBloodEmitter;
    [SerializeField] float decapSpin;
    float neckBloodEmitRate;
    [SerializeField] AudioClip decapSFX;
    [Header("Squash and Stretch")]
    [SerializeField] Transform squashPoint;
    float playerXScale;
    float playerYScale;
    [SerializeField] float startingFlipScale;
    [SerializeField] float startingLandScale;
    [SerializeField] float scaleChangeSpeed;
    [SerializeField] float heightToSquash;
    bool facingRight;
    [SerializeField] float maxInAirHeight;
    [SerializeField] ParticleSystem landParticles;
    [Header("Animation")]
    [SerializeField] string currentAnimState;
    Animator myAnimator;
    const string IDLE = "PlayerIdle";
    const string RUN = "PlayerRun";
    const string JUMP = "PlayerJump";
    const string FALL = "PlayerFall";
    string animModifier;
    bool canIdle;
    public bool gameEnd;
    // Start is called before the first frame update
    void Start()
    {
        myRB = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        standingGravity = myRB.gravityScale * 2;
        inAirGravity = myRB.gravityScale;
        playerXScale = 1;
        playerYScale = 1;
        facingRight = true;
        headCollider = headGO.GetComponent<Collider2D>();
        myCollider = GetComponent<Collider2D>();
        headRB = headGO.GetComponent<Rigidbody2D>();
        neckBloodEmitter = neckBloodParticles.emission;
        neckBloodEmitRate = neckBloodEmitter.rateOverTime.constant;
        neckBloodEmitter.rateOverTime = 0;
        flamingEmitter = flamingParticles.emission;
        fullFlameEmitterRate = flamingEmitter.rateOverTime.constant;
        flamingEmitter.rateOverTime = 0;
        startPos = transform.position;
        myAS = GetComponent<AudioSource>();
        if(debugPos)
        { transform.position = new Vector2(PlayerPrefs.GetFloat("playerPosX"), PlayerPrefs.GetFloat("PlayerPosY")); }
    }

    // Update is called once per frame
    void Update()
    {
        if(!canMove || gameEnd)
        { return; }
        MoveHandle();
        JumpHandle();
        if (myState == PlayerState.walking)
        {
            animModifier = "";
            jumpForce = walkingJumpForce;
        }
        if (myState == PlayerState.flat)
        {
            animModifier = "Flat";
            jumpForce = flatJumpForce;
        }
        if (myState == PlayerState.flaming)
        {
            FlamingHandle();
            jumpForce = walkingJumpForce;
        }
        if (myState == PlayerState.headless)
        {
            animModifier = "Headless";
            jumpForce = walkingJumpForce;
            if (headBeingHeld)
            {
                headRB.velocity = Vector2.zero;
                headGO.transform.position = headHoldSpot.position;
                Physics2D.IgnoreCollision(myCollider, headCollider, true);
            }
            else
            {
                headTimer -= Time.deltaTime;
                if(headTimer<=0)
                { Physics2D.IgnoreCollision(myCollider, headCollider, false); }
            }
        }
        if(myState != PlayerState.headless)
        { 
            neckBloodEmitter.rateOverTime = 0;
            headGO.SetActive(false);
        }
        if (myState != PlayerState.flaming)
        { 
            flamingEmitter.rateOverTime = 0;
            flameWallHitIndex = 0;
            mySR.color = Color.white;
            mySR.enabled = true;
        }
        if (myState != PlayerState.flat)
        {
            walkingCollider.enabled = true;
            flatCollider.enabled = false;
        }
        else
        {
            walkingCollider.enabled = false;
            flatCollider.enabled = true;
        }
        SquashAndStretchHandle();
        if(respawning)
        { 
            myRB.velocity = new Vector2(0, myRB.velocity.y);
        }
    }
    private void OnApplicationQuit()
    {
        if(debugPos)
        {
            PlayerPrefs.SetFloat("playerPosX", transform.position.x);
            PlayerPrefs.SetFloat("playerPosY", transform.position.y);
        }
    }
    private void FlamingHandle()
    {
        ChangeAnimationState("PlayerRunFlaming");
        if(flameWallHitIndex == 0)
        {
            flamingEmitter.rateOverTime = fullFlameEmitterRate;
        }
        else if(flameWallHitIndex == 1)
        {
            mySR.color = level2FlameColor;
        }
        else if (flameWallHitIndex == 2)
        {
            mySR.color = level3FlameColor;
        }
        else if(flameWallHitIndex == 3)
        {
            flameWallHitIndex++;
            flamingEmitter.rateOverTime = 0;
            mySR.enabled = false;
            ashParticles.Play();
            myAS.PlayOneShot(ashSFX);
            StartCoroutine(Respawn());
        }
        if (facingRight)
        {
            myRB.velocity = new Vector2(flamingRunSpeed, myRB.velocity.y);
            if (Physics2D.OverlapCircle(new Vector2(transform.position.x + wallCheckOffset, transform.position.y), wallCheckSize, groundLayer))
            { 
                FlipLeft();
                flameWallHitIndex++;
            }
            
        }
        else
        {
            myRB.velocity = new Vector2(-flamingRunSpeed, myRB.velocity.y);
            if (Physics2D.OverlapCircle(new Vector2(transform.position.x - wallCheckOffset, transform.position.y), wallCheckSize, groundLayer))
            { 
                FlipRight();
                flameWallHitIndex++;
            }
            
        }
    }

    private void MoveHandle()
    {
        if (myState == PlayerState.flaming)
        { return; }
        myRB.velocity = new Vector2(moveVector.x * moveSpeed, myRB.velocity.y);
        if (GroundCheck() && !hasJumped)
        {
            if (moveVector.x == 0)
            {
                if(canIdle)
                { ChangeAnimationState(IDLE+animModifier); }
                myRB.sharedMaterial = fullFriction;
            }
            else
            {
                ChangeAnimationState(RUN + animModifier);
                myRB.sharedMaterial = noFriction;
            }
        }
    }
    public void IdleAnimCheck(int idleCheck)
    {
        if (idleCheck == 1)
        { canIdle = true; }
        else 
        { canIdle = false; }
    }
    void JumpHandle()
    {
        if (GroundCheck())
        {
            jumpTimer = coyoteTime;
            if (inAir && myRB.velocity.y <= 2)
            {
                inAir = false;
                hasJumped = false;
                startingJumpHeight = 0;
                myRB.gravityScale = standingGravity;
                if (maxInAirHeight > transform.position.y + halfJumpHeight + 1)
                {
                    playerYScale = startingLandScale;
                    landParticles.Play();
                }
                maxInAirHeight = 0;
            }
        }
        else
        {
            myRB.sharedMaterial = noFriction;
            jumpTimer -= Time.deltaTime;
            inAir = true;
            if (maxInAirHeight == 0 && myRB.velocity.y < 0)
            { maxInAirHeight = transform.position.y; }
            if(myState != PlayerState.flaming)
            {
                if (myRB.velocity.y > 0)
                { ChangeAnimationState(JUMP + animModifier); }
                else
                { ChangeAnimationState(FALL + animModifier); }
            }
        }
        if (jumpPressed && !hasJumped && jumpTimer >= 0 && canJump)
        {
            myRB.sharedMaterial = noFriction;
            myRB.velocity = new Vector2(myRB.velocity.x, jumpForce);
            myAS.PlayOneShot(jumpSFX[Random.Range(0, jumpSFX.Length)]);
            myRB.gravityScale = inAirGravity;
            canJump = false;
            hasJumped = true;
            startingJumpHeight = transform.position.y;
        }
        if (hasJumped && !jumpPressed && transform.position.y > startingJumpHeight + halfJumpHeight && myRB.velocity.y > 0)
        {
            myRB.velocity = new Vector2(myRB.velocity.x, myRB.velocity.y * .5f);
        }

    }
    void SquashAndStretchHandle()
    {
        squashPoint.transform.localScale = new Vector2(playerXScale, playerYScale);
        if (myState != PlayerState.flaming)
        { HandleFlip(); }
        if (facingRight)
        {
            if (playerXScale < 1)
            { playerXScale += Time.deltaTime * scaleChangeSpeed; }
            else
            { playerXScale = 1; }
        }
        else
        {
            if (playerXScale > -1)
            { playerXScale -= Time.deltaTime * scaleChangeSpeed; }
            else
            { playerXScale = -1; }
        }
        if (playerYScale < 1)
        { playerYScale += Time.deltaTime * scaleChangeSpeed; }
        else
        { playerYScale = 1; }
    }
    private void HandleFlip()
    {
        if (myRB.velocity.x > 0 && !facingRight)
        {
            FlipRight();
        }
        else if (myRB.velocity.x < 0 && facingRight)
        {
            FlipLeft();
        }
    }
    void FlipLeft()
    {
        facingRight = false;
        playerXScale = -startingFlipScale;
    }
    void FlipRight()
    {
        facingRight = true;
        playerXScale = startingFlipScale;
    }
    public void DecapitateMe()
    {
        if (myState == PlayerState.headless)
        { return; }
        neckBloodEmitter.rateOverTime = neckBloodEmitRate;
        decapParticles.Play();
        Physics2D.IgnoreCollision(myCollider, headCollider, true);
        headTimer = .2f;
        headGO.transform.position = headStartSpot.position;
        headGO.SetActive(true);
        Rigidbody2D headRB = headGO.GetComponent<Rigidbody2D>();
        if (facingRight)
        {
            decapParticles.transform.localScale = Vector3.one;
            headGO.transform.localScale = Vector2.one;
            headRB.velocity = new Vector2(-headThrowForce.x, headThrowForce.y);
            headRB.angularVelocity = decapSpin;
        }
        else
        {
            decapParticles.transform.localScale = new Vector3(-1, 1, 1);
            headGO.transform.localScale = new Vector2(-1, 1);
            headRB.velocity = headThrowForce;
            headRB.angularVelocity = -decapSpin;
        }
        ChangePlayerState(PlayerState.headless);
        myAS.PlayOneShot(decapSFX);
        StartCoroutine(Respawn());
    }
    public void CrushMe()
    {
        if (facingRight)
        {
            decapParticles.transform.localScale = Vector3.one;
        }
        else
        {
            decapParticles.transform.localScale = new Vector3(-1, 1, 1);
        }
        decapParticles.Play();
        playerYScale = .1f;
        ChangePlayerState(PlayerState.flat);
        myAS.PlayOneShot(flatSFX);
        StartCoroutine(Respawn());
        
    }
    IEnumerator Respawn()
    {
        respawning = true;
        yield return new WaitForSeconds(1.5f);
        if (myState == PlayerState.flaming)
        { myState = PlayerState.walking; }
        transform.position = MoveToRespawn();
        myAS.PlayOneShot(respawnSFX);
        respawning = false;
    }
    Vector2 MoveToRespawn()
    {
        
        Checkpoint[] checkpoints = FindObjectsOfType<Checkpoint>();
        foreach(Checkpoint checkpoint in checkpoints)
        {
            if(checkpoint.activeCheckpoint)
            { return checkpoint.transform.position; }
        }
        return startPos;
    }
    bool GroundCheck()
    {
        if (Physics2D.OverlapBox(new Vector2(transform.position.x, transform.position.y + groundCheckOffset), groundCheckDims, 0, groundLayer))
        { return true; }
        else
        { return false; }
    }
    public void MoveInput(InputAction.CallbackContext act)
    {
        moveVector = act.ReadValue<Vector2>();
    }
    public void JumpInput(InputAction.CallbackContext act)
    {
        if(respawning)
        { return; }
        if (act.performed)
        {
            if (canMove == false)
            { 
                canMove = true;
                FindObjectOfType<TutorialCanvas>().TutLeave();
                return;
            }
            jumpPressed = true; 
        }
        if (act.canceled)
        {
            jumpPressed = false;
            canJump = true;
        }
    }
    public void ActionInput(InputAction.CallbackContext act)
    {
        if (act.performed)
        { 
            if(myState == PlayerState.headless)
            {
                if(headBeingHeld)
                {
                    headBeingHeld = false;
                    if(facingRight)
                    { headGO.GetComponent<Rigidbody2D>().velocity = headThrowForce; }
                    else
                    { headGO.GetComponent<Rigidbody2D>().velocity = new Vector2(-headThrowForce.x, headThrowForce.y); }
                    headTimer = .2f;
                }
                else
                {
                    if (facingRight && Physics2D.OverlapCircle(new Vector2(transform.position.x + wallCheckOffset, transform.position.y), wallCheckSize * 2, headLayer)
                                        || !facingRight && Physics2D.OverlapCircle(new Vector2(transform.position.x - wallCheckOffset, transform.position.y), wallCheckSize * 2, headLayer))
                    {
                        headBeingHeld = true;
                    }
                } 
            }
        }
    }
    public void ChangePlayerState(PlayerState newState)
    { myState = newState; }
    void ChangeAnimationState(string newState)
    {
        if (newState == currentAnimState)
        { return; }
        currentAnimState = newState;
        myAnimator.Play(currentAnimState);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(new Vector2(transform.position.x, transform.position.y + groundCheckOffset), groundCheckDims);
        Gizmos.DrawRay(transform.position, Vector2.up * halfJumpHeight);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(new Vector2(transform.position.x + wallCheckOffset, transform.position.y), wallCheckSize);
    }
}
