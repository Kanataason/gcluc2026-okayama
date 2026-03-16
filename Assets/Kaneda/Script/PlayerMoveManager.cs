using UnityEngine;

public class PlayerMoveManager : CharaBase
{
    PlayerInputManager c_PlayerInput;

    const float M_Speed = 8f;

    const float JUMP_POWER = 15f;
    const float GRAVITY = -25f;

    float jumpVelocity = 0f;

    bool isGround = true;
    bool isJumping = false;

    float groundY;

    const float g_Min = -6f;
    const float g_Max = -4f;

    public override void Start()
    {
        base.Start();

        c_PlayerInput = GetComponent<PlayerInputManager>();

        groundY = transform.position.y;
    }

    public virtual void MoveUpdate()
    {
        Move();
        Jump();
        UpdateAnimation();
    }

    void Move()
    {
        CheckGround(g_Min, g_Max);

        if (GetIsAttackFlag()) return;

        float h = c_PlayerInput.GetHorizontal();
        float v = c_PlayerInput.GetVertical();

        Vector3 move = new Vector3(h, v, 0);

        transform.Translate(move * M_Speed * Time.deltaTime);

        Vector3 scale = transform.localScale;

        if (h < 0)
            scale.x = -Mathf.Abs(scale.x);

        if (h > 0)
            scale.x = Mathf.Abs(scale.x);

        transform.localScale = scale;
    }

    float jumpStartY;

    void Jump()
    {
        //ジャンプ開始
        if (c_PlayerInput.GetJump() && isGround)
        {
            isGround = false;
            isJumping = true;

            jumpVelocity = JUMP_POWER;

            //ジャンプ開始位置を保存
            jumpStartY = transform.position.y;

            a_Animator.SetTrigger("Jump");
        }

        //空中処理
        if (isJumping)
        {
            jumpVelocity += GRAVITY * Time.deltaTime;

            Vector3 pos = transform.position;
            pos.y += jumpVelocity * Time.deltaTime;

            transform.position = pos;

            //着地
            if (pos.y <= jumpStartY)
            {
                pos.y = jumpStartY;
                transform.position = pos;

                isGround = true;
                isJumping = false;
                jumpVelocity = 0;
            }
        }
    }

    void UpdateAnimation()
    {
        float h = Mathf.Abs(c_PlayerInput.GetHorizontal());
        float v = Mathf.Abs(c_PlayerInput.GetVertical());

        float speed = h + v;

        a_Animator.SetFloat("MoveSpeed", speed);
        a_Animator.SetBool("Ground", isGround);
    }

    public bool IsGround()
    {
        return isGround;
    }
}