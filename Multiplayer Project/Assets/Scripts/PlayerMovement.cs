using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private float movementSpeed;
    private float movementDir;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    public void OnMove(InputAction.CallbackContext context)
    {
        Debug.Log(context.ReadValue<float>());

        movementDir = context.ReadValue<float>();
    }

    private void MovePlayer(float movementDir)
    {
        rb.AddForce(movementSpeed * movementDir, 0f, 0f, ForceMode.Force);
    }

    private void FixedUpdate()
    {


        MovePlayer(movementDir);
    }
}
