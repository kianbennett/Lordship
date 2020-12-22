using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveMarker : MonoBehaviour {

    void Awake() {
        // Destroy after the animation has finished playing
        Destroy(gameObject, GetComponentInChildren<Animator>().GetCurrentAnimatorStateInfo(0).length);    
    }
}
