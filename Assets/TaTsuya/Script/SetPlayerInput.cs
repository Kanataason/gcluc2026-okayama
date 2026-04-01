using UnityEngine;
using UnityEngine.InputSystem;

public class SetPlayerInput : MonoBehaviour
{

    private void Start()
    {
        BattleManager.Instance.OnCreateCharacters += OnInit;
    }
    private void OnDisable()
    {
        BattleManager.Instance.OnCreateCharacters -= OnInit;
    }
    private void OnInit(GameObject boss,InputInfo inputInfo)
    {
        SetCharacter(boss);
        SetCharacter(inputInfo.TargetObj);
    }
    private InputInfo CreateInputInfo(PlayerInput input)
    {
        var inputs = new InputInfo();
        inputs.ControllerDevice = input.GetDevice<InputDevice>();
        inputs.ControllerScheme = input.defaultControlScheme;
        return inputs;

    }
    private void SetCharacter(GameObject character)
    {
        var input = character.GetComponent<PlayerInput>();
        var strInput = CreateInputInfo(input);

        input.defaultControlScheme = strInput.ControllerScheme;
        input.SwitchCurrentControlScheme(strInput.ControllerDevice);
    }


}
 public struct InputInfo
{
    public int PlayerNum;
    public string ControllerScheme;
    public InputDevice ControllerDevice;
    public GameObject TargetObj;
}