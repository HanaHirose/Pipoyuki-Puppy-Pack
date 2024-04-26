using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraControl : MonoBehaviour
{
    private PlayerInputAction playerInputAction;

    [SerializeField] private Transform playerTransform;
    [SerializeField] private Vector3 offsetValues; //カメラとプレイヤーとのオフセット調整用
    private Vector3 targetOffsetXZ;
    private Vector3 offset;
    private Vector3 offsetXZ;
    private bool isChangingDirection = false;
    private float speed = 5f;

    

    private void OnEnable()
    {
        playerInputAction = new PlayerInputAction();
        playerInputAction.Player.Enable();
        playerInputAction.Player.Camera.performed += CameraDirection;
    }

    private void OnDisable()
    {
        playerInputAction.Player.Camera.performed -= CameraDirection;
        playerInputAction.Player.Disable();
    }


    private void Start()
    {
        offsetXZ = -Vector3.forward * offsetValues.z;
    }

    private void Update()
    {
        if (isChangingDirection)
        {
            offsetXZ = Vector3.Slerp(offsetXZ, targetOffsetXZ, speed * Time.deltaTime);

            if(Vector3.Distance(offsetXZ, targetOffsetXZ)< 0.01f)
            {
                isChangingDirection = false;
            }
        }
        offset = offsetXZ + Vector3.up * offsetValues.y;

        transform.position = playerTransform.position + offset;
        transform.LookAt(playerTransform);
    }

    private void CameraDirection(InputAction.CallbackContext context)
    {
        targetOffsetXZ = -playerTransform.forward * offsetValues.z;
        isChangingDirection = true;
    }
}
