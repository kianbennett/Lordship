using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAction {

    public bool HasFinished;
    public bool CanCancel;

    protected Character owner;
    protected List<Coroutine> coroutines = new List<Coroutine>();

    public CharacterAction(Character owner) {
        this.owner = owner;
        CanCancel = true;
    }

    public void StartAction() {
        addCoroutine(owner.StartCoroutine(execute()));
    }

    public void StopAction() {
        if (!CanCancel) return;
        foreach(Coroutine coroutine in coroutines) if(coroutine != null) owner.StopCoroutine(coroutine);
        onCancel();
        HasFinished = true;
    }

    private IEnumerator execute() {
        HasFinished = false;
        yield return addCoroutine(owner.StartCoroutine(actionIEnum()));
        HasFinished = true;
    }

    protected virtual IEnumerator actionIEnum() {
        yield return null;
    }

    protected virtual void onCancel() {
    }

    // Adds a coroutine to the list and returns it if needed to yield to
    protected Coroutine addCoroutine(Coroutine coroutine) {
        coroutines.Add(coroutine);
        return coroutine;
    }

    /*** Action helper coroutines ***/

    public static IEnumerator ActionMoveIEnum(Character owner, Vector3 target, bool usePathfinding = true, bool ignoreCanMove = false, params Vector3[] extraPoints) {
        owner.movement.MoveToPoint(target, usePathfinding, ignoreCanMove);
        if (extraPoints.Length > 0) owner.movement.AddToPath(extraPoints);
        while (!owner.movement.HasReachedDestination()) yield return null;
    }

    public static IEnumerator ActionTurnIEnum(Character owner, Vector3 dir) {
        owner.movement.SetLookDir(dir);
        while (Vector3.Angle(owner.transform.forward, dir) > 10) {
            yield return null;
        }
    }

    public static IEnumerator ActionMoveAndTurnIEnum(Character owner, Vector3 target, Vector3 endDir, bool usePathfinding = true, bool ignoreCanMove = false) {
        owner.movement.MoveToPoint(target, usePathfinding, ignoreCanMove);
        while (!owner.movement.HasReachedDestination()) yield return null;
        //while (!owner.Movement.HasPath() || owner.Movement.GetRemainingDist() > 0.01f) yield return null;
        yield return owner.StartCoroutine(ActionTurnIEnum(owner, endDir));
    }

    public static IEnumerator ActionPlayAnimIEnum(Character owner, string trigger, string stateName) {
        owner.appearance.modelAnim.SetTrigger(trigger);
        while(!owner.appearance.modelAnim.GetCurrentAnimatorStateInfo(0).IsName(stateName) || owner.appearance.modelAnim.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.9f) {
            yield return null;
        }
    }
}