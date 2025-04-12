using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;
using System;

public class ThirdPersonShooterCameraLogic : MonoBehaviour
{ 
    [SerializeField] CinemachineVirtualCamera defaultCam;
    [SerializeField] CinemachineVirtualCamera shooterCam;
    [SerializeField] CinemachineBrain cinemachineBrain;
    [SerializeField] LayerMask aimLayers;
    [SerializeField] Transform debugAim;

    public bool ActiveLongRangeWeapon { get; set; } = true;

    const int lowPriority = 5;
    const int hightPriority = 8;


    PlayerController playerController;
    Camera mainCamera;
    Coroutine blendTimeReset;
    Vector3 aimWorldPosition;
    float previousBlendTime;
    bool allowSavePreviousBlendTime;
    bool aimState;

    private void Awake()
    {
        PlayerController.onStart += PlayerController_Started;
    }

    private void PlayerController_Started(PlayerController controller)
    {
        playerController = controller;
    }

    private void Start()
    {
        previousBlendTime = cinemachineBrain.m_DefaultBlend.m_Time;

        if (mainCamera == null)
        {
            mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
            if (mainCamera == null)
            {
                Debug.LogError("Не найдена камера с тэгом MainCamera");
            }
        }
    }

    public void OnAim(InputValue value)
    {
        aimState = value.isPressed;
        SetAimCameraState(value.isPressed);
    }

    private void Update()
    {
        if (!ActiveLongRangeWeapon)
            return;

        var aimPos = new Vector2(Screen.width / 2, Screen.height / 2);

        var ray = mainCamera.ScreenPointToRay(aimPos);
        if (Physics.Raycast(ray, out var hit, 888, aimLayers))
        {
            aimWorldPosition = hit.point;
            if (debugAim)
            {
                debugAim.transform.position = aimWorldPosition;
            }
        }

        if (playerController)
        {
            if (aimState)
            {
                var playerTransform = playerController.transform;
                var worldTarget = aimWorldPosition;
                worldTarget.y = playerController.transform.position.y;
                var aimDir = (worldTarget - playerTransform.position).normalized;


                playerTransform.forward = Vector3.Lerp
                (
                    playerTransform.forward,
                    aimDir,
                    Time.deltaTime * 18f
                );

                playerController.SetEnableRotationOnMove(false);
            }
            else
            {
                playerController.SetEnableRotationOnMove(true);
            }

            
        }
    }

    void SetAimCameraState(bool state)
    {
        if (!ActiveLongRangeWeapon)
            return;

        StopResetBlentTime();

        if (!state)
        {
            defaultCam.Priority = hightPriority;
            shooterCam.Priority = lowPriority;

            blendTimeReset = StartCoroutine(ResetBlendTime());
        }
        else
        {
            if (allowSavePreviousBlendTime)
            {
                previousBlendTime = cinemachineBrain.m_DefaultBlend.m_Time;
                allowSavePreviousBlendTime = false;
            }

            cinemachineBrain.m_DefaultBlend.m_Time = 0.18f;
            defaultCam.Priority = lowPriority;
            shooterCam.Priority = hightPriority;
   
        }
    }

    void StopResetBlentTime()
    {
        if (blendTimeReset != null)
        {
            StopCoroutine(blendTimeReset);
        }
    }

    IEnumerator ResetBlendTime()
    {
        yield return new WaitForSeconds(0.19f);

        cinemachineBrain.m_DefaultBlend.m_Time = previousBlendTime;

        blendTimeReset = null;
        allowSavePreviousBlendTime = true;
    }
}
