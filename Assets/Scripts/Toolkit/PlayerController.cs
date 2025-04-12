using System;
using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] CharacterMove characterMove;

    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject CinemachineCameraTarget;

    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 70.0f;

    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -30.0f;

    public float sensitivityMouseY = 1.5f;

    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    public float CameraAngleOverride = 0.0f;

    [Tooltip("For locking the camera position on all axis")]
    public bool LockCameraPosition = false;

    [ReadOnlyField] public float _cinemachineTargetYaw;
    [ReadOnlyField] public float _cinemachineTargetPitch;



    internal void SetSprint(bool sprint)
    {
        characterMove.Sprint = sprint;
    }

    public static Action<PlayerController> onStart; 
    public Action<bool> onJumpChange;

    private bool IsCurrentDeviceMouse
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            return _playerInput ? _playerInput.currentControlScheme == "KeyboardMouse" : true;
#else
				return false;
#endif
        }
    }

    public bool IsOwner { get; set; } = true;
    public bool AllowCameraRotation { get; set; } = true;

    private const float _threshold = 0.01f;

    private GameObject _mainCamera;
#if ENABLE_INPUT_SYSTEM
    private PlayerInput _playerInput;
#endif
    private Vector2 lookDirection;



    private void Awake()
    {
        // get a reference to our main camera
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            if (_mainCamera == null)
            {
                Debug.LogError("Не найдена камера с тэгом MainCamera");
            }
        }
    }

    private void Start()
    {
        _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
        
        onStart?.Invoke(this);

        characterMove.onJumpChange += JumpValue_Changed;
    }

    private void JumpValue_Changed(bool value)
    {
        onJumpChange?.Invoke(value);
    }

    public void SetMove(Vector2 move)
    {
        characterMove.Movenment = move;
        characterMove.yDirectionOverride = _mainCamera.transform.eulerAngles.y;
    }

    public void SetEnableRotationOnMove(bool value)
    {
        characterMove.AllowRotateOnMove = value;
    }

    public void SetJump(bool value)
    {
        characterMove.Jump = value;
    }

    private void LateUpdate()
    {
#if !UNITY_SERVER
        if (!IsOwner || !AllowCameraRotation)
            return;

        CameraRotation();
#endif
    }

    public void SetupInput(PlayerInput playerInput)
    {
        _playerInput = playerInput;
    }

    public void SetLookDirection(Vector2 value)
    {
        lookDirection = value;
    }

    public void SetLookClamps(float bottom, float top)
    {
        BottomClamp = bottom;
        TopClamp = top;
    }

    private void CameraRotation()
    {
        // if there is an input and camera position is not fixed
        if (lookDirection.sqrMagnitude >= _threshold && !LockCameraPosition)
        {
            //Don't multiply mouse input by Time.deltaTime;
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            _cinemachineTargetYaw += lookDirection.x * deltaTimeMultiplier;
            _cinemachineTargetPitch += lookDirection.y * deltaTimeMultiplier * sensitivityMouseY;
        }

        // clamp our rotations so our values are limited 360 degrees
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        // Cinemachine will follow this target
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
            _cinemachineTargetYaw, 0.0f);
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    public void SetPitchAndYaw(float pitch, float yaw)
    {
        _cinemachineTargetPitch = pitch;
        _cinemachineTargetYaw = yaw;

        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

    }
}
