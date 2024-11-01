using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float MoveSpeed = 5f;

    [SerializeField] float currentSpeed; // 이동 속도 확인용 변수

    Vector2 inputVector;
    Rigidbody2D rigid;
    Animator animator;

    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        inputVector.x = Input.GetAxisRaw("Horizontal");
        inputVector.y = Input.GetAxisRaw("Vertical");

        if (inputVector.magnitude > 1)
        {
            inputVector.Normalize(); // 대각선 이동 시 속도 고정
        }

        currentSpeed = inputVector.normalized.magnitude * MoveSpeed;

        // 이동 여부에 따른 애니메이션 설정
        bool isStopped = inputVector.magnitude == 0; // 멈춤 여부 확인
        animator.SetBool("IsStopped", isStopped);

        if (!isStopped)
        {
            if (inputVector.y > 0)
            {
                animator.SetTrigger("MoveUp");
                animator.ResetTrigger("MoveDown");
                animator.ResetTrigger("HorizontalMove");
            }
            else if (inputVector.y < 0)
            {
                animator.SetTrigger("MoveDown");
                animator.ResetTrigger("MoveUp");
                animator.ResetTrigger("HorizontalMove");
            }
            else if (inputVector.x != 0)
            {
                animator.SetTrigger("HorizontalMove");
                animator.ResetTrigger("MoveUp");
                animator.ResetTrigger("MoveDown");
            }
        }
        else
        {
            animator.ResetTrigger("MoveUp");
            animator.ResetTrigger("MoveDown");
            animator.ResetTrigger("HorizontalMove");
        }


        // 좌우 반전 설정
        if (inputVector.x != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(inputVector.x), 1, 1);
        }
    }


    void FixedUpdate()
    {
        Vector2 nextVector = inputVector * MoveSpeed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVector);
    }
}
