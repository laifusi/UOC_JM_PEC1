using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField] private GameObject gameManager;

    public void OnStartButton(InputAction.CallbackContext context)
    {
        /*
        if (!tanks.ContainsKey(0) && Input.GetButtonDown("Start1"))
        {
            InstantiateNewPlayer(0);
        }
        if (!tanks.ContainsKey(1) && Input.GetButtonDown("Start2"))
        {
            InstantiateNewPlayer(1);
        }
        if (!tanks.ContainsKey(2) && Input.GetButtonDown("Start3"))
        {
            InstantiateNewPlayer(2);
        }
        if (!tanks.ContainsKey(3) && Input.GetButtonDown("Start4"))
        {
            InstantiateNewPlayer(3);
        }*/
    }
}
