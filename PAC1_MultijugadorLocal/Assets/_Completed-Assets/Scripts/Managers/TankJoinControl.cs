using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TankJoinControl : MonoBehaviour
{
    private Complete.GameManager gameManager;

    private void Awake()
    {
        gameManager = FindObjectOfType<Complete.GameManager>();
    }

    public void OnJoin(int id)
    {
        gameManager.InstantiateNewPlayer(id);
    }

    public void OnJoinGamepad()
    {
        gameManager.InstantiateNewUndefinedPlayer();
    }
}
