using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flower : MonoBehaviour {
    private const string ANIMATION_IS_STEPPED_ON = "IsSteppedOn";
    [SerializeField] private Transform hitBoxArea;
    [SerializeField] private Animator flowerAnimator;
    [SerializeField] private ContactFilter2D hitFilter;

    private void Update() {
        Collider2D[] collisionResults = new Collider2D[1];
        int numCollisions = Physics2D.OverlapBox(
            hitBoxArea.position,
            hitBoxArea.localScale,
            0f,
            hitFilter,
            collisionResults
        );

        if (numCollisions > 0) flowerAnimator.SetBool(ANIMATION_IS_STEPPED_ON, true);
    }
}
