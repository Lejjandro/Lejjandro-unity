using UnityEngine;

public class player_Script : MonoBehaviour
{
    public Rigidbody2D rb;
    [Header("Rörelse/Movement")]
    public float movementSpeed = 5f;
    private Vector3 move = new Vector3(0,0,0);
    [Header("Jump/Hoppa")]
    public float jump;
    private int extraJump;
    public int extraJumpsValue = 1;
    [Header("Ground check/Markkontroll")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    private bool isGrounded;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        extraJump = extraJumpsValue;
    }

    // Update is called once per frame
    void Update()
    {
        //Rörelse
        //Movement
        float input = Input.GetAxis("Horizontal");
        move.x = input * movementSpeed * Time.deltaTime;
        transform.Translate(move);

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
                rb.AddForce(new Vector3(rb.linearVelocityX, jump));
            }
        //Double Jump
        //Dubbelhopp
            else if (extraJump > 0)
            {
                rb.AddForce(new Vector3(rb.linearVelocityX, jump));
                extraJump --;
            }

        }
    }
    private void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }
}
