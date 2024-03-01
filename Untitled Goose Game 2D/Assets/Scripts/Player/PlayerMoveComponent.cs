using System;
using UnityEngine;

public class PlayerMoveComponent : MonoBehaviour {
    [SerializeField, Range(0f, 30f)] private float maxWalkSpeed = 5f;
    [SerializeField, Range(0f, 30f)] private float maxRunSpeed = 10f;
    [SerializeField, Range(0f, 30f)] private float maxFallSpeed = 3f;
    [SerializeField, Range(0f, 25f)] private float maxAcceleration = 12f;
    [SerializeField, Range(0f, 50f)] private float jumpInitialVelocity = 8f;
    [SerializeField, Range(0f, 1f)] private float gravityMultiplier = 0.2f;
    [SerializeField, Range(0f, 90f)] private float maxWalkableAngle = 60f;
    [SerializeField] private SpriteRenderer worldBoundsObject;
    [SerializeField] private Transform groundCheckArea;
    [SerializeField] private ContactFilter2D walkableFilter;
    [SerializeField] private CircleCollider2D predictiveCollider;
    [SerializeField] private ContactFilter2D collidableFilter;

    private float currentMaxSpeed;
    private Vector2 velocity;
    private Player player;
    private float snapBuffer = 0.01f;
    private int maxCollideAndSlideDepth = 3;
    private Rect allowedArea;

    private void Awake() {
        Bounds worldBounds = worldBoundsObject.bounds;

        float WorldBoundsLength = worldBounds.max.x - worldBounds.min.x;
        float WorldBoundsHeight = worldBounds.max.y - worldBounds.min.y;

        allowedArea = new Rect(
            worldBounds.min.x + predictiveCollider.radius,
            worldBounds.min.y + predictiveCollider.radius,
            WorldBoundsLength - predictiveCollider.radius * 2f,
            WorldBoundsHeight - predictiveCollider.radius * 2f
        );
    }

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
        Vector2 positionChange = velocity * Time.deltaTime;
        UpdatePosition(positionChange);
    }

    public void Jump() {
        velocity.y = jumpInitialVelocity;
        Vector2 positionChange = velocity * Time.deltaTime;
        UpdatePosition(positionChange);
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
            predictiveCollider.radius * 2f + snapBuffer
        );

        if (numRaycastHits < 1) return;

        player.transform.position += Vector3.down * (raycastResults[0].distance - snapBuffer);
    }

    public bool IsMoving() {
        return velocity != Vector2.zero;
    }

    private void Slide(float input) {
        CalculateVelocity(input);

        Vector2 positionChange = CollideAndSlide(velocity * Time.deltaTime, predictiveCollider.transform.position);
        UpdatePosition(positionChange);
        SnapToGround();
    }

    private Vector2 CollideAndSlide(Vector2 positionChange, Vector3 startPosition, int depth = 0) {
        if (positionChange == Vector2.zero) return Vector2.zero;
        if (depth >= maxCollideAndSlideDepth) return Vector2.zero;

        RaycastHit2D[] raycastResults = new RaycastHit2D[1];
        int numRaycastHits = Physics2D.CircleCast(
            startPosition,
            predictiveCollider.radius,
            positionChange.normalized,
            collidableFilter,
            raycastResults,
            positionChange.magnitude + snapBuffer
        );

        if (numRaycastHits == 0) return positionChange;

        Vector2 reducedVelocity = (raycastResults[0].distance - snapBuffer) * positionChange.normalized;

        float hitAngle = Vector2.Angle(Vector2.up, raycastResults[0].normal);
        bool didHitSteepSlope = hitAngle > maxWalkableAngle;
        if (didHitSteepSlope) {
            velocity = Vector2.zero;
            return reducedVelocity;
        }

        Vector2 remainingVelocity = positionChange - reducedVelocity;
        Vector2 hitTangential = Vector2.Perpendicular(raycastResults[0].normal);
        // hitTangential's magnitude is essentially 1f
        Vector2 projectedVelocity = Vector2.Dot(remainingVelocity, hitTangential) * hitTangential;

        Vector3 updatedPosition = new Vector3(
            startPosition.x + reducedVelocity.x,
            startPosition.y + reducedVelocity.y,
            startPosition.z
        );
        return reducedVelocity + CollideAndSlide(projectedVelocity, updatedPosition, depth + 1);
    }

    private void CalculateVelocity(float input) {
        Vector2 desiredVelocity = new Vector2(input, 0f) * currentMaxSpeed;
        float speedChange = maxAcceleration * Time.deltaTime;

        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, speedChange);
        velocity.y = Mathf.MoveTowards(velocity.y, desiredVelocity.y, speedChange);
    }

    private void UpdatePosition(Vector2 positionChange) {
        Vector3 displacement = new Vector3(positionChange.x, positionChange.y, 0f);
        Vector3 updatedPosition = player.transform.position + displacement;
        if (!allowedArea.Contains(updatedPosition)) {
            velocity.x = 0f;
            updatedPosition.x = player.transform.position.x;
        }
        player.transform.position = updatedPosition;
    }
}
