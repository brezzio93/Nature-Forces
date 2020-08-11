using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Animator animator;
    public PlayerController controller;

    public float walkSpeed = 30f;
    public float runSpeed = 40f;
    private float actualSpeed;
    public float horizontalMove = 0f;
    private bool jump = false;
    private bool crouch = false;
    public bool isAttacking;

    private int taps = 0;

    private float buttonCooler = 1f;
    private float lastMoveTime, timeDelay, dashDuration = 0;

    private bool doubleTap;

    // Start is called before the first frame update
    private void Start()
    {
        controller = GetComponent<PlayerController>();
        actualSpeed = walkSpeed;
    }

    private void Update()
    {
        //Taunt
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("SA-X_Taunt"))
        {
            animator.SetBool("isTaunting", false);
            return;
        }
        if (Input.GetButton("Taunt"))
        {
            animator.SetBool("isTaunting", true);
        }
        else animator.SetBool("isTaunting", false);

        //Attack
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("SA-X_Attack"))
        {
            horizontalMove = 0;
            StartCoroutine("FinishAttack");
            animator.SetBool("isAttacking", controller.attacking);
            return;
        }
        if (Input.GetButtonDown("Attack"))
        {
            if (!controller.attacking)
            {
                controller.attacking = true;
                animator.SetBool("isAttacking", controller.attacking);
            }
        }
        //else animator.SetBool("isAttacking", false);

        //Hurt
        if (controller.hurt) animator.SetBool("isHurt", true);
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("SA-X_Hurt"))
        {
            animator.SetBool("isHurt", false);
            horizontalMove = 0f;
            //controller.hurt = false;//Esto se realiza en el IEnumerator GotHit
            return;
        }

        //Wall grabbing
        //animator.SetBool("isGrappling", controller.isOnWall);

        //Run/Walk
        if (controller.isGrounded())//Can control speed only if player is grounded
        {
            if (Input.GetButton("Run"))//Run with Run Button Pressed
            {
                actualSpeed = runSpeed;
                horizontalMove = Input.GetAxisRaw("Horizontal") * actualSpeed;
            }
            else //Run by double pressing "Walk"
            {
                if (Input.GetButton("Horizontal"))
                {
                    horizontalMove = Input.GetAxisRaw("Horizontal") * actualSpeed;
                }
                if (Input.GetButtonDown("Horizontal"))
                {
                    taps += 1;
                }
                if (Input.GetButtonUp("Horizontal"))
                {
                    actualSpeed = walkSpeed;
                    horizontalMove = 0;
                }

                if ((taps == 1) && (timeDelay < .5f))
                {
                    actualSpeed = walkSpeed;
                    timeDelay += Time.deltaTime;
                }
                if ((taps == 1) && (timeDelay > .5f))
                {
                    actualSpeed = walkSpeed;
                    timeDelay = 0f;
                    taps = 0;
                }
                if ((taps == 2) && (timeDelay < .5f))
                {
                    actualSpeed = runSpeed;
                    taps = 0;
                }
                if ((taps == 2) && (timeDelay > .5f))
                {
                    actualSpeed = walkSpeed;
                    taps = 0;
                    timeDelay = 0f;
                }
                /*
                //En caso de querer añadir un dash
                if (actualSpeed == runSpeed)
                {
                    dashDuration += Time.deltaTime;
                }
                if (dashDuration > 1)
                {
                    actualSpeed = walkSpeed;
                    dashDuration = 0;
                    taps = 0;
                    timeDelay = 0;
                }*/
            }
        }
        else
        {
            horizontalMove = Input.GetAxisRaw("Horizontal") * actualSpeed;
        }

        animator.SetFloat("speed", Mathf.Abs(horizontalMove));

        //Jump

        if (Input.GetButtonDown("Jump"))
        {
            jump = true;
            animator.SetBool("isJumping", jump);
            controller.HandleJumping();
        }
        if (controller.isGrounded())
        {
            jump = false;
            animator.SetBool("isJumping", jump);
        }
        //Crouch

        if (Input.GetButtonDown("Crouch"))
        {
            crouch = true;
            //animator.SetBool("isCrouching", crouch);

        }
        else if (Input.GetButtonUp("Crouch"))
        {
            crouch = false;
            //animator.SetBool("isCrouching", crouch);
        }
    }

    private void FixedUpdate()
    {
        //  controller.HandleJumping();
        controller.HandleMovement(horizontalMove * Time.fixedDeltaTime, crouch);
    }

    public void OnCrouching(bool isCrouching)
    {
        animator.SetBool("isCrouching", isCrouching);
    }

    private IEnumerator FinishAttack()
    {
        yield return new WaitForSeconds(.25f);
        controller.attacking = false;
    }
}