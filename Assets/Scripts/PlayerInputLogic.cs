using System;
using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputLogic : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private StarterAssetsInputs input;

    PlayerController playerControler;

    private void Awake()
    {
        PlayerController.onStart += PlayerController_Started;
    }

    private void PlayerController_Started(PlayerController playerControler)
    {
        this.playerControler = playerControler;
        playerControler.SetupInput(playerInput);

        playerControler.onJumpChange += JumpValue_Changed;
    }

    private void JumpValue_Changed(bool value)
    {
        input.jump = value;
    }

    private void Update()
    {
        if (playerControler)
        {
            playerControler.SetLookDirection(input.look);
            playerControler.SetMove(input.move);
            playerControler.SetJump(input.jump);
            playerControler.SetSprint(input.sprint);
        }

        var animator = playerControler.GetComponent<Animator>();
        if (Input.GetKey(KeyCode.R))
        {
            
            animator.SetBool("LongRange", true);
        }
        else
        {
            animator.SetBool("LongRange", false);
        }
        
    }
}
