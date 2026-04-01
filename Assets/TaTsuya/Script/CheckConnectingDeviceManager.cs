using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;

public class CheckConnectingDeviceManager : MonoBehaviour
{
    public static CheckConnectingDeviceManager Instance { get; private set; }

    private List<InputDevice> l_DeviceList = new List<InputDevice>();
    private void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
    }
    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        CheckConnectDevice();
    }
    public void CheckConnectDevice()
    {
        l_DeviceList.Clear();

        // Gamepadを優先で追加
        for (int i = 0; i < Gamepad.all.Count; i++)
        {
            if (l_DeviceList.Count >= 2) break;

            AddDeviceList(Gamepad.all[i]);
        }

        // Gamepadが1つ以下ならキーボード追加
        if (l_DeviceList.Count < 2 && Keyboard.current != null)
        {
            AddDeviceList(Keyboard.current);
        }
        Debug.Log($"listdevice{l_DeviceList.Count}");
        foreach (var devi in l_DeviceList)
        {
            Debug.Log(devi);
        }
    }
    public List<InputDevice> GetDeviceList()
    {
        return l_DeviceList;
    }
    private void AddDeviceList(InputDevice device)
    {
        l_DeviceList.Add(device);
    }
    private void RemoveDevice(InputDevice device)
    {
        l_DeviceList.Remove(device);
    }
    void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (change == InputDeviceChange.Added)
        {
            // コントローラーが追加されたとき
            if (device is Gamepad)
            {
                Debug.Log("コントローラー接続された: " + device.displayName);
                
            }
            CheckConnectDevice();
        }
        else if (change == InputDeviceChange.Removed)
        {
            if (device is Gamepad)
            {
                Debug.Log("コントローラー切断された: " + device.displayName);
                RemoveDevice(device);
            }
            CheckConnectDevice();
        }
    }
}
