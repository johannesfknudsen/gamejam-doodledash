using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{

    private GameActions playerControls;
    private InputAction jumpAction;
    private Rigidbody2D rb;
    private Vector2 movement;

    public CardStack cardStack;

    [Header("GroundedMovement")]
    [SerializeField] private float speed = 8;
    [SerializeField] private float speedPower = 1f;
    [SerializeField] private float acceleration = 13f;
    [SerializeField] private float decceleration = 16f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 15;
    [SerializeField] private float fallGravity = 2;
    [SerializeField] private float jumpBuffer = 0.2f;
    [SerializeField] private float jumpCutMultiplier = 0.5f;
    [SerializeField] private float jumpCutWindow = 1f;
    [SerializeField] private float coyoteTimeWindow = 0.05f;
    private float jumpBufferTimer;
    private float coyoteTimeTimer;
    private float gravityScale = 1;
    private float jumpCutTimer;
    private bool jumpCutted = false;
    private bool isGrounded;
    private bool isJumping;

    [Header("PowerUp Settings")]
    [SerializeField] private float wallJumpXmultiplier;
    [SerializeField] private float dashSpeed = 15;
    [SerializeField] private float dashAngleMultiplier = 0.4f;
    [SerializeField] private float bounceMultiplier = 1.5f;
    [SerializeField] private GameObject tpPosition;
    [SerializeField] private float tpPositionMultiplier = 1.5f;

    private bool touchingWall = false;
    private bool bounceActivated = false;
    private float bouncePower;
    private PowerType nextPower;
    private Vector3 wallPos;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerControls = new GameActions();
        jumpAction = playerControls.Movement.Jump;
    }

    private void OnEnable()
    {
        playerControls.Enable();
        jumpAction.canceled += Jump;
    }
    private void OnDisable()
    {
        playerControls.Disable();
        jumpAction.canceled -= Jump;
    }

    private void Start()
    {
        gravityScale = rb.gravityScale;
        isJumping = false;

    }

    private void FixedUpdate()
    {

        #region This Code was made with this tutorial: https://www.youtube.com/watch?v=KbtcEVCM7bw
        //Calculating next direction of movement
        float nextMove = movement.x * speed;

        //Calculating the speed we are going and speed we want to be
        float speedDif = nextMove - rb.velocity.x;

        //calculating our accelarition/decceleration rate
            //Mathf.Abs returns an absolute meaning it cant be negative
            //not sure about the "?"
        float accelerationRate = (Mathf.Abs(nextMove) > 0) ? acceleration : decceleration;

        //Calculation The characters movement itself
            // Mathf.Sign returns a value of 1 or -1 to indicate the direction being right or left
            // Mathf.Pow Means to the power of something this is needed since its an acceleration and we want it to be smooth
        float moveAction = Mathf.Pow(Mathf.Abs(speedDif) * accelerationRate, speedPower) * Mathf.Sign(speedDif);

        rb.AddForce(moveAction * Vector2.right);
        #endregion

        if (rb.velocity.y < 0)
        {
            rb.gravityScale = gravityScale * fallGravity;
        }
        else
        {
            rb.gravityScale = gravityScale;
        }

        if (jumpBufferTimer > 0)
        {   
            jumpBufferTimer -= Time.fixedDeltaTime;
            if (isGrounded)
            {
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                Debug.Log("JUMP BUFFERED");
                isJumping = true;
                isGrounded = false;
                   
            }
        }

        if (jumpCutTimer > 0)
        {
            jumpCutTimer -= Time.fixedDeltaTime;
        }

        if (coyoteTimeTimer > 0)
        {
            coyoteTimeTimer -= Time.fixedDeltaTime;
        }

        //PowerUps
        if (cardStack.cards.Count > 0)
        {
            nextPower = cardStack.cards[0].power;
        }

        if (bounceActivated)
        {
            if (!isGrounded)
            {
                bouncePower = Mathf.Abs(rb.velocity.y);
            }
            
            if (isGrounded) 
            {
                rb.AddForce(Vector2.up * bouncePower * bounceMultiplier, ForceMode2D.Impulse);
                bounceActivated = false;
                Debug.Log(bouncePower);
            }


        }

        if (movement.x != 0)
        {
            tpPosition.transform.position = rb.position + movement * tpPositionMultiplier;
        }


    }

    private void OnMovement(InputValue input)
    {
        movement = input.Get<Vector2>();
    }

    private void OnJump(InputValue buttonPress)
    {   
        //Jumping
        if (isGrounded)
        {
            if (!isJumping)
            {
                jumpCutTimer = jumpCutWindow;
                isJumping = true;
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                isGrounded = false;
            }
        }
        else if (isGrounded == false)
        {   
            if (coyoteTimeTimer > 0)
            {
                Debug.Log("COYOTE JUMP MOVE");
                jumpCutTimer = jumpCutWindow;
                isJumping = true;
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
            jumpBufferTimer = jumpBuffer;
        }

    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (jumpCutted == false)
        {
            if (rb.velocity.y > 0 && jumpCutTimer > 0)
            {
                jumpCutted = true;
                Debug.Log("JumpCutted");
                rb.AddForce(Vector2.down * rb.velocity.y * (1 - jumpCutMultiplier), ForceMode2D.Impulse);
            }
        }
    }

    private void OnUsePowerUp()
    {
        if (cardStack.cards.Count > 0)
        {
            switch (nextPower)
            {
                case PowerType.Dash:
                    
                    if (movement.x != 0)
                    {
                        Debug.Log("DASH MOVE");

                        rb.AddForce(Vector2.right * movement * dashSpeed + Vector2.up * dashAngleMultiplier, ForceMode2D.Impulse);

                        cardStack.Use();
                    }
                    

                break;

                case PowerType.DoubleJump:
                    if (!isGrounded && isJumping)
                    {
                        Debug.Log("DOUBLE JUMP MOVE");
                        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                        cardStack.Use();

                    }
                break;

                case PowerType.WallJump:

                    if (touchingWall)
                    {
                        Debug.Log("WALL JUMP MOVE");

                        float wallJumpXDirection = Mathf.Sign(gameObject.transform.position.x - wallPos.x);

                        rb.AddForce(Vector2.up * jumpForce + Vector2.right * wallJumpXDirection * wallJumpXmultiplier, ForceMode2D.Impulse);

                        cardStack.Use();
                    }

                break;

                case PowerType.Teleport:
                    Debug.Log("TP MOVE");

                    transform.position = tpPosition.transform.position;
                    cardStack.Use();


                break;

                case PowerType.Bounce:
                    
                    if (!isGrounded)
                    {
                        Debug.Log("BOUNCE MOVE");

                        bounceActivated = true;
                        cardStack.Use();
                    }

                break;
            }

        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            isGrounded = true;
            isJumping = false;
            jumpCutted = false;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Wall")
        {
            touchingWall = true;

            wallPos = collision.transform.position;


        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            isGrounded = false;
            jumpCutted = false;

            if (isJumping == false)
            {
                coyoteTimeTimer = coyoteTimeWindow;
            }

        }

        if (collision.gameObject.tag == "Wall")
        {
            touchingWall = false;
        }

    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Collectable")
        {
            Destroy(collision.gameObject);
        }
    }

}
