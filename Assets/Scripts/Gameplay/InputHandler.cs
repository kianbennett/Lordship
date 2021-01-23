using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// Centralised location for all player input

public class InputHandler : Singleton<InputHandler> 
{
    [SerializeField] private bool allowSpeedUp;

    private Vector2 mouseDelta;
    private Vector2 lastMousePos;
    private Vector2 leftClickInit, rightClickInit;
    private Vector2 leftClickMoveDelta, rightClickMoveDelta;
    private bool leftClickHeld, rightClickHeld;
    private const int minDistForDrag = 5;

    void Update() 
    {
        if(LevelManager.instance.IsPaused) return;

        handleKeys();

        // Update distance mouse has moved since last frame
        mouseDelta = (Vector2) Input.mousePosition - lastMousePos;
        lastMousePos = Input.mousePosition;

        if (!EventSystem.current.IsPointerOverGameObject()) 
        {
            if (Input.GetMouseButtonDown(0)) 
            {
                leftClickHeld = true;
                leftClickInit = Input.mousePosition;
                onLeftClickDown();
            }
            if (Input.GetMouseButtonDown(1)) 
            {
                rightClickHeld = true;
                rightClickInit = Input.mousePosition;
                onRightClickDown();
            }
        }

        if (leftClickHeld) 
        {
            leftClickMoveDelta = (Vector2) Input.mousePosition - leftClickInit;
            onLeftClickHold();
        }

        if (rightClickHeld) 
        {
            rightClickMoveDelta = (Vector2) Input.mousePosition - rightClickInit;
            onRightClickHold();
        }

        if (Input.GetMouseButtonUp(0)) 
        {
            leftClickHeld = false;
            onLeftClickRelease();
        }

        if (Input.GetMouseButtonUp(1)) 
        {
            rightClickHeld = false;
            onRightClickRelease();
        }
    }

    // Keyboard input
    private void handleKeys() 
    {
        if (Input.GetKey(KeyCode.LeftArrow)) CameraController.instance.PanLeft();
        if (Input.GetKey(KeyCode.RightArrow)) CameraController.instance.PanRight();
        if (Input.GetKey(KeyCode.UpArrow)) CameraController.instance.PanForward();
        if (Input.GetKey(KeyCode.DownArrow)) CameraController.instance.PanBackward();

        if(Input.GetKeyDown(KeyCode.S)) 
        {
            PlayerController.instance.playerCharacter.Movement.StopMoving();
        }

        // Cycle between selected characters
        if (Input.GetKeyDown(KeyCode.Space)) 
        {
            if(CameraController.instance.objectToFollow == PlayerController.instance.playerCharacter.CameraTarget) 
            {
                CameraController.instance.objectToFollow = null;
            } 
            else 
            {
                CameraController.instance.objectToFollow = PlayerController.instance.playerCharacter.CameraTarget;
            }
        }

        if(Input.GetKeyDown(KeyCode.Escape)) 
        {
            if(HUD.instance.optionsMenu.gameObject.activeSelf) 
            {
                HUD.instance.optionsMenu.SetActive(false);
                AudioManager.instance.PlayButtonClick();
            } 
            else 
            {
                LevelManager.instance.TogglePaused();
            }
        }

        // Use f8 to speed the game up (only used for testing)
        if(allowSpeedUp) 
        {
            if(Input.GetKeyDown(KeyCode.F8) && !LevelManager.instance.IsPaused) 
            {
                Time.timeScale = 4;
            }
            if(Input.GetKeyUp(KeyCode.F8)) 
            {
                Time.timeScale = LevelManager.instance.IsPaused ? 0 : 1;
            }
        }
    } 

    private void onLeftClickDown() 
    {
        CameraController.instance.Grab();
    }

    private void onLeftClickHold() 
    {
        if (leftClickMoveDelta.magnitude >= minDistForDrag && mouseDelta.magnitude > 0) 
        {
            CameraController.instance.Pan();
        }
    }

    private void onLeftClickRelease() 
    {
        CameraController.instance.Release();
        if (leftClickMoveDelta.magnitude == 0) 
        {
            // PlayerController.instance.DeselectSelectedCharacter();
            if (PlayerController.instance.characterHovered && !PlayerController.instance.IsInDialogue) 
            {
                PlayerController.instance.characterHovered.OnLeftClick();
            }
        }
    }

    private void onRightClickDown() {}

    private void onRightClickHold() {}

    private void onRightClickRelease() 
    {
        if (rightClickMoveDelta.magnitude < minDistForDrag && !PlayerController.instance.IsInDialogue) 
        {
            if (PlayerController.instance.characterHovered) 
            {
                PlayerController.instance.characterHovered.OnRightClick();
            } 
            else 
            {
                if(CameraController.instance.GetMousePointOnGround(out Vector3 point)) 
                {
                    PlayerController.instance.MoveCharacter(point, true);
                }
            }
        }
    }
}
