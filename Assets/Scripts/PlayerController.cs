using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float m_JumpForce = 400f;                          // Amount of force added when the player jumps.
    [Range(0, 1)] [SerializeField] private float m_CrouchSpeed = .36f;          // Amount of maxSpeed applied to crouching movement. 1 = 100%
    [Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;  // How much to smooth out the movement
    [SerializeField] private bool m_AirControl = false;                         // Whether or not a player can steer while jumping;
    [SerializeField] private LayerMask platformLayerMask;                       // A mask determining what is ground to the character
    [SerializeField] private Transform m_CeilingCheck;                          // A position marking where to check for ceilings
    [SerializeField] private Transform m_GroundCheck;                           // A position marking where to check if the player is grounded.
    [SerializeField] private Collider2D m_CrouchDisableCollider1;                // A collider that will be disabled when crouching

    //[SerializeField] private Collider2D m_CrouchDisableCollider2;                // A collider that will be disabled when crouching
    [SerializeField] private CircleCollider2D m_CrouchDisableCollider2;                // A collider that will be disabled when crouching

    [SerializeField] private BoxCollider2D m_AirDisableCollider;                // A collider that will be disabled when crouching
    private int jumpCount = 0;
    public bool hurt;
    public bool attacking;
    public bool isOnWall;

    private const float k_GroundedRadius = .5f; // Radius of the overlap circle to determine if grounded
    private bool m_Grounded;            // Whether or not the player is grounded.
    private const float k_CeilingRadius = .1f; // Radius of the overlap circle to determine if the player can stand up
    public Rigidbody2D m_Rigidbody2D;
    public BoxCollider2D boxCollider2D;
    private bool m_FacingRight = true;  // For determining which way the player is currently facing.
    private Vector3 m_Velocity = Vector3.zero;

    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }

    public BoolEvent OnCrouchEvent;
    private bool m_wasCrouching = false;

    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();

        if (OnCrouchEvent == null)
            OnCrouchEvent = new BoolEvent();
    }

    public void HandleJumping()
    {
        if (isGrounded())
        {
            jumpCount = 0;
            m_Rigidbody2D.velocity = Vector2.up * m_JumpForce;
            jumpCount++;
        }
        if (!isGrounded() && jumpCount > 0)
        {
            m_Rigidbody2D.velocity = Vector2.up * m_JumpForce;
            jumpCount = 0;
        }

        if (!isGrounded() && isOnWall)
        {
            float jumpDir;
            if (m_FacingRight) jumpDir = -1f;
            else jumpDir = 1f;

            Vector3 targetVelocity = new Vector2(jumpDir * 20 * 10f, 7f * m_JumpForce);
            m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);

            jumpCount = 0;
        }
    }

    public void HandleMovement(float move, bool crouch)
    {
        // If crouching, check to see if the character can stand up
        if (!crouch)
        {
            // If the character has a ceiling preventing them from standing up, keep them crouching
            if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, platformLayerMask))
            {
                crouch = true;
            }
        }
        if (isGrounded())
        {
            GetComponent<PlayerMovement>().animator.SetBool("isOnWall", false);
            if (!isOnWall) m_Rigidbody2D.gravityScale = 3;

            if (m_AirDisableCollider != null)
                m_AirDisableCollider.enabled = true;
            // If crouching
            if (crouch)
            {
                if (!m_wasCrouching)
                {
                    m_wasCrouching = true;
                    OnCrouchEvent.Invoke(true);
                }

                // Reduce the speed by the crouchSpeed multiplier
                move *= m_CrouchSpeed;

                // Disable one of the colliders when crouching
                if (m_CrouchDisableCollider1 != null)
                    m_CrouchDisableCollider1.enabled = false;
                if (m_CrouchDisableCollider2 != null)
                    m_CrouchDisableCollider2.enabled = false;
            }
            else
            {
                // Enable the collider when not crouching
                if (m_CrouchDisableCollider1 != null)
                    m_CrouchDisableCollider1.enabled = true;
                if (m_CrouchDisableCollider2 != null)
                    m_CrouchDisableCollider2.enabled = true;

                if (m_wasCrouching)
                {
                    m_wasCrouching = false;
                    OnCrouchEvent.Invoke(false);
                }
            }
        }
        if (!isGrounded())
        {
            /*
            if (isLanding())
            {
                if (m_AirDisableCollider != null) m_AirDisableCollider.enabled = true;
                Debug.Log("IsLanding");
            }
            else
            {
                if (m_AirDisableCollider != null) m_AirDisableCollider.enabled = false;
                Debug.Log("IsNOTLanding");
            }*/
            if (isWall())
            {
                GetComponent<PlayerMovement>().animator.SetBool("isJumping", false);

                m_Rigidbody2D.velocity = new Vector2(0, 0);
                transform.position = new Vector3(transform.position.x, Mathf.Ceil(transform.position.y) - .5f, transform.position.z);
                m_Rigidbody2D.gravityScale = 0;

                GetComponent<PlayerMovement>().animator.SetBool("isOnWall", true);
                isOnWall = true;                
                jumpCount = 1;

                //Soltarse del muro al agacharse mientras cuelgas
                if (crouch)
                {
                    float jumpDir;
                    if (m_FacingRight) jumpDir = -1f;
                    else jumpDir = 1f;

                    Vector3 dropVelocity = new Vector2(jumpDir * 80f, -1f * m_JumpForce);
                    m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, dropVelocity, ref m_Velocity, m_MovementSmoothing);
                }
            }
            else
            {
                GetComponent<PlayerMovement>().animator.SetBool("isOnWall", false);
                m_Rigidbody2D.gravityScale = 3;
                isOnWall = false;
                m_CrouchDisableCollider1.enabled = true;
                m_CrouchDisableCollider2.enabled = true;
                if (!hurt && !attacking)
                {
                    GetComponent<PlayerMovement>().animator.SetBool("isJumping", true);
                }
                if (!hurt && attacking)
                {
                    GetComponent<PlayerMovement>().animator.SetBool("isJumping", false);
                    GetComponent<PlayerMovement>().animator.SetBool("isAttacking", true);
                }
                if (hurt)
                {
                    GetComponent<PlayerMovement>().animator.SetBool("isAttacking", false);
                    GetComponent<PlayerMovement>().animator.SetBool("isJumping", false);
                    GetComponent<PlayerMovement>().animator.SetBool("isHurt", true);
                }
            }
        }

        // Move the character by finding the target velocity
        Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
        // And then smoothing it out and applying it to the character
        m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);

        // If the input is moving the player right and the player is facing left...
        if (move > 0 && !m_FacingRight)
        {
            // ... flip the player.
            Flip();
        }
        // Otherwise if the input is moving the player left and the player is facing right...
        else if (move < 0 && m_FacingRight)
        {
            // ... flip the player.
            Flip();
        }
    }

    public bool isGrounded()
    {
        RaycastHit2D raycastHit2D = Physics2D.BoxCast(boxCollider2D.bounds.center, boxCollider2D.bounds.size, 0f, Vector2.down, 0.01f, platformLayerMask);
        //Debug.Log(raycastHit2D.collider);
        return raycastHit2D.collider != null;
    }

    public bool isLanding()
    {
        RaycastHit2D raycastHit2D = Physics2D.BoxCast(m_AirDisableCollider.bounds.center, m_AirDisableCollider.bounds.size, 0f, Vector2.down, 0.01f, platformLayerMask);
        Debug.Log(raycastHit2D.collider);
        return raycastHit2D.collider != null;
    }

    public bool isWall()
    {
        RaycastHit2D raycastHit2D;
        if (m_FacingRight)
            raycastHit2D = Physics2D.BoxCast(m_CrouchDisableCollider2.bounds.center, m_CrouchDisableCollider2.bounds.size, 0f, Vector2.right, 0.1f, platformLayerMask);
        else
            raycastHit2D = Physics2D.BoxCast(m_CrouchDisableCollider2.bounds.center, m_CrouchDisableCollider2.bounds.size, 0f, Vector2.left, 0.1f, platformLayerMask);
        return raycastHit2D.collider != null;
    }

    public bool isCeiling()
    {
        RaycastHit2D raycastHit2D = Physics2D.BoxCast(m_CrouchDisableCollider1.bounds.center, m_CrouchDisableCollider1.bounds.size, 0f, Vector2.right, 0.1f, platformLayerMask);
        return raycastHit2D.collider != null;
    }

    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        m_FacingRight = !m_FacingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        m_AirDisableCollider.enabled = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!isGrounded())
            m_AirDisableCollider.enabled = false;
    }

    /*
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!isGrounded() && isWall())
        {
            m_Rigidbody2D.velocity = new Vector2(0, 0);
            transform.position = new Vector3(transform.position.x, Mathf.Floor(transform.position.y)+.5f, transform.position.z);
            m_Rigidbody2D.gravityScale = 0;
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (!isGrounded() && !isWall())
        {
            m_Rigidbody2D.gravityScale = 3;
        }
    }
    */

    public IEnumerator GotHit(float knockDur, float knockPwr, Vector2 knockDir, float enemyX)
    {
        float launchDir = 0;
        float launchMagnitude = 80f;
        if (enemyX > knockDir.x)
        {
            launchDir = -launchMagnitude;
        }
        else if (enemyX < knockDir.x)
        {
            launchDir = launchMagnitude;
        }
        //Debug.Log("I'm hit!!!");
        hurt = true;
        float timer = 0;
        m_Rigidbody2D.velocity = m_Velocity;

        while (knockDur > timer)
        {
            timer += Time.deltaTime;
            m_Rigidbody2D.AddForce(new Vector3(knockDir.x + launchDir, knockPwr, transform.position.z));
        }
        yield return new WaitForSeconds(0.5f);
        hurt = false;
    }
}