using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float MoveSpeed = 5f;
    [SerializeField] float currentSpeed;

    private Vector2 inputVector;
    private Vector2 lastNonZeroInput; // Store last movement direction
    private Rigidbody2D rigid;
    private Animator animator;

    // Animation states
    private readonly string IS_MOVING = "IsMoving";
    private readonly string IS_STANDING = "IsStanding";
    private readonly string HORIZONTAL_MOVEMENT = "PlayerHorizontalMovement";
    private readonly string IS_VERTICAL = "IsVerticalMovement";
    private readonly string LAST_VERTICAL = "LastVertical";

    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Get input
        inputVector.x = Input.GetAxisRaw("Horizontal");
        inputVector.y = Input.GetAxisRaw("Vertical");

        // Store last non-zero input for standing state
        if (inputVector.magnitude > 0)
        {
            lastNonZeroInput = inputVector;
        }

        // Normalize diagonal movement
        if (inputVector.magnitude > 1)
        {
            inputVector.Normalize();
        }

        currentSpeed = inputVector.normalized.magnitude * MoveSpeed;

        // Handle sprite flipping
        if (inputVector.x != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(inputVector.x), 1, 1);
        }

        // Update animations
        UpdateAnimationStates();
    }

    void FixedUpdate()
    {
        Vector2 nextVector = inputVector * MoveSpeed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVector);
    }

    private void UpdateAnimationStates()
    {
        bool isMoving = inputVector.magnitude > 0;
        animator.SetBool(IS_MOVING, isMoving);
        animator.SetBool(IS_STANDING, !isMoving);

        if (isMoving)
        {
            bool isHorizontal = Mathf.Abs(inputVector.x) > 0;
            bool isVertical = Mathf.Abs(inputVector.y) > 0;

            animator.SetFloat(HORIZONTAL_MOVEMENT, Mathf.Abs(inputVector.x));
            animator.SetBool(IS_VERTICAL, isVertical);

            // Store last vertical direction for stand state
            if (isVertical)
            {
                animator.SetFloat(LAST_VERTICAL, inputVector.y);
            }

            // Reset all triggers first
            animator.ResetTrigger("PlayerUp");
            animator.ResetTrigger("PlayerDown");
            animator.ResetTrigger("UnderDiagonalUp");
            animator.ResetTrigger("UnderDiagonalDown");

            // Set appropriate trigger based on movement direction
            if (isHorizontal && isVertical)
            {
                if (inputVector.y > 0)
                {
                    animator.SetTrigger("UnderDiagonalUp");
                }
                else
                {
                    animator.SetTrigger("UnderDiagonalDown");
                }
            }
            else if (isVertical)
            {
                if (inputVector.y > 0)
                {
                    animator.SetTrigger("PlayerUp");
                }
                else
                {
                    animator.SetTrigger("PlayerDown");
                }
            }
        }
    }
}