using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    private enum State {
        Idle,
        Walk,
        Run, 
        Fall
    };

    [SerializeField] private State initialState = State.Idle;
    [SerializeField] private PlayerMoveComponent moveComponent;
    private State currentState;
    private PlayerInputActions playerInputActions;

    private void Awake() {
        ChangeState(initialState);
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        moveComponent.Init(this);
    }

    private void Update() {
        switch (currentState) {
            case State.Idle:
                HandleStateIdle();
                break;
            case State.Walk:
                HandleStateWalk(); 
                break;
            case State.Run:
                break;
            case State.Fall:
                HandleStateFall();
                break;
        }

        HandleJump();
    }

    private void HandleStateIdle() {
        if (moveComponent.IsFalling()) {
            ChangeState(State.Fall);
            return;
        }
        float playerDirectionInput = playerInputActions.Player.Move.ReadValue<float>();
        if (playerDirectionInput != 0) ChangeState(State.Walk);
    }

    private void HandleStateWalk() {
        if (moveComponent.IsFalling()) {
            ChangeState(State.Fall);
            return;
        }
        float playerDirectionInput = playerInputActions.Player.Move.ReadValue<float>();
        moveComponent.Walk(playerDirectionInput);
        if (!moveComponent.IsMoving()) {
            ChangeState(State.Idle);
            return;
        }
    }

    private void HandleStateFall() {
        if (!moveComponent.IsFalling() && moveComponent.IsMoving()) {
            moveComponent.SnapToGround();
            ChangeState(State.Walk);
            return;
        }
        if (!moveComponent.IsFalling()) {
            moveComponent.SnapToGround();
            ChangeState(State.Idle);
            return;
        }
        float playerDirectionInput = playerInputActions.Player.Move.ReadValue<float>();
        moveComponent.Fall(playerDirectionInput);
    }

    private void HandleJump() {
        if (moveComponent.IsFalling()) return;
        if (playerInputActions.Player.Jump.ReadValue<float>() == 0f) return;
        moveComponent.Jump();
    }

    private void ChangeState(State toState) {
        print(toState);
        currentState = toState;
    }
}
