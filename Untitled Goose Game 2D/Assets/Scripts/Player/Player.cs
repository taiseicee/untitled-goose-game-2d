using System;
using UnityEngine;

public class Player : MonoBehaviour {
    private enum State {
        Idle,
        Walk,
        Run, 
        Fall,
        Jump
    };

    [SerializeField] private State initialState = State.Idle;
    [SerializeField] private PlayerMoveComponent moveComponent;
    [SerializeField] private SpriteRenderer playerSprite;
    [SerializeField] private PlayerCamera playerCamera;
    private State currentState;
    private PlayerInputActions playerInputActions;
    private float direction = 1f;

    private void Awake() {
        ChangeState(initialState);
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        moveComponent.Init(this);
    }

    private void Update() {
        float playerDirectionInput = playerInputActions.Player.Move.ReadValue<float>();
        bool shouldRun = playerInputActions.Player.Run.ReadValue<float>() != 0f;
        bool shouldJump = playerInputActions.Player.Jump.ReadValue<float>() != 0f;
        bool shouldHonk = playerInputActions.Player.Honk.ReadValue<float>() != 0f;

        direction = playerDirectionInput == 0f ? direction : playerDirectionInput / Mathf.Abs(playerDirectionInput);

        switch (currentState) {
            case State.Idle:
                HandleStateIdle(playerDirectionInput, shouldRun, shouldJump);
                break;
            case State.Walk:
                HandleStateWalk(playerDirectionInput, shouldRun, shouldJump); 
                break;
            case State.Run:
                HandleStateRun(playerDirectionInput, shouldRun, shouldJump); 
                break;
            case State.Fall:
                HandleStateFall(playerDirectionInput, shouldRun);
                break;
            case State.Jump:
                HandleStateJump(playerDirectionInput);
                break;
        }

        if (shouldHonk) HandleHonk();
        playerSprite.flipX = direction < 0f ? true : false;
        playerCamera.SetDirection(direction);
    }

    private void HandleStateIdle(float playerDirectionInput, bool shouldRun, bool shouldJump) {
        if (moveComponent.IsFalling()) {
            ChangeState(State.Fall);
            return;
        }

        if (shouldJump) {
            moveComponent.Jump();
            HandleStateJump(playerDirectionInput);
            return;
        }
        
        if (playerDirectionInput == 0f) return;

        if (shouldRun) {
            ChangeState(State.Run);
            return;
        }

        ChangeState(State.Walk);
    }

    private void HandleStateWalk(float playerDirectionInput, bool shouldRun, bool shouldJump) {
        if (moveComponent.IsFalling()) {
            ChangeState(State.Fall);
            return;
        }

        if (shouldJump) {
            moveComponent.Jump();
            HandleStateJump(playerDirectionInput);
            return;
        }

        if (shouldRun) {
            ChangeState(State.Run);
            return;
        }

        moveComponent.Walk(playerDirectionInput);
        if (!moveComponent.IsMoving()) {
            ChangeState(State.Idle);
            return;
        }
    }

    private void HandleStateRun(float playerDirectionInput, bool shouldRun, bool shouldJump) {
        if (moveComponent.IsFalling()) {
            ChangeState(State.Fall);
            return;
        }

        if (shouldJump) {
            moveComponent.Jump();
            HandleStateJump(playerDirectionInput);
            return;
        }

        if (!shouldRun) {
            ChangeState(State.Walk);
            return;
        }

        moveComponent.Run(playerDirectionInput);

        if (!moveComponent.IsMoving()) {
            ChangeState(State.Idle);
            return;
        }
    }

    private void HandleStateFall(float playerDirectionInput, bool shouldRun) {
        if (moveComponent.IsFalling()|| moveComponent.IsJumping()) {
            moveComponent.Fall(playerDirectionInput);
            return;
        }

        moveComponent.LandAfterFall();

        if (!moveComponent.IsMoving()) {
            ChangeState(State.Idle);
            return;
        }

        if (shouldRun) {
            ChangeState(State.Run);
            return;
        }

        ChangeState(State.Walk);
        return;
    }

    private void HandleStateJump(float playerDirectionInput) {
        moveComponent.Fall(playerDirectionInput);
        if (!moveComponent.IsJumping()) {
            ChangeState(State.Fall);
            return;
        }
    }

    private void HandleHonk() {
        print("HONK!");
    }

    private void ChangeState(State toState) {
        currentState = toState;
    }
}
