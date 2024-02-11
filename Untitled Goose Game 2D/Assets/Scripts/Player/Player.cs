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
    [SerializeField] private SpriteRenderer playerSprite;
    [SerializeField] private Camera playerCamera;
    [SerializeField, Range(0f, 1f)] private float cameraMoveSpeed = 0.05f;
    private State currentState;
    private PlayerInputActions playerInputActions;
    private float direction = 1f;
    private float CameraHorizontalOffset;

    private void Awake() {
        ChangeState(initialState);
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        moveComponent.Init(this);

        CameraHorizontalOffset = playerCamera.transform.localPosition.x;
    }

    private void Update() {
        float playerDirectionInput = playerInputActions.Player.Move.ReadValue<float>();
        bool shouldRun = playerInputActions.Player.Run.ReadValue<float>() != 0f;
        bool shouldJump = playerInputActions.Player.Jump.ReadValue<float>() != 0f;
        bool shouldHonk = playerInputActions.Player.Honk.ReadValue<float>() != 0f;

        direction = playerDirectionInput == 0f ? direction : playerDirectionInput / Mathf.Abs(playerDirectionInput);

        switch (currentState) {
            case State.Idle:
                HandleStateIdle(playerDirectionInput, shouldRun);
                break;
            case State.Walk:
                HandleStateWalk(playerDirectionInput, shouldRun); 
                break;
            case State.Run:
                HandleStateRun(playerDirectionInput, shouldRun); 
                break;
            case State.Fall:
                HandleStateFall(playerDirectionInput, shouldRun);
                break;
        }

        if (shouldJump) HandleJump();
        if (shouldHonk) HandleHonk();
        playerSprite.flipX = direction < 0 ? true : false;
        ChangeCameraDirection();
    }

    private void HandleStateIdle(float playerDirectionInput, bool shouldRun) {
        if (moveComponent.IsFalling()) {
            ChangeState(State.Fall);
            return;
        }
        
        if (playerDirectionInput == 0f) return;

        if (shouldRun) {
            ChangeState(State.Run);
            return;
        }

        ChangeState(State.Walk);
    }

    private void HandleStateWalk(float playerDirectionInput, bool shouldRun) {
        if (moveComponent.IsFalling()) {
            ChangeState(State.Fall);
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

    private void HandleStateRun(float playerDirectionInput, bool shouldRun) {
        if (moveComponent.IsFalling()) {
            ChangeState(State.Fall);
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
        if (moveComponent.IsFalling()) {
            moveComponent.Fall(playerDirectionInput);
            return;
        }

        moveComponent.SnapToGround();

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

    private void HandleJump() {
        if (currentState == State.Fall) return;
        moveComponent.Jump();
    }

    private void HandleHonk() {
        print("HONK!");
    }

    private void ChangeCameraDirection() {
        float newCameraHorizontalOffset = Mathf.MoveTowards(
            playerCamera.transform.localPosition.x, 
            direction * CameraHorizontalOffset, 
            cameraMoveSpeed
        );
        playerCamera.transform.localPosition = new Vector3(
            newCameraHorizontalOffset,
            playerCamera.transform.localPosition.y,
            playerCamera.transform.localPosition.z
        );
    }

    private void ChangeState(State toState) {
        currentState = toState;
    }
}
