using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputHandler : Singleton<InputHandler> {

    private Vector2 mouseDelta;
    private Vector2 lastMousePos;
    private Vector2 leftClickInit, rightClickInit;
    private Vector2 leftClickMoveDelta, rightClickMoveDelta;
    private bool leftClickHeld, rightClickHeld;
    private const int minDistForDrag = 5;

    void Update() {
        handleKeys();

        // Update distance mouse has moved since last frame
        mouseDelta = (Vector2) Input.mousePosition - lastMousePos;
        lastMousePos = Input.mousePosition;

        if (!EventSystem.current.IsPointerOverGameObject()) {
            if (Input.GetMouseButtonDown(0)) {
                leftClickHeld = true;
                leftClickInit = Input.mousePosition;
                onLeftClickDown();
            }
            if (Input.GetMouseButtonDown(1)) {
                rightClickHeld = true;
                rightClickInit = Input.mousePosition;
                onRightClickDown();
            }
        }

        if (leftClickHeld) {
            float d = leftClickMoveDelta.magnitude;
            leftClickMoveDelta = (Vector2) Input.mousePosition - leftClickInit;

            // The first frame the mouse is dragged
            if (d == 0 && leftClickMoveDelta.magnitude != 0) onLeftClickDragStart();
            onLeftClickHold();
        }

        if (rightClickHeld) {
            rightClickMoveDelta = (Vector2) Input.mousePosition - rightClickInit;
            onRightClickHold();
        }

        if (Input.GetMouseButtonUp(0)) {
            leftClickHeld = false;
            onLeftClickRelease();
        }

        if (Input.GetMouseButtonUp(1)) {
            rightClickHeld = false;
            onRightClickRelease();
        }
    }

    private void handleKeys() {
        if (Input.GetKey(KeyCode.LeftArrow)) CameraController.instance.PanLeft();
        if (Input.GetKey(KeyCode.RightArrow)) CameraController.instance.PanRight();
        if (Input.GetKey(KeyCode.UpArrow)) CameraController.instance.PanForward();
        if (Input.GetKey(KeyCode.DownArrow)) CameraController.instance.PanBackward();

        // Cycle between selected characters
        if (Input.GetKeyDown(KeyCode.Space)) {
            if(CameraController.instance.objectToFollow == PlayerController.instance.selectedCharacter.cameraTarget) {
                CameraController.instance.objectToFollow = null;
            } else {
                CameraController.instance.objectToFollow = PlayerController.instance.selectedCharacter.cameraTarget;
            }
        }

        // if(Input.GetKeyDown(KeyCode.Escape)) {
        //     if(PlayerController.instance.IsInDialogue) {
        //         PlayerController.instance.ExitDialogue();
        //     }
        // }
    }

    private void onLeftClickDown() {
        // Delect the selected character unless the player is still hovering it
        if(PlayerController.instance.selectableHovered != PlayerController.instance.selectedCharacter) {
            PlayerController.instance.DeselectSelectedCharacter();
        }
    }

    private void onLeftClickDragStart() {
    }

    private void onLeftClickHold() {
    }

    private void onLeftClickRelease() {
        if (leftClickMoveDelta.magnitude == 0) {
            // PlayerController.instance.DeselectSelectedCharacter();
            if (PlayerController.instance.selectableHovered && !PlayerController.instance.IsInDialogue) {
                PlayerController.instance.selectableHovered.OnLeftClick();
            }
        }
    }

    private void onRightClickDown() {
        CameraController.instance.Grab();
    }

    private void onRightClickHold() {
        if (rightClickMoveDelta.magnitude >= minDistForDrag && mouseDelta.magnitude > 0) {
            CameraController.instance.Pan();
        }
    }

    private void onRightClickRelease() {
        CameraController.instance.Release();

        if (rightClickMoveDelta.magnitude < minDistForDrag && !PlayerController.instance.IsInDialogue) {
            if (PlayerController.instance.selectableHovered) {
                PlayerController.instance.selectableHovered.OnRightClick();
            } else {
                if(CameraController.instance.GetMousePointOnGround(out Vector3 point)) {
                    PlayerController.instance.MoveCharacter(point, true);
                }
            }
        }
    }
}
