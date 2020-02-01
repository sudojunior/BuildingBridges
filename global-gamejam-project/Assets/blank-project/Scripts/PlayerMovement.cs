﻿using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Physics")]
    private Rigidbody rigidBody;

    [Header("Position")]
    [SerializeField]
    private float movementSpeed;
    [SerializeField]
    private float snapValue;
    private Vector3 nextPosition;

    [SerializeField]
    private Vector3[] directionsToCheck;
    [SerializeField]
    private float checkDistance;
    [SerializeField]
    private float groundCheckDistance;

    [SerializeField]
    private float moveDelay;

    [SerializeField]
    private LayerMask maskCheck;
    [Space]

    [Header("Rotation")]
    [SerializeField]
    private float rotateSpeed;

    [Header("Input")]
    [SerializeField]
    private string moveXName;
    private float moveX;
    [SerializeField]
    private string moveZName;
    private float moveZ;

    [Header("Coroutines")]
    private Coroutine moveCoroutine;
    private Coroutine rotateCoroutine;

    public delegate void OnPlayerMove(Vector3 currentPosition);
    public event OnPlayerMove onPlayerMove;
    

    private void Start()
    {
        rigidBody = this.GetComponent<Rigidbody>();
        onPlayerMove += InvokePosition;
    }

    private void Update()
    {
        GetPlayerInput();
    }

    private void FixedUpdate()
    {
        if (moveX != 0 || moveZ != 0)
        {
            onPlayerMove?.Invoke(rigidBody.position);
        }
    }

    private void InvokePosition(Vector3 currentPosition)
    {
        nextPosition = new Vector3(currentPosition.x + (snapValue * moveZ), currentPosition.y, currentPosition.z + (snapValue * -moveX));
        Vector3 direction = new Vector3(moveZ, 0, -moveX);
        rotateCoroutine = StartCoroutine(AimPlayer(direction, rotateSpeed));

        RaycastHit hit;

        if (Physics.Raycast(rigidBody.position, direction, out hit, checkDistance, maskCheck))
        {
            if (hit.collider.GetComponent<Ladder>() != null)
            {
                nextPosition = hit.transform.position;
                nextPosition.y += 2f;

                if (moveCoroutine != null) StopCoroutine(moveCoroutine);
                moveCoroutine = StartCoroutine(MovePosition(nextPosition, moveDelay));
                
                return;
            }

            else
            {
                return;
            }
        }

        RaycastHit belowHit;
        if (Physics.Raycast(transform.position, -Vector3.up, out belowHit, groundCheckDistance, maskCheck))
        {
            if (belowHit.collider.GetComponent<Ladder>() != null)
            {
                if (moveCoroutine == null) moveCoroutine = StartCoroutine(MovePosition(nextPosition, moveDelay));
                return;
            }

            else if (belowHit.collider == null)
            {
                return;
            }
        }

        if (!Physics.Raycast(nextPosition, -Vector3.up, groundCheckDistance, maskCheck)) return;

        if (moveCoroutine == null) moveCoroutine = StartCoroutine(MovePosition(nextPosition, moveDelay));
        rotateCoroutine = StartCoroutine(AimPlayer(direction, rotateSpeed));
    }

    private IEnumerator MovePosition(Vector3 nextPosition, float moveDelay)
    {
        float t = 0;
        while (t <= .5f)
        {
            t += Time.deltaTime * movementSpeed;
            rigidBody.position = Vector3.Lerp(rigidBody.position, nextPosition, t);
            yield return new WaitForFixedUpdate();
        }

        rigidBody.position = nextPosition;

        yield return new WaitForSeconds(moveDelay);
        moveCoroutine = null;
    }

    private IEnumerator AimPlayer(Vector3 direction, float rotateSpeed)
    {
        float t = 0;
        while (t <= 1)
        {
            t += Time.deltaTime * rotateSpeed;
            rigidBody.transform.forward = Vector3.Slerp(rigidBody.transform.forward, direction, t);
            yield return new WaitForFixedUpdate();
        }
      
        rigidBody.transform.forward = direction;
        rotateCoroutine = null;
        yield return null;
    }

    private void GetPlayerInput()
    {
        moveX = Input.GetAxisRaw(moveXName);
        moveZ = Input.GetAxisRaw(moveZName);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * checkDistance);
    }

}
