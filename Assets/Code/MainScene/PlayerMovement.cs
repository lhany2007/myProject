using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float MoveSpeed = 5f;

    [SerializeField] float currentSpeed; // �̵� �ӵ� Ȯ�ο� ����

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
            inputVector.Normalize(); // �밢�� �̵� �� �ӵ� ����
        }

        currentSpeed = inputVector.normalized.magnitude * MoveSpeed;

        // �̵� ���ο� ���� �ִϸ��̼� ����
        bool isStopped = inputVector.magnitude == 0; // ���� ���� Ȯ��
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


        // �¿� ���� ����
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
