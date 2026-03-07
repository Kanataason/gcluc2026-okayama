using UnityEngine;

public class BossMoveManager : CharaBase
{
    
    private void Start()
    {
        a_Animator = GetComponent<Animator>();
        m_hp = 100;
    }
    private void Update()
    {
        if(Input.GetKey(KeyCode.A))
        {
            a_Animator.SetFloat("Move", 3f * Time.deltaTime, 0.05f, Time.deltaTime);
            transform.Translate(Vector3.left * 3f*Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            a_Animator.SetFloat("Move",0, 0.05f, Time.deltaTime);
            transform.Translate(Vector3.right * 4f * Time.deltaTime);
        }
        if (Input.GetKeyDown(KeyCode.H)) { TakeDamage(5); }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            c_SaveState.m_AnimeStateValue = a_Animator.GetFloat("Move");
            c_SaveState.m_AnimeTime = a_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            c_SaveState.m_AnimeHash = a_Animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
            c_SaveState.m_Inihp = m_hp;
            c_SaveState.v_IniPosition = transform.position;
            c_SaveState.q_IniRotate = transform.rotation;
            Debug.Log($"hp{c_SaveState.m_Inihp}/pos{c_SaveState.v_IniPosition}/rotete" +
                $"{c_SaveState.q_IniRotate}/animetime{c_SaveState.m_AnimeTime}");
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            a_Animator.SetFloat("Move", c_SaveState.m_AnimeStateValue);
            a_Animator.Play(c_SaveState.m_AnimeHash, 0, c_SaveState.m_AnimeTime);
            m_hp = c_SaveState.m_Inihp;
            transform.position = c_SaveState.v_IniPosition;
            transform.rotation = c_SaveState.q_IniRotate;
            Debug.Log($"hp{m_hp}/pos{transform.position}/rotete{transform.rotation}");
        }
    }
    
}
