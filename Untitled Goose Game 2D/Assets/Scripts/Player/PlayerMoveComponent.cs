using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMoveComponent : MonoBehaviour {
    [SerializeField, Range(0f, 30f)] private float maxWalkSpeed = 5f;
    [SerializeField, Range(0f, 30f)] private float maxRunSpeed = 10f;
    [SerializeField, Range(0f, 30f)] private float maxFallSpeed = 3f;
    [SerializeField, Range(0f, 25f)] private float maxAcceleration = 12f;
    [SerializeField, Range(0f, 100f)] private float jumpInitialVelocity = 8f;
    [SerializeField, Range(0f, 1f)] private float gravityMultiplier = 0.2f;
    [SerializeField] private Transform groundCheckArea;
    [SerializeField] private ContactFilter2D walkableFilter;
    [SerializeField] private Transform predictiveCollider;
    [SerializeField] private ContactFilter2D collidableFilter;

    private float currentMaxSpeed;
    private Vector2 velocity;
    private Player player;
    private Collider2D[] fallCollisionResults = new Collider2D[1];
     private bool canJump = false;

    public void Init(Player player) {
        this.player = player;
    }

    public void Walk(float input) {
        currentMaxSpeed = maxWalkSpeed;
        CollideAndSlide(input);
    }

    public void Run(float input) {
        currentMaxSpeed = maxRunSpeed;
        CollideAndSlide(input);
    }

    public void Fall(float input) {
        if (velocity.y > 0f)
            velocity += Physics2D.gravity * Time.deltaTime;
        else 
            velocity += Physics2D.gravity * gravityMultiplier * Time.deltaTime;
        currentMaxSpeed = maxFallSpeed;
        CalculateVelocity(input);
        UpdatePosition();
    }

    public void Jump() {
        if (canJump) {
            velocity.y += jumpInitialVelocity;
            canJump = false;
        }
        UpdatePosition();
    }

    public bool IsFalling() {
        int numCollisions = Physics2D.OverlapBox(
            groundCheckArea.position,
            groundCheckArea.localScale,
            0f,
            walkableFilter,
            fallCollisionResults
        );
        return numCollisions <= 0;
    }

    public void SnapToGround() {
        if (fallCollisionResults.Length < 1) return;
        Vector2 groundSurface = Physics2D.ClosestPoint(
            predictiveCollider.transform.position, 
            fallCollisionResults[0]
        );
        player.transform.position = new Vector3(
            player.transform.position.x, 
            groundSurface.y, 
            player.transform.position.z
        );
        velocity.y = 0f;
        canJump = true;
    }

    public bool IsMoving() {
        return velocity != Vector2.zero;
    }

    private void CollideAndSlide(float input) {
        CalculateVelocity(input);
        // TODO: Collision Detection

        RaycastHit2D[] circleCastHitResults = new RaycastHit2D[1];

        int numCircleCastHitResults = Physics2D.CircleCast(
            predictiveCollider.transform.position,
            predictiveCollider.localScale.x/2.1f,
            velocity.normalized,
            collidableFilter,
            circleCastHitResults,
            velocity.magnitude * Time.deltaTime
        );
        
        bool didCollide = numCircleCastHitResults > 0;
        if (!didCollide) {
            UpdatePosition();
            return;
        }

        velocity.x = 0;
        UpdatePosition();
    }

    private void CalculateVelocity(float input) {
        Vector2 desiredVelocity = new Vector2(input, 0f) * currentMaxSpeed;

        float maxSpeedChange = maxAcceleration * Time.deltaTime;

        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
    }

    private void UpdatePosition() {
        Vector3 displacement = new Vector3(velocity.x, velocity.y, 0f) * Time.deltaTime;
        player.transform.position += displacement;
    }
}
