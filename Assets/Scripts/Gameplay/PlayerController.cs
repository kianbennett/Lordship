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
    [ReadOnly] public NPC npcFollowing, npcSpeaking;
    public Character playerCharacter;

    public bool IsInDialogue { get { return npcSpeaking; } }

    protected override void Awake() {
        base.Awake();
    }

    void Update() {
        // If following an npc and they get within a certain range, start dialogue
        if(npcFollowing != null && playerCharacter.movement.FollowedCharacterDistance() < 3) {
            startDialogue();
        }
    }

    public void SelectCharacter(Character character) {
        if(character != playerCharacter) return;
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
            selectedCharacter = null;
        }
    }

    // Move a group of characters around a target position so they don't all end up at the same point
    public void MoveCharacter(Vector3 target, bool showMarker) {
        if (selectedCharacter != playerCharacter) return;

        selectedCharacter.movement.MoveToPoint(target, true);

        // Spawn marker
        if (showMarker) Instantiate(moveMarkerPrefab, target, Quaternion.identity);

        if(selectedCharacter == playerCharacter) {
            CancelFollowing();
        }
    }

    public void TalkToNpc(NPC npc) {
        if (selectedCharacter != playerCharacter) return;
        npcFollowing = npc;
        selectedCharacter.movement.FollowCharacter(npc, true);
    }

    public void CancelFollowing() {
        npcFollowing = null;
        playerCharacter.movement.CancelFollowing();
    }

    private void startDialogue() {
        npcSpeaking = npcFollowing;
        CancelFollowing();
        playerCharacter.movement.SetSpeaking(npcSpeaking);
        npcSpeaking.movement.SetSpeaking(playerCharacter);
        // HUD.instance.screenFader.FadeOut(delegate {
        //     HUD.instance.screenFader.FadeIn();
            CameraController.instance.SetInDialogue(npcSpeaking, playerCharacter);    
            DialogueSystem.instance.DisplayBeat(1, true);
        // });
    }

    public void ExitDialogue() {
        NPC speaking = npcSpeaking;
        npcSpeaking = null;
        DialogueSystem.instance.ExitDialogue();
        // HUD.instance.screenFader.FadeOut(delegate {
            // HUD.instance.screenFader.FadeIn();
            speaking.movement.SetSpeaking(null);
            playerCharacter.movement.SetSpeaking(null);
            CameraController.instance.CancelDialogue();
        // });
    }
}
