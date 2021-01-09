using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/*
 *  Central component to refer to other character components
 */

public class Character : MonoBehaviour {

    public CharacterMovement movement;
    public CharacterAppearance appearance;
    public Transform cameraTarget;

    [SerializeField] private GameObject selectMarker;

    private bool isHovered;
    private float doubleLeftClickTimer;
    private const float doubleLeftClickThreshold = 0.5f;

    protected virtual void Awake() {
        movement = GetComponent<CharacterMovement>();
        appearance = GetComponent<CharacterAppearance>();
    }

    protected virtual void Update() {
        if(doubleLeftClickTimer < doubleLeftClickThreshold) {
            doubleLeftClickTimer += Time.deltaTime;
        }
    }

    public virtual void SetHovered(bool hovered) {
        isHovered = hovered;
        PlayerController.instance.characterHovered = hovered ? this : null;
    }

    public virtual void OnLeftClick() {
        if (doubleLeftClickTimer < doubleLeftClickThreshold) OnDoubleLeftClick();
        doubleLeftClickTimer = 0;
    }

    public virtual void OnDoubleLeftClick() {
        CameraController.instance.objectToFollow = cameraTarget;
    }

    public virtual void OnRightClick() {
    }

    void OnMouseEnter() {
        SetHovered(true);
    }

    void OnMouseExit() {
        SetHovered(false);
    }
}
