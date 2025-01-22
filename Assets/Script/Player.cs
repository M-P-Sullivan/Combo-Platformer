using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Horizontal Movement")]
    public float moveSpeed = 10f;
    public Vector2 direction;
    private bool facingRight = true;

    [Header("Vertical Movement")]
    public float jumpSpeed = 15f;
    public float wallJumpVerticalSpeed = 10f;
    public float wallJumpHorizontalSpeed = 5f;
    public float jumpDelay = 0.25f;
    private float jumpTimer;

    [Header("Components")]
    public Rigidbody2D rb;
    public List<string> listOfGroundLayers = new List<string>();
    public List<string> listOfPassableGroundLayers = new List<string>();
    public LayerMask groundLayer;
    //private LayerMask maskOfGroundLayers;
    //public Animator animator;
    //public GameObject characterHolder;

    [Header("Physics")]
    public float maxSpeed = 7f;
    public float linearDrag = 4f;
    public float gravity = 1f;
    public float fallMultiplier = 5f;

    [Header("Collision")]
    public bool onGround = false;
    public bool onLeftWall = false;
    public bool onRightWall = false;
    public float groundLength = 0.22f;
    public float sideLength = 0.22f;
    public Vector3 colliderOffset;

    [Header("Character Swap")]
    public bool isActiveCharacter = false;
    public string characterType = "stone";

    void Awake()
    {

    }

    void Start()
    {
        applySettings();
/*        string[] arrayOfGroundLayers = listOfGroundLayers.ToArray();
        string[] arrayOfPassableGroundLayers = listOfPassableGroundLayers.ToArray();
        Debug.Log(1 << 3);
        Debug.Log(LayerMask.GetMask(arrayOfGroundLayers));
        Debug.Log(LayerMask.GetMask(arrayOfPassableGroundLayers));
        Debug.Log(LayerMask.GetMask(arrayOfGroundLayers) & LayerMask.GetMask(arrayOfPassableGroundLayers));
*/    }

    // Update is called once per frame
    void Update()
    {
        bool wasOnGround = onGround;
        onGround = checkGround();
        onLeftWall = checkLeftWall();
        onRightWall = checkRightWall();

        if (Input.GetButtonDown("Swap"))
        {
            switchActiveCharacter();
        }

        if (!isActiveCharacter)
        {
            return;
        }

        if (!wasOnGround && onGround)
        {
            //StartCoroutine(JumpSqueeze(1.25f, 0.8f, 0.05f));
        }

        if (Input.GetButtonDown("Jump"))
        {
            jumpTimer = Time.time + jumpDelay;
        }
        //animator.SetBool("onGround", onGround);
        direction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }
    void FixedUpdate()
    {
        if (!isActiveCharacter)
        {
            modifyInactivePhysics();
            return;
        }
        moveCharacter(direction.x);
        if (jumpTimer > Time.time)
        {
            if (onGround)
            {
                Jump();
            }
            else if (onLeftWall)
            {
                wallJump("left");
            }
            else if (onRightWall)
            {
                wallJump("right");
            }
        }

        modifyPhysics();
    }
    void moveCharacter(float horizontal)
    {
        rb.AddForce(Vector2.right * horizontal * moveSpeed);

        if ((horizontal > 0 && !facingRight) || (horizontal < 0 && facingRight))
        {
            Flip();
        }
        if (Mathf.Abs(rb.velocity.x) > maxSpeed)
        {
            rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * maxSpeed, rb.velocity.y);
        }
        //animator.SetFloat("horizontal", Mathf.Abs(rb.velocity.x));
        //animator.SetFloat("vertical", rb.velocity.y);
    }
    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.AddForce(Vector2.up * jumpSpeed, ForceMode2D.Impulse);
        jumpTimer = 0;
        //StartCoroutine(JumpSqueeze(0.5f, 1.2f, 0.1f));
    }
    void wallJump(string wallSide)
    {
        rb.velocity = new Vector2(0, 0);
        if (wallSide == "left")
        {
            rb.AddForce(new Vector2(wallJumpHorizontalSpeed, wallJumpVerticalSpeed), ForceMode2D.Impulse);
        }
        else if (wallSide == "right")
        {
            rb.AddForce(new Vector2(-wallJumpHorizontalSpeed, wallJumpVerticalSpeed), ForceMode2D.Impulse);
        }
        jumpTimer = 0;
    }
    void modifyPhysics()
    {
        bool changingDirections = (direction.x > 0 && rb.velocity.x < 0) || (direction.x < 0 && rb.velocity.x > 0);

        if (onGround)
        {
            if (Mathf.Abs(direction.x) < 0.4f || changingDirections)
            {
                rb.drag = linearDrag;
            }
            else
            {
                rb.drag = 0f;
            }
            rb.gravityScale = 0;
        }
        else
        {
            rb.gravityScale = gravity;
            rb.drag = linearDrag * 0.15f;
            if (rb.velocity.y < 0)
            {
                rb.gravityScale = gravity * fallMultiplier;
            }
            else if (rb.velocity.y > 0 && !Input.GetButton("Jump"))
            {
                rb.gravityScale = gravity * (fallMultiplier / 2);
            }
        }
    }
    void Flip()
    {
        facingRight = !facingRight;
        transform.rotation = Quaternion.Euler(0, facingRight ? 0 : 180, 0);
    }
    //IEnumerator JumpSqueeze(float xSqueeze, float ySqueeze, float seconds)
    //{
    //    Vector3 originalSize = Vector3.one;
    //    Vector3 newSize = new Vector3(xSqueeze, ySqueeze, originalSize.z);
    //    float t = 0f;
    //    while (t <= 1.0)
    //    {
    //        t += Time.deltaTime / seconds;
    //        characterHolder.transform.localScale = Vector3.Lerp(originalSize, newSize, t);
    //        yield return null;
    //    }
    //    t = 0f;
    //    while (t <= 1.0)
    //    {
    //        t += Time.deltaTime / seconds;
    //        characterHolder.transform.localScale = Vector3.Lerp(newSize, originalSize, t);
    //        yield return null;
    //    }
    //}
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 xOffset = new Vector3(colliderOffset.x, 0, 0);
        Vector3 yOffset = new Vector3(0, colliderOffset.y, 0);
        Gizmos.DrawLine(transform.position + xOffset, transform.position + xOffset+ Vector3.down * groundLength);
        Gizmos.DrawLine(transform.position - xOffset, transform.position - xOffset + Vector3.down * groundLength);
        Gizmos.DrawLine(transform.position + yOffset, transform.position + yOffset + Vector3.left * sideLength);
        Gizmos.DrawLine(transform.position - yOffset, transform.position - yOffset + Vector3.left * sideLength);
        Gizmos.DrawLine(transform.position + yOffset, transform.position + yOffset + Vector3.right * sideLength);
        Gizmos.DrawLine(transform.position - yOffset, transform.position - yOffset + Vector3.right * sideLength);
    }
    private void switchActiveCharacter()
    {
        isActiveCharacter = !isActiveCharacter;
        applySettings();
    }
    private void applySettings()
    {
        if (characterType == "stone")
        {
            if (!isActiveCharacter)
            {
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
                gameObject.layer = LayerMask.NameToLayer("InactiveStone");
            }
            else
            {
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                gameObject.layer = LayerMask.NameToLayer("Stone");
            }
        }
        if (characterType == "air")
        {
            if (!isActiveCharacter)
            {
                gameObject.layer = LayerMask.NameToLayer("InactiveAir");
            }
            else
            {
                gameObject.layer = LayerMask.NameToLayer("Air");
            }
        }
        if (!isActiveCharacter)
        {
            rb.gravityScale = gravity;
        }
    }
    private void modifyInactivePhysics()
    {
        if (characterType == "air")
        {
            if (rb.velocity.y < 0)
            {
                rb.gravityScale = gravity * 0.5f;
            }

            if (onGround)
            {
                rb.drag = linearDrag;
            }
        }

        if (characterType == "air" && rb.velocity.y < 0 && !onGround)
        {
            rb.gravityScale = gravity * 0.5f;
        }
    }
    private bool checkGround()
    {
        Vector3 xOffset = new Vector3(colliderOffset.x, 0, 0);
        LayerMask maskOfGroundLayers = checkSurfaceHelper();
        return (Physics2D.Raycast(transform.position + xOffset, Vector2.down, groundLength, maskOfGroundLayers) || Physics2D.Raycast(transform.position - xOffset, Vector2.down, groundLength, maskOfGroundLayers));
    }
    private bool checkLeftWall()
    {
        Vector3 yOffset = new Vector3(0,colliderOffset.y, 0);
        LayerMask maskOfGroundLayers = checkSurfaceHelper();
        return (Physics2D.Raycast(transform.position + yOffset, Vector2.left, groundLength, maskOfGroundLayers) || Physics2D.Raycast(transform.position - yOffset, Vector2.left, groundLength, maskOfGroundLayers));
    }
    private bool checkRightWall()
    {
        Vector3 yOffset = new Vector3(0, colliderOffset.y, 0);
        LayerMask maskOfGroundLayers = checkSurfaceHelper();
        return (Physics2D.Raycast(transform.position + yOffset, Vector2.right, groundLength, maskOfGroundLayers) || Physics2D.Raycast(transform.position - yOffset, Vector2.right, groundLength, maskOfGroundLayers));
    }
    private LayerMask checkSurfaceHelper()
    {
        string[] arrayOfGroundLayers = listOfGroundLayers.ToArray();
        string[] arrayOfPassableGroundLayers = listOfPassableGroundLayers.ToArray();
        LayerMask newMask;
        if (!isActiveCharacter && characterType == "air")
        {
            newMask = LayerMask.GetMask(arrayOfGroundLayers) & ~LayerMask.GetMask(arrayOfPassableGroundLayers);
        }
        else
        {
            newMask = LayerMask.GetMask(arrayOfGroundLayers);
        }
        return newMask;
    }
}