using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectableObject : MonoBehaviour {

    protected bool isHovered, isSelected;
    private float doubleLeftClickTimer;
    private const float doubleLeftClickThreshold = 0.5f;

    protected virtual void Awake() {
    }

    protected virtual void Update() {
        if(doubleLeftClickTimer < doubleLeftClickThreshold) doubleLeftClickTimer += Time.deltaTime;
    }

    void OnMouseEnter() {
        SetHovered(true);
    }

    void OnMouseExit() {
        SetHovered(false);
    }

    public virtual void SetSelected(bool selected) {
        isSelected = selected;
    }

    public virtual void SetHovered(bool hovered) {
        isHovered = hovered;
        PlayerController.instance.selectableHovered = hovered ? this : null;
    }

    public virtual void OnLeftClick() {
        if (doubleLeftClickTimer < doubleLeftClickThreshold) OnDoubleLeftClick();
        doubleLeftClickTimer = 0;
    }

    public virtual void OnDoubleLeftClick() {
        // TODO: Centre camera on object (and follow)
    }

    public virtual void OnRightClick() {
    }
}
