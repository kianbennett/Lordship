using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/*
 *  Interface between input and player actions
 */

public class PlayerController : Singleton<PlayerController> {

    [SerializeField] private GameObject moveMarkerPrefab;

    [ReadOnly] public Character selectedCharacter;
    [ReadOnly] public SelectableObject selectableHovered;

    protected override void Awake() {
        base.Awake();
    }

    public void SelectCharacter(Character character) {
        character.SetSelected(true);
        if(selectedCharacter != null) DeselectSelectedCharacter();
        selectedCharacter = character;
    }

    public void DeselectCharacter(Character character) {
        character.SetSelected(false);
    }

    public void DeselectSelectedCharacter() {
        if(selectedCharacter != null) {
            DeselectCharacter(selectedCharacter);
        }
    }

    // Move a group of characters around a target position so they don't all end up at the same point
    public void MoveCharacters(Vector3 target, bool showMarker) {
        if (selectedCharacter == null) return;

        selectedCharacter.movement.SetRunning(true);
        selectedCharacter.movement.MoveToPoint(target, true);

        // Spawn marker
        if (showMarker) Instantiate(moveMarkerPrefab, target, Quaternion.identity);
    }
}
