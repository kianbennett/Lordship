using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour {

    public float width, depth;
    public ParticleSystem smokeParticles;

    public Vector2 size {
        get { return new Vector2(width, depth); }
    }

    void Awake() {
        // 50% chance to have smoke coming out of chimney
        smokeParticles.gameObject.SetActive(Random.value > 0.5f);
    }
}
