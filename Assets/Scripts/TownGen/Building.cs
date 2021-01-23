using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour 
{
    [SerializeField] private float width, depth;
    [SerializeField] private ParticleSystem smokeParticles;

    // Size is used get which grid points are underneath the building
    public Vector2 Size { get { return new Vector2(width, depth); } }

    void Awake() 
    {
        // 50% chance to have smoke coming out of chimney
        smokeParticles.gameObject.SetActive(Random.value > 0.5f);
    }
}
