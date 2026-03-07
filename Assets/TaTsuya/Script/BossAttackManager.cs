using UnityEngine;

public class BossAttackManager : MonoBehaviour
{
    Animator a_Animetor;
    void Start()
    {
        a_Animetor = GetComponent<Animator>();
    }


    public void AttackEnter()
    {
        a_Animetor.SetTrigger("Attack");
        Debug.Log("‚ ‚Á‚½‚Á‚­");
    }
}
