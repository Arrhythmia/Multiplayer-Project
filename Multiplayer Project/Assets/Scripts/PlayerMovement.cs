using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float jumpForce;
    private float movementDir;

    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;

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

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
            Jump();
    }

    private void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void FixedUpdate()
    {
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1f) * Time.deltaTime;
        }

        MovePlayer(movementDir);
    }
}
