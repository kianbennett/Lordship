using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MovementParams", menuName = "Movement Params")]
public class MovementParams : ScriptableObject {

    public float WalkSpeed, RunSpeed;
    public AnimationCurve AccCurve, DecCurve;
}