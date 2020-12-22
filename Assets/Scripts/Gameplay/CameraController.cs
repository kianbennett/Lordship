using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : Singleton<CameraController> {

    [SerializeField] private new Camera camera;
    [SerializeField] private Transform cameraContainer;

    [SerializeField] private float panSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float zoomSpeed;
    [SerializeField] private float minCameraDist, maxCameraDist;
    [SerializeField] private bool panSmoothing;
    [SerializeField] private float panSmoothingValue;

    // TargetPosition is separate from Target.position to allow for interpolated camera movement
    [SerializeField, ReadOnly] private Vector3 targetPosition;

    private bool isRotating, isGrabbing;
    // private Vector3 localCameraOffset;
    private float cameraDist;
    private Vector3 worldSpaceGrab, worldSpaceGrabLast;

    // A plane that a ray cast from a mouse position can collide with
    private Plane raycastPlane;
    // Position of mouse when the left mouse button is pressed
    private Vector2 grabbedMousePos;

    protected override void Awake() {
        base.Awake();

        // localCameraOffset = camera.transform.localPosition;
        cameraDist = camera.transform.localPosition.z;
        targetPosition = transform.position;

        raycastPlane = new Plane(Vector3.up, Vector3.zero);
    }

    void LateUpdate() {
        // TODO: Move this to InputHandler
        // Hold middle mouse to rotate
        isRotating = Input.GetMouseButton(2);
        bool pivot = Input.GetKey(KeyCode.LeftShift);

        if (isRotating) {
            float rot = Input.GetAxis("Mouse X") * rotationSpeed;
            // TODO: RotateAround not working properly
            if (pivot) transform.RotateAround(camera.transform.position, Vector3.up, rot);
                else transform.Rotate(Vector3.up, rot);
        }

        // Only zoom if the mouse isn't over a UI element to avoid zooming when scrolling
        if (!EventSystem.current.IsPointerOverGameObject()) {
            float scrollDelta = -Input.mouseScrollDelta.y;
            zoom(scrollDelta * zoomSpeed * Time.deltaTime);
        }

        transform.position = targetPosition;
        // Lerp towards camera zoom dist
        camera.transform.localPosition = new Vector3(0, 0, Mathf.Lerp(camera.transform.localPosition.z, cameraDist, Time.deltaTime * 10));

        // cameraContainer.position = panSmoothing ? Vector3.Lerp(cameraContainer.position, targetPosition, Time.deltaTime * panSmoothingValue) : targetPosition;
        // camera.transform.localPosition = Vector3.Lerp(camera.transform.localPosition, localCameraOffset, Time.deltaTime * 10);
    }

    private void move(Vector3 delta) {
        targetPosition += delta;
    }

    public void PanLeft() {
        if (isGrabbing) return;
        move(-transform.right * panSpeed * Time.deltaTime);
    }

    public void PanRight() {
        if (isGrabbing) return;
        move(transform.right * panSpeed * Time.deltaTime);
    }

    public void PanForward() {
        if (isGrabbing) return;
        move(transform.forward * panSpeed * Time.deltaTime);
    }

    public void PanBackward() {
        if (isGrabbing) return;
        move(-transform.forward * panSpeed * Time.deltaTime);
    }

    private void setAbsolutePosition(Vector3 pos) {
        targetPosition = pos;
        transform.position = pos;
        worldSpaceGrab = Vector3.zero;
        worldSpaceGrabLast = Vector3.zero;
    }

    // Gets the ground position where the mouse right clicks
    public void Grab() {
        if(GetMousePointOnGround(out Vector3 point)) {
            worldSpaceGrab = point;
            worldSpaceGrabLast = worldSpaceGrab;
            isGrabbing = true;
            grabbedMousePos = (Vector2) Input.mousePosition;
        }
    }

    // Called when the mouse moves while grabbing - find the new position and translate the camera in the opposite direction
    public void Pan() {
        if(GetMousePointOnGround(out Vector3 point)) {
            worldSpaceGrab = point;
            Vector3 delta = worldSpaceGrab - worldSpaceGrabLast;
            move(-delta);
        }
    }

    public void Release() {
        isGrabbing = false;
    }

    private void zoom(float dist) {
        // zoom faster the further out the camera is
        dist *= (cameraDist / 4);

        if(cameraDist + dist > minCameraDist && cameraDist + dist < maxCameraDist) {
            cameraDist += dist;
        }
    }

    // Gets the point at which the mouse position ray hits a plane at y = 0
    public bool GetMousePointOnGround(out Vector3 point) {
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        bool hit = raycastPlane.Raycast(ray, out float distance);
        point = ray.GetPoint(distance);
        return hit;
    }
}
