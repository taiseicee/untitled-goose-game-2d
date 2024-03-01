using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour {
    [SerializeField] private CircleCollider2D colliderPlayer;
    [SerializeField] private Rigidbody2D ballBody;
    [SerializeField] private CircleCollider2D colliderBall;
    [SerializeField] private ContactFilter2D ballKickFilter;
    [SerializeField] private float hitBuffer = 0.05f;
    [SerializeField] private float kickForce = 3f;
    void Start() {

    }

    void Update() {
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
}
