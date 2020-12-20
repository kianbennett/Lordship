using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterActionHandler : MonoBehaviour {

    public readonly List<CharacterAction> Actions = new List<CharacterAction>();

    void Update() {
        evaluateActions();
    }

    // If the first action has finished, remove it and start the next in the list
    private void evaluateActions() {
        if (Actions.Count > 0) {
            bool complete = Actions[0].HasFinished;
            if (complete) {
                Actions.RemoveAt(0);
                if (Actions.Count > 0) {
                    Actions[0].StartAction();
                }
            }
        }
    }

    public void AddAction(CharacterAction action) {
        Actions.Add(action);
        if (Actions.Count == 1) {
            action.StartAction();
        }
    }

    // Cancel and remove all actions that can be cancelled (returns success)
    public bool ClearActions() {
        if (Actions.Count == 0) return true;
        if (IsInUnskibbableAction()) return false;
        List<CharacterAction> actionsToRemove = new List<CharacterAction>();
        foreach (CharacterAction action in Actions) {
            if (action.CanCancel) {
                action.StopAction();
                actionsToRemove.Add(action);
            }
        }
        Actions.RemoveAll(o => o.CanCancel);
        return true;
    }

    public bool IsInUnskibbableAction() {
        return Actions.Count > 0 && !Actions[0].CanCancel;
    }
}
