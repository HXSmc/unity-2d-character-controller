using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterController2D : MonoBehaviour
{
    #region Components Variables
    [System.NonSerialized]static public Rigidbody2D rb;
    private CapsuleCollider2D col;
    #endregion

    #region On Ground Condition Variables
    [Header("On Ground Check")]
    static public bool isGrounded;
    public Transform groundCheck;
    [SerializeField] private float checkRadius;
    public LayerMask whatIsGround;
    #endregion

    #region On Wall Condition Variables
    [Header("On Wall Check")]
    public bool isTouchingFront;
    public Transform frontCheck;
    #endregion

    #region Horizontal Movement Variables
    [Header("Walk")]
    [SerializeField] private float moveSpeed;
    private float moveInput;

    [Header("Dash")] 
    public bool isDashing;
    [SerializeField] private float dashDistance;
    [SerializeField] private float dashTime;
    private float dashcoldown;
    public float startdashcoldown;
    private bool dashed = false;


    #endregion
    
    #region Vertical Movement Variables
    [Header("Jump")]
    public bool isJumping;
    [SerializeField] private int jumpForce;
    private float jumpTimeCounter;
    private bool jumpinput;
    [SerializeField] private float jumpTime;

    [Header("Double Jump")]
    [SerializeField] private int extraJumps;
    [SerializeField] private int extraJumpsValue;

    [Header("Wall Jump")]
    public bool wallJumping;
    [SerializeField] private float xWallForce;
    [SerializeField] private float yWallForce;
    [SerializeField] private float wallJumpTime;
    
    [Header("Wall Slide")]
    public bool wallSliding;
    [SerializeField]private float wallSlidingSpeed;
    #endregion


    private void Start()
    {
        
        #region Components
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
        #endregion

        #region Initialize Values
        extraJumps = extraJumpsValue;
        #endregion
    }

    private void FixedUpdate()
    {
        #region Change Character Facing Direction
        //when the player press walk right key
        if (moveInput > 0)
        {
            //character face right
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, 0, transform.eulerAngles.z);
        } 
        //when the player press walk left key
        else if (moveInput < 0)
        {
            //character face left
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, 180, transform.eulerAngles.z);
        }
        #endregion
        
        #region On Ground Condition Check
        //by creating a circle under player foot and check if the circle collides with the floor
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);
        #endregion

        #region On Wall Condition Check
        //by creating a circle in front of player and check if the circle collides with the wall
        isTouchingFront = Physics2D.OverlapCircle(frontCheck.position, checkRadius, whatIsGround);
        #endregion
        
        #region Walk Movement
        //to avoid conflict with dash movement
        if (!isDashing)
        {
            //normal walking speed, left and right
            rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        }
        #endregion

        #region Wall Slide Movement
        if (wallSliding)
        {
            //return a minimum falling speed of the character
            //so when player slide, it falls slowly than normal fall after jump
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
        }
        #endregion

        #region Wall Jump Movement
        if (wallJumping)
        {
            //jump to the opposite direction
            //such as when holding left key on the wall, it will jump to the right
            rb.velocity = new Vector2(xWallForce * -moveInput, yWallForce);
        }
        #endregion

        
        
    }
    #region movement input reader
    public void dash(InputAction.CallbackContext ctx){
        if(ctx.started){
            StartCoroutine(transform.localRotation.y == 0 ? Dash(1f) : Dash(-1f));
        }
    }
    
    public void Movement(InputAction.CallbackContext ctx){
        moveInput = ctx.ReadValue<float>(); //left -> move = -1, right -> move = 1
    }

    public void jump(InputAction.CallbackContext ctx){
        #region Wall Jump Control
        //if player press space while it is sliding
        if (ctx.started && wallSliding)
        {
            wallJumping = true;
            //set a duration for wall jump
            Invoke("SetWallJumpingToFalse", wallJumpTime);
        }
        #endregion


        #region jumping
        
        //when player press space and it is the first or second jump
        if (ctx.started && extraJumps > 0)
        {
            //each jump minus one, total two jumps
            extraJumps--;
            //reset for hold jump
            isJumping = true;
            jumpTimeCounter = jumpTime;
        }

        //if player is holding the space key and it is on midair
        if (ctx.performed && isJumping)
        {
            //when jump time is not finish and not dashing (avoid conflict with dash movement)
            if (jumpTimeCounter > 0 && !isDashing)
            {
                //increase the y-axis velocity until jump time is over
                rb.velocity = Vector2.up * jumpForce;
                //count down the jump time
                jumpTimeCounter -= Time.deltaTime;
            }
            //when jump time is over, player will reach the maximum height
            else
            {
                //stop increasing the move upward velocity
                isJumping = false;
            }
            jumpinput = true;
        }
        
        //if player stops holding down the space key
        if (ctx.canceled)
        {
            //stop increasing the move upward velocity
            isJumping = false;
            rb.gravityScale = 4;
            jumpinput = false;
        }
        #endregion
    }

    #endregion

    private void Update()
    {

        


        #region Jump Control
        //when it is on ground
        if (isGrounded)
        {
            //reset for double jump
            extraJumps = extraJumpsValue;
            rb.gravityScale = 2.8f;
        }
        
        //if player is holding the space key and it is on midair
        if (jumpinput && isJumping)
        {
            //when jump time is not finish and not dashing (avoid conflict with dash movement)
            if (jumpTimeCounter > 0 && !isDashing)
            {
                //increase the y-axis velocity until jump time is over
                rb.velocity = Vector2.up * jumpForce;
                //count down the jump time
                jumpTimeCounter -= Time.deltaTime;
            }
            //when jump time is over, player will reach the maximum height
            else
            {
                //stop increasing the move upward velocity
                isJumping = false;
            }
        }
        #endregion
        #region dash col down
        if(dashed){
            dashcoldown -= Time.deltaTime;
          if(dashcoldown <= 0){
            dashed = false;
            
        }
        }
        #endregion

        #region Wall Slide Control
        //if player touch the wall, not on the ground and holding the left and right key
        if (isTouchingFront && isGrounded == false && moveInput != 0)
        {
            //player is wall sliding
            wallSliding = true;
            rb.gravityScale = 4;
        }
        else
        {
            wallSliding = false;
        }
        #endregion

        
    }

  
        
        
        


    #region Stop Wall Jump
    void SetWallJumpingToFalse()
    {
        wallJumping = false;
    }
    #endregion
    
   
    #region Dash Movement
    IEnumerator Dash(float dashDirection)
    {
        if(dashcoldown <= 0)
        {//change to dash horizontal and vertical movement
        isDashing = true;
        //set x be faster velocity and y be 0 to avoid falling down while dashing
        rb.velocity = new Vector2(dashDistance * dashDirection, 0f);
        //store the initial gravity
        float gravity = rb.gravityScale;
        //set to no gravity to avoid falling
        rb.gravityScale = 0;
        //dashing time
        yield return new WaitForSeconds(dashTime);
        //return to normal horizontal and vertical movement
        isDashing = false;
        //restore the initial gravity value
        rb.gravityScale = gravity;
        //start col down
        dashed = true;
        dashcoldown = startdashcoldown;
        }
    }
    #endregion
}