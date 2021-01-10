using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.PostProcessing;

public class CameraController : Singleton<CameraController> {

    [Header("Transforms")]
    [ReadOnly] public Transform objectToFollow;

    public new Camera camera;
    [SerializeField] private Transform cameraContainer;
    [SerializeField] private Transform dialogueCameraPos;
    [SerializeField] private PostProcessVolume postProcessVolume;
    [SerializeField] private PostProcessLayer postProcessLayer;

    [Header("Parameters")]
    [SerializeField] private float panSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float zoomSpeed;
    [SerializeField] private float minCameraDist, maxCameraDist;
    [SerializeField] private bool panSmoothing;
    [SerializeField] private float panSmoothingValue;

    // TargetPosition is separate from Target.position to allow for interpolated camera movement
    [SerializeField, ReadOnly] private Vector3 targetPosition;

    private bool isRotating, isGrabbing, inDialogue;
    // private Vector3 localCameraOffset;
    private float cameraDist;
    private Vector3 worldSpaceGrab, worldSpaceGrabLast;

    // A plane that a ray cast from a mouse position can collide with
    private Plane raycastPlane;
    // Position of mouse when the left mouse button is pressed
    // private Vector2 grabbedMousePos;
    private float rotBeforeDialogue;
    // Store reference to objects that block the characters and get hidden during dialogue so they can be reactivated
    private List<GameObject> objectsHiddenInDialogue;

    protected override void Awake() {
        base.Awake();

        // localCameraOffset = camera.transform.localPosition;
        cameraDist = camera.transform.localPosition.z;
        targetPosition = transform.position;

        raycastPlane = new Plane(Vector3.up, Vector3.zero);
        objectsHiddenInDialogue = new List<GameObject>();
    }

    void LateUpdate() {
        if(!inDialogue) {
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

            // Lerp towards camera zoom dist
            camera.transform.localPosition = new Vector3(0, 0, Mathf.Lerp(camera.transform.localPosition.z, cameraDist, Time.deltaTime * 10));

            if(objectToFollow != null) {
                targetPosition = objectToFollow.transform.position;
            }
            transform.position = targetPosition;
        }

        // cameraContainer.position = panSmoothing ? Vector3.Lerp(cameraContainer.position, targetPosition, Time.deltaTime * panSmoothingValue) : targetPosition;
        // camera.transform.localPosition = Vector3.Lerp(camera.transform.localPosition, localCameraOffset, Time.deltaTime * 10);
    }

    private void move(Vector3 delta) {
        if(inDialogue) return; // Can't move the camera when in dialogue
        targetPosition += delta;
        objectToFollow = null;
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

    public void SetPositionImmediate(Vector3 pos) {
        targetPosition = pos;
        transform.position = pos;
        worldSpaceGrab = Vector3.zero;
        worldSpaceGrabLast = Vector3.zero;
    }

    public void ResetCameraDist() {
        camera.transform.localPosition = Vector3.forward * cameraDist;
    }

    // Gets the ground position where the mouse right clicks
    public void Grab() {
        if(GetMousePointOnGround(out Vector3 point)) {
            worldSpaceGrab = point;
            worldSpaceGrabLast = worldSpaceGrab;
            isGrabbing = true;
            // grabbedMousePos = (Vector2) Input.mousePosition;
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

    public void SetInDialogue(Character characterFocus, Character characterSpeaking) {
        transform.position = characterFocus.transform.position;
        rotBeforeDialogue = transform.rotation.eulerAngles.y;
        transform.rotation = Quaternion.LookRotation(characterFocus.transform.position - characterSpeaking.transform.position, Vector3.up);
        camera.transform.localPosition = dialogueCameraPos.localPosition;
        camera.transform.localRotation = dialogueCameraPos.localRotation;
        inDialogue = true;
        SetPostProcessingEffectEnabled<DepthOfField>(true);

        // Cast rays to each character's feet and head to check if anything is blocking them from view of the camera
        // Keep ray directions separate to rays as once you create a ray it normalises the direction
        Vector3[] rayDirections = new Vector3[] {
            characterFocus.transform.position - camera.transform.position,
            characterSpeaking.transform.position - camera.transform.position,
            characterFocus.transform.position + Vector3.up * 2f - camera.transform.position,
            characterSpeaking.transform.position + Vector3.up * 2f - camera.transform.position
        };

        for(int i = 0; i < rayDirections.Length; i++) {
            // Move each ray back incase it's inside a collider
            Ray ray = new Ray(camera.transform.position, rayDirections[i]);
            ray.origin -= rayDirections[i] * 5; 

            RaycastHit[] hits = Physics.RaycastAll(ray, rayDirections[i].magnitude * 6, LayerMask.GetMask("Scenery"));
            foreach(RaycastHit hit in hits) {
                hit.collider.gameObject.SetActive(false);
                objectsHiddenInDialogue.Add(hit.collider.gameObject);
            }
        }
    }

    public void CancelDialogue() {
        if(!inDialogue) return;
        // Restore the camera Y rotation
        transform.rotation = Quaternion.Euler(Vector3.up * rotBeforeDialogue);
        camera.transform.localPosition = Vector3.forward * cameraDist;
        camera.transform.localRotation = Quaternion.identity;
        inDialogue = false;
        SetPostProcessingEffectEnabled<DepthOfField>(false);

        foreach(GameObject gameObject in objectsHiddenInDialogue) {
            gameObject.SetActive(true);
        }
        objectsHiddenInDialogue.Clear();
    }

    public void SetPostProcessingEffectEnabled<T>(bool enabled) where T : PostProcessEffectSettings {
        postProcessVolume.profile.TryGetSettings(out T settings);
        if(settings != null) {
            settings.enabled.value = enabled;
            settings.active = enabled;
        }
    }

    public void SetAntialiasingEnabled(bool enabled) {
        postProcessLayer.antialiasingMode = enabled ? PostProcessLayer.Antialiasing.FastApproximateAntialiasing : PostProcessLayer.Antialiasing.None;
    }
}
