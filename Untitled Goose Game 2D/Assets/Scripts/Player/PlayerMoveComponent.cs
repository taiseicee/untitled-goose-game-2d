using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveComponent : MonoBehaviour {
    private static string LAYER_MASK_GROUND = "Ground";
    [SerializeField, Range(0f, 30f)] private float maxWalkSpeed = 5f;
    [SerializeField, Range(0f, 30f)] private float maxRunSpeed = 10f;
    [SerializeField, Range(0f, 30f)] private float maxFallSpeed = 3f;
    [SerializeField, Range(0f, 25f)] private float maxAcceleration = 12f;
    [SerializeField, Range(0f, 100f)] private float jumpInitialVelocity = 8f;
    [SerializeField, Range(0f, 1f)] private float gravityMultiplier = 0.2f;

    private float currentMaxSpeed;
    private Vector2 velocity;
    private Player player;
    private float fallDistanceCheck = 0f;

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
        velocity.y += jumpInitialVelocity;
        UpdatePosition();
    }

    public bool IsFalling() {
        //BoxCast(Vector2 origin, Vector2 size, float angle, Vector2 direction, float distance)
        bool isTouchingGround = Physics2D.BoxCast(
            player.transform.position + (0.5f * Vector3.up),
            new Vector2(1f, 1f),
            0f,
            Vector2.down,
            fallDistanceCheck,
            LayerMask.GetMask(LAYER_MASK_GROUND)
        );
        return !isTouchingGround;
    }

    public bool IsMoving() {
        return velocity != Vector2.zero;
    }

    private void CollideAndSlide(float input) {
        CalculateVelocity(input);
        velocity.y = 0f;
        // TODO: Collision Detection
        // RaycastHit hit;
        // bool collisionDetected = Physics.SphereCast(transform.position, 0.5f, desiredVelocity, out hit);

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
