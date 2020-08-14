using UnityEngine;

public class PlayerControllerNew : MonoBehaviour
{
    private float moveHorizontal;
    private bool isFacingRight = true;
    public bool crouch;
    public bool isGrounded;
    public bool isTouchingWall;
    public bool isWallSliding;
    private bool canNormalJump;
    private bool canWallJump;
    private bool isTaunting;
    public int jumpAmount;
    public int jumpLefts;
    public int facingDirection = 1;
    private int lastWallJumpDirection = 1;

    private float jumpTimer;
    private float jumpTimerSet = .15f;
    private bool isAttemptingToJump;
    private bool checkJumpMultiplier;
    private bool canMove;
    private bool canFlip;
    private bool hasWallJumped;
    private float turnTimer;
    private float turnTimerSet = .1f;
    private float wallJumpTimer;
    private float wallJumpTimerSet = .5f;

    public float walkSpeed;
    public float jumpForce = 16f;
    public float groundCheckRadius;
    public float wallCheckDistance;
    public float wallSlideSpeed;
    public float movementForceInAir;
    public float airDragMultiplier = 1f;
    public float variableJumpHeightMultiplier = .5f;
    public float crouchSpeedMultiplier = 0f;

    public float wallHopForce;
    public float wallJumpForce;

    public Vector2 wallHopDirection;
    public Vector2 wallJumpDirection;

    private Rigidbody2D rb;
    private Animator anim;
    public BoxCollider2D torsoCollider;
    public BoxCollider2D legColliders;

    public Transform groundCheck;
    public Transform wallCheck;

    public LayerMask whatIsGround;

    // Start is called before the first frame update
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        jumpLefts = jumpAmount;
        wallHopDirection.Normalize();
        wallJumpDirection.Normalize();
    }

    // Update is called once per frame
    private void Update()
    {
        //Taunt
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("SA-X_Taunt"))
        {
            moveHorizontal = 0;
            anim.SetBool("isTaunting", false);
            return;
        }

        CheckInput();
        UpdateAnimation();
        CheckMovementDirection();
        CheckIfCanJump();
        CheckIfWallSliding();
        CheckJump();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
        CheckSorroundings();
        DisableColliders();
    }

    private void UpdateAnimation()
    {
        if (!isGrounded && isTouchingWall && rb.velocity.y < 0)
            anim.SetBool("isJumping", !isWallSliding);
        else
            anim.SetBool("isJumping", !isGrounded);
        anim.SetBool("isOnWall", isWallSliding);
        anim.SetFloat("speed", Mathf.Abs(moveHorizontal * walkSpeed));
        anim.SetBool("isCrouching", crouch);

        anim.SetBool("isTaunting", isTaunting);
    }

    private void CheckInput()
    {
        moveHorizontal = Input.GetAxisRaw("Horizontal");
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded || (jumpLefts > 0 && !isTouchingWall))
            {
                NormalJump();
            }
            else
            {
                jumpTimer = jumpTimerSet;
                isAttemptingToJump = true;
            }
        }

        if (Input.GetButtonDown("Horizontal") && isTouchingWall)
        {
            if (!isGrounded && moveHorizontal != facingDirection)
            {
                canMove = false;
                canFlip = false;

                turnTimer = turnTimerSet;
            }
        }
        if (!canMove)
        {
            turnTimer -= Time.deltaTime;
            if (turnTimer <= 0)
            {
                canMove = true;
                canFlip = true;
            }
        }
        if (!Input.GetButton("Jump") && checkJumpMultiplier)
        {
            checkJumpMultiplier = false;
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * variableJumpHeightMultiplier);
        }
        if (Input.GetButton("Crouch"))
        {
            crouch = true;
        }
        if (Input.GetButtonUp("Crouch"))
        {
            crouch = false;
        }
        if (Input.GetButton("Taunt"))
            isTaunting = true;
        else isTaunting = false;
    }

    private void CheckMovementDirection()
    {
        if (isFacingRight && moveHorizontal < 0)
        {
            Flip();
        }
        else if (!isFacingRight && moveHorizontal > 0)
        {
            Flip();
        }
    }

    private void CheckIfCanJump()
    {
        if ((isGrounded && rb.velocity.y <= 0.01f))
        {
            jumpLefts = jumpAmount;
        }

        if (isTouchingWall)
        {
            canWallJump = true;
        }
        if (jumpLefts <= 0)
        {
            canNormalJump = false;
        }
        else
        {
            canNormalJump = true;  // Todo bien hasta aqui
            ////
            ////
            ///
            //
        }
    }

    private void CheckJump()
    {
        if (jumpTimer > 0)
        {
            //WallJump
            if (!isGrounded && isTouchingWall && moveHorizontal != 0 && moveHorizontal != facingDirection)
            {
                WallJump();
            }
            else if (isGrounded)
            {
                NormalJump();
            }
        }
        if (isAttemptingToJump)
        {
            jumpTimer -= Time.deltaTime;
        }
        if (wallJumpTimer > 0)
        {
            if (hasWallJumped && moveHorizontal == -lastWallJumpDirection)
            {
                rb.velocity = new Vector2(rb.velocity.x, 0f);
                hasWallJumped = false;
            }
            else if (wallJumpTimer <= 0)
            {
                hasWallJumped = false;
            }
            else
            {
                wallJumpTimer = Time.deltaTime;
            }
        }
    }

    private void NormalJump()
    {
        if (canNormalJump && !isWallSliding)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpLefts--;
            jumpTimer = 0;
            isAttemptingToJump = false;
            checkJumpMultiplier = true;
        }
    }

    private void WallJump()
    {
        //Wall Jump (Direction)
        if (canWallJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            isWallSliding = false;
            jumpLefts = jumpAmount;
            jumpLefts--;
            Vector2 forceToAdd = new Vector2(wallJumpForce * wallJumpDirection.x * -moveHorizontal, wallJumpForce * wallJumpDirection.y);
            rb.AddForce(forceToAdd, ForceMode2D.Impulse);
            jumpTimer = 0;
            isAttemptingToJump = false;
            checkJumpMultiplier = true;
            turnTimer = 0;
            canMove = true;
            canFlip = true;
            hasWallJumped = true;
            wallJumpTimer = wallJumpTimerSet;
            lastWallJumpDirection = -facingDirection;
        }
    }

    private void CheckIfWallSliding()
    {
        if (isTouchingWall && rb.velocity.y < -0.01f)//&& moveHorizontal == facingDirection)//Esto ultimo obliga a estar presionando la dirección de la pared para seguir colgado
        {
            isWallSliding = true;
            jumpLefts = jumpAmount;
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void ApplyMovement()
    {
        if (!isGrounded && !isWallSliding && moveHorizontal == 0)
        {
            rb.velocity = new Vector2(rb.velocity.x * airDragMultiplier, rb.velocity.y);
        }
        else if (canMove)
        {
            rb.velocity = new Vector2(moveHorizontal * walkSpeed, rb.velocity.y);
        }
        //Esto evita que cambie la velocidad de salto si me agacho en el aire
        if (isGrounded && crouch)
        {
            rb.velocity = new Vector2(moveHorizontal * walkSpeed * crouchSpeedMultiplier, rb.velocity.y);
        }

        if (isWallSliding)
        {
            if (rb.velocity.y < -wallSlideSpeed)
            {
                rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
            }
            if (crouch)
            {
                rb.velocity = new Vector2(-facingDirection * walkSpeed/2, -wallJumpForce/3);
            }
        }
    }

    private void Flip()
    {
        if (!isWallSliding && canFlip)
        {
            facingDirection *= -1;
            isFacingRight = !isFacingRight;
            // Multiply the player's x local scale by -1.
            Vector3 theScale = transform.localScale;
            theScale.x *= -1;
            //transform.localScale = theScale;
            transform.Rotate(0.0f, 180.0f, 0.0f);//esto ayuda al isTouchingWall, ya que transform.right tambien funcionará si está mirando a la izquierda
        }
    }

    private void CheckSorroundings()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);
        isTouchingWall = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, whatIsGround);
    }

    private void DisableColliders()
    {
        if (!isGrounded)
        {
            //legColliders.enabled = false;
            torsoCollider.enabled = true;
        }
        if (isGrounded)
        {
            legColliders.enabled = true;
            if (crouch) torsoCollider.enabled = false;
            if (!crouch) torsoCollider.enabled = true;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y, wallCheck.position.z));
    }
}