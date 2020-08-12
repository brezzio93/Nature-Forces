using UnityEngine;

public class PlayerControllerNew : MonoBehaviour
{
    private float moveHorizontal;
    private bool isFacingRight = true;
    public bool crouch;
    public bool isGrounded;
    public bool isTouchingWall;
    public bool isWallSliding;
    private bool canJump;
    public int jumpAmount;
    public int jumpLefts;

    public float walkSpeed;
    public float jumpForce = 16f;
    public float groundCheckRadius;
    public float wallCheckDistance;
    public float wallSlideSpeed;
    public float movementForceInAir;
    public float airDragMultiplier = 1f;
    public float variableJumpHeightMultiplier = .5f;
    public float crouchSpeedMultiplier = 0f;

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
        UpdateAnimation();
        CheckInput();
        CheckMovementDirection();
        CheckIfCanJump();
        CheckIfWallSliding();
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
        {
            anim.SetBool("isOnWall", isTouchingWall);
            anim.SetBool("isJumping", false);
        }
        else
        {
            anim.SetBool("isOnWall", false);
            anim.SetFloat("speed", Mathf.Abs(moveHorizontal * walkSpeed));
            anim.SetBool("isJumping", !isGrounded);
            anim.SetBool("isCrouching", crouch);

            if (Input.GetButton("Taunt"))
                anim.SetBool("isTaunting", true);
            else anim.SetBool("isTaunting", false);
        }
    }

    private void CheckInput()
    {
        moveHorizontal = Input.GetAxisRaw("Horizontal");
        if (Input.GetButtonDown("Jump"))
        {
            Jump();
        }
        if (Input.GetButtonUp("Jump"))
        {
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
        //if (isGrounded && rb.velocity.y <= 0)
        //{
        //    canJump = true;
        //}
        //else canJump = false;

        if (isGrounded && rb.velocity.y <= 0)
        {
            jumpLefts = jumpAmount;
        }
        if (jumpLefts <= 0)
        {
            canJump = false;
        }
        else canJump = true;
    }

    private void Jump()
    {
        //Original
        if (canJump)
        {
            if (!isWallSliding)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            }
            if (isWallSliding)
            {
                float jumpDir;
                if (isFacingRight) jumpDir = -1f;
                else jumpDir = 1f;

                Vector2 targetVelocity = new Vector2(jumpDir * 10f,jumpForce);
                rb.velocity = targetVelocity;
            }
            jumpLefts--;
        }

        //Esta versión permite doble salto
        //if (isGrounded)
        //{
        //    jumpCount = 0;
        //    rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        //    jumpCount++;
        //}
        //if (!isGrounded && jumpCount > 0)
        //{
        //    rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        //    jumpCount = 0;
        //}
    }

    private void CheckIfWallSliding()
    {
        if (isTouchingWall && !isGrounded && rb.velocity.y < 0)
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
        if (isGrounded)
        {
            rb.velocity = new Vector2(moveHorizontal * walkSpeed, rb.velocity.y);
            if (crouch)
            {
                rb.velocity = new Vector2(moveHorizontal * walkSpeed * crouchSpeedMultiplier, rb.velocity.y);
            }
        }
        else if (!isGrounded && !isWallSliding && moveHorizontal != 0)
        {
            Vector2 forceToAdd = new Vector2(movementForceInAir * moveHorizontal, 0);
            rb.AddForce(forceToAdd);

            if (Mathf.Abs(rb.velocity.x) > walkSpeed)
            {
                rb.velocity = new Vector2(walkSpeed * moveHorizontal, rb.velocity.y);
            }
        }
        else if (!isGrounded && !isWallSliding && moveHorizontal == 0)
        {
            rb.velocity = new Vector2(rb.velocity.x * airDragMultiplier, rb.velocity.y);
        }

        if (isWallSliding)
        {
            //Soltarse del muro al agacharse mientras cuelgas
            if (crouch)
            {
                float jumpDir;
                if (isFacingRight) jumpDir = -1f;
                else jumpDir = 1f;

                Vector2 targetVelocity = new Vector2(jumpDir * 5f, -jumpForce);
                rb.velocity = targetVelocity;
            }
            if (rb.velocity.y < -wallSlideSpeed)
            {
                rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
            }
        }
    }

    private void Flip()
    {
        if (!isWallSliding)
        {
            isFacingRight = !isFacingRight;
            // Multiply the player's x local scale by -1.
            Vector3 theScale = transform.localScale;
            theScale.x *= -1;
            //transform.localScale = theScale;
            transform.Rotate(0.0f, 180.0f, 0.0f);
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