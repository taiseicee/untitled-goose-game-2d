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
    [SerializeField, Range(0f, 50f)] private float jumpInitialVelocity = 8f;
    [SerializeField, Range(0f, 1f)] private float gravityMultiplier = 0.2f;
    [SerializeField, Range(0f, 90f)] private float maxWalkableAngle = 60f;
    [SerializeField] private Transform groundCheckArea;
    [SerializeField] private ContactFilter2D walkableFilter;
    [SerializeField] private CircleCollider2D predictiveCollider;
    [SerializeField] private ContactFilter2D collidableFilter;

    private float currentMaxSpeed;
    private Vector2 velocity;
    private Player player;

    public void Init(Player player) {
        this.player = player;
    }

    public void Walk(float input) {
        currentMaxSpeed = maxWalkSpeed;
        Slide(input);
    }

    public void Run(float input) {
        currentMaxSpeed = maxRunSpeed;
        Slide(input);
    }

    public void Fall(float input) {
        if (velocity.y > 0f)
            velocity += Physics2D.gravity * Time.deltaTime;
        else 
            velocity += Physics2D.gravity * gravityMultiplier * Time.deltaTime;
        currentMaxSpeed = maxFallSpeed;
        
        Vector2 desiredVelocity = new Vector2(input, 0f) * currentMaxSpeed;
        float maxSpeedChange = maxAcceleration * Time.deltaTime;
        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
        
        UpdatePosition();
    }

    public void Jump() {
        velocity.y = jumpInitialVelocity;
        UpdatePosition();
    }

    public bool IsFalling() {
        Collider2D[] fallCollisionResults = new Collider2D[1];
        int numCollisions = Physics2D.OverlapBox(
            groundCheckArea.position,
            groundCheckArea.localScale,
            0f,
            walkableFilter,
            fallCollisionResults
        );
        return numCollisions <= 0;
    }

    public bool IsJumping() {
        return velocity.y > 0f;
    }

    public void LandAfterFall() {
        SnapToGround();
        velocity.y = 0f;
    }

    private void SnapToGround() {
        RaycastHit2D[] raycastResults = new RaycastHit2D[1];
        int numRaycastHits = Physics2D.CircleCast(
            predictiveCollider.transform.position,
            predictiveCollider.radius,
            Vector2.down,
            walkableFilter,
            raycastResults,
            predictiveCollider.radius * 2f
        );

        if (numRaycastHits < 1) return;

        player.transform.position += Vector3.down * raycastResults[0].distance;
    }

    public bool IsMoving() {
        return velocity != Vector2.zero;
    }

    private void Slide(float input) {
        RaycastHit2D[] raycastResults = new RaycastHit2D[1];

        int numRaycastHits = Physics2D.CircleCast(
            predictiveCollider.transform.position,
            predictiveCollider.radius * 0.9f,
            Vector2.down,
            collidableFilter,
            raycastResults,
            predictiveCollider.radius * 2f
        );

        if (numRaycastHits <= 0) return;

        CalculateVelocity(input, raycastResults[0]);

        UpdatePosition();
        SnapToGround();
        // CollideAndSlide(circleCastHitResults);
    }

    private void CollideAndSlide(RaycastHit2D[] hitResults) {
        // Interpretation of Kasper Fauerby's Improved Collision detection and Response
        float hitAngle = Mathf.Abs(90 - Vector2.Angle(Vector2.up, hitResults[0].normal));
        if (hitAngle > maxWalkableAngle) {
            velocity.x = 0;
            UpdatePosition();
            return;
        }

        Vector2 reducedVelocity = hitResults[0].fraction * velocity;
        Vector2 remainingVelocity = velocity - reducedVelocity;
        Vector2 hitTangential = Vector2.Perpendicular(hitResults[0].normal);
        // hitTangential's magnitude is essentially 1f
        Vector2 projectedVelocity = Vector2.Dot(remainingVelocity, hitTangential) * hitTangential;

        velocity = reducedVelocity + projectedVelocity.normalized * remainingVelocity.magnitude;
        UpdatePosition();
    }

    private void CalculateVelocity(float input, RaycastHit2D raycastResult) {
        Vector2 desiredVelocity = new Vector2(input, 0f);
        float maxSpeedChange = maxAcceleration * Time.deltaTime;

        Vector2 hitTangential = Vector2.Perpendicular(raycastResult.normal);

        Vector2 desiredVelocityProjected = Vector2.Dot(desiredVelocity, hitTangential) * hitTangential;
        desiredVelocity = desiredVelocityProjected.normalized * currentMaxSpeed;

        Vector2 velocityProjected = Vector2.Dot(velocity, hitTangential) * hitTangential;
        velocity = velocityProjected.normalized * velocity.magnitude;

        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
        velocity.y = Mathf.MoveTowards(velocity.y, desiredVelocity.y, maxSpeedChange);
    }

    private void UpdatePosition() {
        Vector3 displacement = new Vector3(velocity.x, velocity.y, 0f) * Time.deltaTime;
        player.transform.position += displacement;
    }
}
