using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour {
    [SerializeField] private CircleCollider2D colliderPlayer;
    [SerializeField] private Rigidbody2D ballBody;
    [SerializeField] private CircleCollider2D colliderBall;
    [SerializeField] private ContactFilter2D ballKickFilter;
    [SerializeField] private SpriteRenderer worldBoundsObject;
    [SerializeField] private float hitBuffer = 0.05f;
    [SerializeField] private float kickForce = 3f;
    private Bounds ballBounds;
    private void Start() {
        Bounds worldBounds = worldBoundsObject.bounds;

        float ballRadius = transform.localScale.x / 2f;

        float ballMinX = worldBounds.min.x + ballRadius;
        float ballMaxX = worldBounds.max.x - ballRadius;

        float ballMinY = worldBounds.min.y + ballRadius;
        float ballMaxY = worldBounds.max.y - ballRadius;

        ballBounds = new Bounds();
        ballBounds.SetMinMax(
            new Vector3(ballMinX, ballMinY, -transform.position.z),
            new Vector3(ballMaxX, ballMaxY, transform.position.z)
        );
    }

    private void Update() {
        StayInBounds();

        Vector3 playerPosition = colliderPlayer.transform.position;
        Vector3 repulsionDirection = (transform.position - playerPosition).normalized;

        RaycastHit2D[] raycastResults = new RaycastHit2D[1];
        int numRaycastHits = Physics2D.CircleCast(
            transform.position,
            colliderBall.radius + hitBuffer,
            repulsionDirection,
            ballKickFilter,
            raycastResults,
            0f
        );

        if (numRaycastHits <= 0) return;
        ballBody.AddForce(repulsionDirection * kickForce, ForceMode2D.Impulse);

    }

    private void StayInBounds() {
        if (transform.position.x < ballBounds.min.x) ballBody.AddForce(Vector2.right * kickForce, ForceMode2D.Impulse);
        if (transform.position.x > ballBounds.max.x) {
            ballBody.AddForce(Vector2.left * kickForce, ForceMode2D.Impulse);
        }
    }
}
