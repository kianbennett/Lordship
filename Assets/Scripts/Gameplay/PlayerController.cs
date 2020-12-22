using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/*
 *  Interface between input and player actions
 */

public class PlayerController : Singleton<PlayerController> {

    [SerializeField] private GameObject moveMarkerPrefab;

    [HideInInspector] public List<Character> selectedCharacters;
    [ReadOnly] public SelectableObject selectableHovered;

    protected override void Awake() {
        base.Awake();
    }

    public void SelectCharacter(Character character) {
        if(!selectedCharacters.Contains(character)) {
            selectedCharacters.Add(character);
        }
        character.SetSelected(true);
    }

    public void DeselectCharacter(Character character) {
        if (selectedCharacters.Contains(character)) {
            selectedCharacters.Remove(character);
        }
        character.SetSelected(false);
    }

    public void DeselectAllCharacters() {
        foreach(Character character in selectedCharacters) {
            character.SetSelected(false);
        }
        selectedCharacters.Clear();
    }

    // Move a group of characters around a target position so they don't all end up at the same point
    public void MoveCharacters(Vector3 target, bool showMarker) {
        if (selectedCharacters.Count == 0) return;

        // Clear actions
        foreach (Character character in selectedCharacters) {
            // character.actionHandler.ClearActions();
            character.movement.MoveToPoint(target, false);
        }

        // Spawn marker
        if (showMarker) Instantiate(moveMarkerPrefab, target, Quaternion.identity);
    }
}
