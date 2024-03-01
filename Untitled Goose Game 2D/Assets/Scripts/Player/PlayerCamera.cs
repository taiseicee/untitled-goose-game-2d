using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour {
    [SerializeField, Range(0f, 1f)] private float cameraMoveSpeed = 0.05f;
    [SerializeField] private SpriteRenderer worldBoundsObject;
    private float cameraHorizontalOffset;
    private float offsetDirection = 1f;
    private Bounds cameraBounds;
    private void Start() {
        Camera playerCamera = GetComponent<Camera>();
        Bounds worldBounds = worldBoundsObject.bounds;

        float viewportHeight = playerCamera.orthographicSize;
        float viewportWidth = viewportHeight * playerCamera.aspect;

        float cameraMinX = worldBounds.min.x + viewportWidth;
        float cameraMaxX = worldBounds.max.x - viewportWidth;

        float cameraMinY = worldBounds.min.y + viewportHeight;
        float cameraMaxY = worldBounds.max.y - viewportHeight;

        cameraBounds = new Bounds();
        cameraBounds.SetMinMax(
            new Vector3(cameraMinX, cameraMinY, -transform.position.z),
            new Vector3(cameraMaxX, cameraMaxY, transform.position.z)
        );
        
    }

    private void Awake() {
        cameraHorizontalOffset = transform.localPosition.x;
    }

    private void Update() {
        ChangeCameraDirection();
    }

    private void LateUpdate() {
        transform.position = GetPositionWithinBounds();
    }

    public void SetDirection(float direction) {
        offsetDirection = direction;
    }

    private void ChangeCameraDirection() {
        float newCameraHorizontalOffset = Mathf.MoveTowards(
            transform.localPosition.x, 
            offsetDirection * cameraHorizontalOffset, 
            cameraMoveSpeed
        );

        Vector3 updatedCameraPosition = new Vector3(
            newCameraHorizontalOffset,
            transform.localPosition.y,
            transform.localPosition.z
        );

        transform.localPosition = updatedCameraPosition;
    }

    private Vector3 GetPositionWithinBounds() {
        return new Vector3(
            Mathf.Clamp(transform.position.x, cameraBounds.min.x, cameraBounds.max.x),
            Mathf.Clamp(transform.position.y, cameraBounds.min.y, cameraBounds.max.y),
            transform.localPosition.z
        );
    }

}
