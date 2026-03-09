using UnityEngine;

public class PlayerInputManager : MonoBehaviour
{
    float h_Input;
    float v_Input;

    void Update()
    {
        h_Input = Input.GetAxisRaw("Horizontal");
        v_Input = Input.GetAxisRaw("Vertical");
    }

    public float GetHorizontal()
    {
        return h_Input;
    }

    public float GetVertical()
    {
        return v_Input;
    }
}