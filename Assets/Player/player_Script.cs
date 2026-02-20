using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class player_Script : MonoBehaviour
{
    public Rigidbody2D rb;
    
    [Header("Rörelse/Movement")]
    public float movementSpeed = 5f;
    private float horizontal;
    [Header("Jump/Hoppa")]
    public float jump = 10f;
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
    private  bool isWallSliding;
    public float wallslidingSpeed = 2f;
    public Transform wallCheck;
    public float wallCheckRadius = 0.2f;
    public LayerMask wallLayer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        extraJump = extraJumpsValue;
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //Rörelse
        //Movement
        horizontal = Input.GetAxisRaw("Horizontal");

        //Double Jump
        //Dubbelhopp
        if (isGrounded)
        {
            extraJump = 1;
        }
        
        //Jump
        //Hoppa
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocityX, jump);
            }
        //Double Jump
        //Dubbelhopp
            else if (extraJump > 0)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocityX, jump);
                extraJump --;
            }

        }
        //Healthbar
        //Hälsobar
        healthBar.fillAmount = health / 100f;

        //Wallsliding
        //Väggglidning
    }
    // Use FixedUpdate for physics
    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector3(horizontal * movementSpeed, rb.linearVelocityY);

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
         
        WallSlide();
    }
    private bool IsOnGround()
    {
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }
    private bool isOnWall()
    {
        return Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, wallLayer);
    }

    private void WallSlide()
    {
        if (isOnWall() && !IsOnGround() && horizontal != 0f)
        {
            isWallSliding = true;
            rb.linearVelocity = new Vector3(rb.linearVelocityX, Mathf.Clamp(rb.linearVelocityY, -wallslidingSpeed, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }
        Debug.Log("IsWallSliding:");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Spikes collision
        if (collision.gameObject.tag == "Spikes")
        {
            health -= 50;
            rb.linearVelocity = new Vector3(rb.linearVelocityX, jump);
            if (health <= 0)
            {
                Die();
            }
        }
    }
    private void Die()
    {
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        }
    }
}
