using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/*
 *  Central component to refer to other character components
 */

public class Character : SelectableObject {

    public CharacterMovement movement;
    public CharacterAppearance appearance;

    [SerializeField] private string charName;
    [SerializeField] private GameObject selectMarker;

    protected override void Awake() {
        base.Awake();
        movement = GetComponent<CharacterMovement>();
        appearance = GetComponent<CharacterAppearance>();
    }

    public override void SetHovered(bool hovered) {
        base.SetHovered(hovered);
    }

    public override void SetSelected(bool selected) {
        base.SetSelected(selected);
        selectMarker.SetActive(selected);
    }

    public override void OnLeftClick() {
        base.OnLeftClick();
        PlayerController.instance.SelectCharacter(this);
    }

    public void Destroy() {
        Destroy(gameObject);
        if (PlayerController.instance.selectedCharacters.Contains(this)) {
            PlayerController.instance.selectedCharacters.Remove(this);
        }
        if (PlayerController.instance.selectableHovered == this) {
            PlayerController.instance.selectableHovered = null;
        }
    }
}
