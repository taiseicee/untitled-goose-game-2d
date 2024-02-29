using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bench : MonoBehaviour {
    [SerializeField] private BoxCollider2D benchCollider;
    [SerializeField] private Transform enableCheckArea;
    [SerializeField] private Transform disableCheckArea;
    [SerializeField] private ContactFilter2D canStepFilter;

    void Start() {
        benchCollider.enabled = false;
    }

    void Update() {
        Collider2D[] disableCheckAreaResults = new Collider2D[1];
        int numDisableCheckAreaCollisions = Physics2D.OverlapBox(
            disableCheckArea.position,
            disableCheckArea.localScale,
            0f,
            canStepFilter,
            disableCheckAreaResults
        );

        if (numDisableCheckAreaCollisions > 0) {
            benchCollider.enabled = false;
            return;
        }


        Collider2D[] enableCheckAreaResults = new Collider2D[1];
        int numEnableCheckAreaCollisions = Physics2D.OverlapBox(
            enableCheckArea.position,
            enableCheckArea.localScale,
            0f,
            canStepFilter,
            enableCheckAreaResults
        );

        if (numEnableCheckAreaCollisions <= 0) {
            benchCollider.enabled = false;
            return;
        }

        benchCollider.enabled = true;
    }
}
