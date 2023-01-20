using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class GrapplingHook : NetworkBehaviour
{
    private Camera playerCamera;
    private SpringJoint hookSJ;
    private Rigidbody rb;

    private void Start()
    {
        if (!IsOwner)
        {
            Destroy(this);
            return;
        }
        rb = GetComponent<Rigidbody>();
        Camera.main.transform.SetParent(transform);
        playerCamera = GetComponentInChildren<Camera>();
        
    }
    public void OnHook(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Hook();
        }
        if (context.canceled)
        {
            Destroy(hookSJ);
        }
    }
    private void Hook()
    {
        if (hookSJ == null)
        {
            hookSJ = transform.AddComponent<SpringJoint>();
            hookSJ.autoConfigureConnectedAnchor = false;
            hookSJ.anchor = new Vector3(0f, 1f, 0f);
            hookSJ.connectedAnchor = GetMousePosition();


            hookSJ.spring = spring;
            hookSJ.damper = damper;
            hookSJ.minDistance = minDistance;
            hookSJ.maxDistance = maxDistance;
            hookSJ.tolerance = tolerance;
            hookSJ.massScale = massScale;
            hookSJ.connectedMassScale = connectedMassScale;
        }
    }

    [Header("Created SpringJoint")]
    [SerializeField] private float spring = 10f;
    [SerializeField] private float damper = 0.2f;
    [SerializeField] private float minDistance = 0f;
    [SerializeField] private float maxDistance = 0f;
    [SerializeField] private float tolerance = 0.0025f;
    [SerializeField] private float massScale = 1f;
    [SerializeField] private float connectedMassScale = 1f;
    private Vector3 GetMousePosition()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100f))
        {
            return hit.point;
        }
        return Vector3.zero;
    }

    private float DistanceBetweenPlayerAndPoint()
    {
        if (hookSJ != null)
            return Vector3.Distance(transform.position, hookSJ.connectedAnchor);
        return -1f;
    }
    private void FixedUpdate()
    {
        float distance = DistanceBetweenPlayerAndPoint();
        if (distance > -1 && hookSJ.maxDistance > distance)
        {
            hookSJ.maxDistance = distance;
        }
    }


}
