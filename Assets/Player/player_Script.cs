//using NUnit.Framework;
//using TMPro;
//using Unity.VisualScripting;
//using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
//using UnityEngine.XR;
public class player_Script : MonoBehaviour
{
    public Rigidbody2D rb;
    private Animator anim;
    [Header("Rörelse/Movement")]
    public float movementSpeed = 5f;
    private bool isfacingRight = true;
    private float horizontal;
    [Header("Jump/Hoppa")]
    public float jumpPower = 10f;
    private int extraJump;
    public int extraJumpsValue = 1;
    [Header("Ground check/Markkontroll")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    private bool isGrounded;
    [Header("Players health/Players hälsa")]
    public int health = 100;
    public Image healthBar;
    [Header("Wallsliding check/Väggglidningskontroll")]
    public float wallSlidingSpeed = 2f;
    public float wallSlidingSpeedHolding = 1f; // Slower fall speed when holding toward the wall
    private bool isWallSliding;
    public Transform wallCheck;
    public float wallCheckRadius = 0.2f;
    public LayerMask wallLayer;
    [Header("Walljump/Vägg hopp")]
    public float wallJumpingTime = 0.2f;
    private float wallJumpingCounter;
    public float wallJumpingDuration = 0.2f;
    public Vector3 wallJumpingPower = new Vector3(10f, 10f);
    private bool isWallJumping;
    private float wallJumpDirection;
    [Header("Dash")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    private bool isDashing;
    private bool canDash = true;
    public float dashCooldown = 1f;
    [Header("Attack")]
    public float timeBtwAttack;
    public float startTimeBtwAttack;
    public Transform attackPos;
    public float attackRange;
    public LayerMask whatIsEnemies;
    public int damage;
    public bool isAttacking;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        extraJump = extraJumpsValue;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        //Healthbar
        //Hälsobar
        healthBar.fillAmount = health / 100f;

        HandleAttack();
        HandleMovement();
        HandleJump();
        flip();
        WallJump();
        Dash();
        HandleDashAttack();
    }
    
    // Use FixedUpdate for physics
    private void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // As soon as the player lands, cancel any pending wall jump state immediately.
        if (isGrounded)
        {
            CancelInvoke(nameof(stopWallJumping));
            isWallJumping = false;
        }

        anim.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocityX));
        anim.SetFloat("yVelocity", rb.linearVelocityY);

        // Drive the jump animation purely from grounded state — no manual bool flipping needed.
        anim.SetBool("isJumping", !isGrounded && !isWallSliding);

        WallSlide();
        IsOnGround();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Spikes collision
        if (collision.gameObject.tag == "Spikes")
        {
            health -= 50;
            StartCoroutine(HurtAnimation());

            rb.linearVelocity = new Vector3(rb.linearVelocityX, jumpPower);

            if (health <= 0)
            {
                Die();
            }
        }
    }

    public void TakeDamage (int damage)
    {
        health -= damage;
        StartCoroutine(HurtAnimation());
        rb.linearVelocity = new Vector3(rb.linearVelocityX, jumpPower);

        if (health <= 0)
        {
            Die();
        }
    }

    private IEnumerator HurtAnimation()
    {
        anim.SetBool("gotHurt", true);
        yield return new WaitForSeconds(0.2f);
        anim.SetBool("gotHurt", false);
    }

    private bool IsOnGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        //Debug.Log("IsGrounded: " + isGrounded);
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }
    private bool isOnWall()
    {
        return Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, wallLayer);
    }

    //Rörelse
    //Movement
    private void HandleMovement()
    {
        if(!isDashing)
        {
            rb.linearVelocity = new Vector3(horizontal * movementSpeed, rb.linearVelocityY);
        }
        horizontal = Input.GetAxisRaw("Horizontal");
    }

    private void HandleJump()
    {
        // Don't process normal jumps while a wall jump is controlling the player.
        if (isWallJumping) return;

        //Jump / Hoppa
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocityX, jumpPower);
            }
            //Double Jump / Dubbelhopp
            else if (extraJump > 0 && !isWallSliding)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocityX, jumpPower);
                extraJump--;
            }
        }

        //Double Jump reset / Dubbelhopp återställning
        if (isGrounded)
        {
            extraJump = extraJumpsValue;
        }
    }

    private void Dash()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash && !isDashing)
        {
            StartCoroutine(DashCoroutine());
        }
    }

    private IEnumerator DashCoroutine()
    {
        canDash = false;
        isDashing = true;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        float direction = isfacingRight ? 1f : -1f;

        rb.linearVelocity = new Vector2(direction * dashSpeed, 0f);
        anim.SetBool("isDashing", true);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        isDashing = false;
        anim.SetBool("isDashing", false);

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void HandleDashAttack()
    {
        if (Input.GetMouseButtonDown(0) && isDashing)
        {
            anim.SetBool("isDashingAttacking", true);
            anim.SetTrigger("dashAttack");
            /*Collider2D[] enemysToDamage = Physics2D.OverlapCircleAll(attackPos.position, attackRange, whatIsEnemies);
            for (int i = 0; i < enemysToDamage.Length; i++)
            {
                enemysToDamage[i].GetComponent<enemy_1_script>().TakeDamage(damage);
            }*/
        }
        else
        {
            anim.SetBool("isDashingAttacking", false);
        }
    }

    private void WallSlide()
    {
        if (isOnWall() && !isGrounded)
        {
            isWallSliding = true;
            anim.SetBool("isWallSliding", true);

            if (rb.linearVelocityY <= 0)
            {
                // Determine whether the player is pressing toward the wall they are on.
                // The wall is in the direction the player is facing (localScale.x > 0 = right).
                bool holdingTowardWall = (isfacingRight && horizontal > 0f) || (!isfacingRight && horizontal < 0f);
                float targetSpeed = holdingTowardWall ? wallSlidingSpeedHolding : wallSlidingSpeed;
                rb.linearVelocity = new Vector2(rb.linearVelocityX, -targetSpeed);
            }
        }
        else
        {
            isWallSliding = false;
            anim.SetBool("isWallSliding", false);
        }
    }

    private void WallJump()
    {
        if (isWallSliding)
        {
            // Store the jump-off direction and reset the grace window.
            wallJumpDirection = -transform.localScale.x;
            wallJumpingCounter = wallJumpingTime;
            CancelInvoke(nameof(stopWallJumping));
        }
        else
        {
            wallJumpingCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump") && wallJumpingCounter > 0f)
        {
            isWallJumping = true;
            rb.linearVelocity = new Vector2(wallJumpDirection * wallJumpingPower.x, wallJumpingPower.y);
            wallJumpingCounter = 0f;
            extraJump = extraJumpsValue; // Restore double jump after wall jump

            // Flip player to face the direction they jumped toward.
            if (transform.localScale.x != wallJumpDirection)
            {
                isfacingRight = !isfacingRight;
                Vector3 localScale = transform.localScale;
                localScale.x *= -1f;
                transform.localScale = localScale;
            }

            // Cancel any previous pending stop before scheduling a new one.
            CancelInvoke(nameof(stopWallJumping));
            Invoke(nameof(stopWallJumping), wallJumpingDuration);
        }
    }

    private void HandleAttack()
    {
        isAttacking = false;
        
        if (timeBtwAttack <= 0 && !isDashing)
        {
            if (Input.GetMouseButtonDown(0) && isGrounded)
            {
                isAttacking = true;
                Debug.Log("Player Attacked!");
                anim.SetTrigger("attack");
                /*Collider2D[] enemysToDamage = Physics2D.OverlapCircleAll(attackPos.position, attackRange, whatIsEnemies);
                for (int i = 0; i < enemysToDamage.Length; i++)
                {
                    enemysToDamage[i].GetComponent<enemy_1_script>().TakeDamage(damage);
                }*/
                timeBtwAttack = startTimeBtwAttack;
            }
        }
        else
        {
            timeBtwAttack -= Time.deltaTime;
        }
    }

    private void stopWallJumping()
    {
        isWallJumping = false;
    }

    private void flip()
    {
        // Don't flip while a wall jump is controlling the player's direction.
        if (isWallJumping) return;

        if (isfacingRight && horizontal < 0f || !isfacingRight && horizontal > 0f)
        {
            isfacingRight = !isfacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    private void Die()
    {
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        Gizmos.DrawWireSphere(wallCheck.position, wallCheckRadius);
    }

        void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPos.position, attackRange);
    }
    
}
