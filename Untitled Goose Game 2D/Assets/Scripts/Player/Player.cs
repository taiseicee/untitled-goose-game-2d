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
                HandleStateRun(); 
                break;
            case State.Fall:
                HandleStateFall();
                break;
        }

        HandleJump();
        HandleHonk();
    }

    private void HandleStateIdle() {
        if (moveComponent.IsFalling()) {
            ChangeState(State.Fall);
            return;
        }
        float playerDirectionInput = playerInputActions.Player.Move.ReadValue<float>();
        if (playerDirectionInput == 0f) return;

        bool shouldRun = playerInputActions.Player.Run.ReadValue<float>() != 0f;
        if (shouldRun) {
            ChangeState(State.Run);
            return;
        }

        ChangeState(State.Walk);
    }

    private void HandleStateWalk() {
        if (moveComponent.IsFalling()) {
            ChangeState(State.Fall);
            return;
        }

        bool shouldRun = playerInputActions.Player.Run.ReadValue<float>() != 0f;
        if (shouldRun) {
            ChangeState(State.Run);
            return;
        }

        float playerDirectionInput = playerInputActions.Player.Move.ReadValue<float>();
        moveComponent.Walk(playerDirectionInput);
        if (!moveComponent.IsMoving()) {
            ChangeState(State.Idle);
            return;
        }
    }

    private void HandleStateRun() {
        if (moveComponent.IsFalling()) {
            ChangeState(State.Fall);
            return;
        }

        bool shouldRun = playerInputActions.Player.Run.ReadValue<float>() != 0f;
        if (!shouldRun) {
            ChangeState(State.Walk);
            return;
        }

        float playerDirectionInput = playerInputActions.Player.Move.ReadValue<float>();
        moveComponent.Run(playerDirectionInput);
        
        if (!moveComponent.IsMoving()) {
            ChangeState(State.Idle);
            return;
        }
    }

    private void HandleStateFall() {
        if (moveComponent.IsFalling()) {
            float playerDirectionInput = playerInputActions.Player.Move.ReadValue<float>();
            moveComponent.Fall(playerDirectionInput);
            return;
        }

        moveComponent.SnapToGround();

        if (!moveComponent.IsMoving()) {
            ChangeState(State.Idle);
            return;
        }

        bool shouldRun = playerInputActions.Player.Run.ReadValue<float>() != 0f;
        if (shouldRun) {
            ChangeState(State.Run);
            return;
        }

        ChangeState(State.Walk);
        return;
    }

    private void HandleJump() {
        if (moveComponent.IsFalling()) return;
        if (playerInputActions.Player.Jump.ReadValue<float>() == 0f) return;
        moveComponent.Jump();
    }

    private void HandleHonk() {
        if (playerInputActions.Player.Honk.ReadValue<float>() == 0f) return;
        print("HONK!");
    }

    private void ChangeState(State toState) {
        currentState = toState;
    }
}
