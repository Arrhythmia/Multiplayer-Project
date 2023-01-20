using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    private Rigidbody rb;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float jumpForce;
    private float movementDir;


    private Collider playerCollider;

    private void Start()
    {
        if (!IsOwner)
        {
            Destroy(GetComponent<PlayerInput>());
            Destroy(GetComponent<DevTools>());
            Destroy(this);
            return;
        }
        rb = GetComponent<Rigidbody>();

        playerCollider = GetComponentInChildren<Collider>();
    }
    public void OnMove(InputAction.CallbackContext context)
    {
        movementDir = context.ReadValue<float>();
    }

    [SerializeField] private float maxSpeed = 1.0f;
    [SerializeField] private float minSpeed = 1.0f;
    [SerializeField] private float stopSpeed = 1.0f;
    private void MovePlayer(float movementDir)
    {
        if (movementDir == 0f || (movementDir > 0 && rb.velocity.x < 0) || (movementDir < 0 && rb.velocity.x > 0))
        {
            rb.velocity = new Vector3(rb.velocity.x / stopSpeed, rb.velocity.y, 0f);
        }
        if (Mathf.Abs(rb.velocity.x) <= minSpeed)
        {
            rb.AddForce(movementSpeed * movementDir, 0f, 0f, ForceMode.VelocityChange);
            //rb.velocity = new Vector3(movementSpeed * movementDir, rb.velocity.y, 0f);
        }
        else if (Mathf.Abs(rb.velocity.x) > minSpeed && Mathf.Abs(rb.velocity.x) <= maxSpeed)
        {
            rb.AddForce(movementSpeed * movementDir, 0f, 0f, ForceMode.Acceleration);
            //rb.velocity += new Vector3(movementSpeed * movementDir, 0f, 0f);
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && IsGrounded())
        {
            Jump();
            jumping = true;
        }
        if (context.canceled)
        {
            jumping = false;
        }
    }

    private void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce);
    }

    private void FixedUpdate()
    {
        if (jumping == true)
        {
            timeSinceLastJump += Time.deltaTime;
        }
        if (IsGrounded())
        {
            timeSinceLastJump = 0f;
        }
        if (timeSinceLastJump >= holdJumpTime)
        {
            jumping = false;
        }
        MovePlayer(movementDir);
        HandleGravity2();
    }

    [Header("Gravity")]
    [SerializeField] private float groundColliderHeight = 0.2f;
    [SerializeField] private LayerMask platformLayerMask;
    /*[SerializeField] private float maxGravity = 30f;
    [SerializeField] private float minGravity = 9.8f;
    [SerializeField] private float gravity = 9.8f;
    [SerializeField] private float gravityMultiplier = 1.1f;
    [SerializeField] private float groundColliderHeight = 0.2f;
    [SerializeField] private LayerMask platformLayerMask;


    private void HandleGravity()
    {
        if (!IsGrounded() && gravity <= maxGravity)
        {
            gravity *= gravityMultiplier;
        }
        if (IsGrounded())
        {
            gravity = minGravity;
        }
        rb.AddForce(Vector3.down * gravity, ForceMode.Acceleration);
    }*/

    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;
    private bool jumping = false;
    [SerializeField] private float holdJumpTime = 200f;
    private float timeSinceLastJump = 0f;
    private void HandleGravity2()
    {
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.velocity.y > 0 && !jumping)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
    }
    private bool IsGrounded()
    {
        Vector3 boxCenter = playerCollider.bounds.center;
        Vector3 halfExtents = playerCollider.bounds.extents;

        // modify the height of the box so that origin of the box cast isn't intersecting with the ground
        halfExtents.y = groundColliderHeight;

        float maxDistance = playerCollider.bounds.extents.y;

        return Physics.BoxCast(boxCenter, halfExtents, Vector3.down, transform.rotation, maxDistance, platformLayerMask);

    }
}
