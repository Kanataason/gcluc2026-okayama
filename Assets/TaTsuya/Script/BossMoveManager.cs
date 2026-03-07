using UnityEngine;

public class BossMoveManager : CharaBase
{
    
    private void Start()
    {
        
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.A))
        {
            transform.Translate(Vector3.left * 0.5f*Time.deltaTime);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            transform.Translate(Vector3.right * 0.5f * Time.deltaTime);
        }
    }
    
}
