using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boy : MonoBehaviour {
    private const string IS_SCARED = "IsScared";
    private const string SHOULD_DROP_BALL = "ShouldDropBall";
    private const string SHOULD_BREATHE = "ShouldBreathe";
    private enum State {
        Idle,
        Scared
    };
    [SerializeField] private State initialState = State.Idle;
    [SerializeField] private Transform alertArea;
    [SerializeField] private Animator boyAnimator;
    [SerializeField] private Player player;
    [SerializeField] private GameObject ball;
    [SerializeField] private ContactFilter2D alertFilter;
    [SerializeField] private float timeScared = 2f;
    [SerializeField] private float timeShouldBreathe = 5f;
    private State currentState;
    private float timeScaredElapsed = 0f;
    private bool isFirstInteraction = true;
    private float timeSinceBreathe = 0f;
    
    private void Start() {
        ChangeState(initialState);
    }

    private void Update() {
        if (currentState == State.Scared) timeScaredElapsed += Time.deltaTime;

        switch (currentState) {
            case State.Idle:
                HandleStateIdle();
                break;
            case State.Scared:
                HandleStateScared();
                break;
        }
    }

    private void HandleStateIdle() {
        Collider2D[] alertAreaResults = new Collider2D[1];
        int numCollisions = Physics2D.OverlapCircle(
            alertArea.position,
            alertArea.localScale.x/2,
            alertFilter,
            alertAreaResults
        );

        if (numCollisions <= 0 || !player.GetDidHonk()) {
            timeSinceBreathe += Time.deltaTime;

            if (timeSinceBreathe >= timeShouldBreathe) {
                boyAnimator.SetBool(SHOULD_BREATHE, true);
                timeSinceBreathe = 0f;
            } else {
                boyAnimator.SetBool(SHOULD_BREATHE, false);
            }

            return;
        }

        timeScaredElapsed = 0f;
        boyAnimator.SetBool(IS_SCARED, true);
        ChangeState(State.Scared);
    }

    private void HandleStateScared() {
        if (timeScaredElapsed < timeScared) return;

        timeScaredElapsed = 0f;

        if (isFirstInteraction) {
            isFirstInteraction = false;
            boyAnimator.SetBool(SHOULD_DROP_BALL, true);
            ball.SetActive(true);
            return;
        }

        boyAnimator.SetBool(IS_SCARED, false);
        ChangeState(State.Idle);
    }

    private void ChangeState(State toState) {
        currentState = toState;
    }
}
