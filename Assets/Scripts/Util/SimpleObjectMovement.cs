using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 *  Helper component that contains a variety of simple, commonly used object movements
 */

public class SimpleObjectMovement : MonoBehaviour {

    public enum MovementType { Rotate, Bob }

    public MovementType type;

    // Rotate
    [HideInInspector] public Vector3 rotationAxis;
    [HideInInspector] public float rotationSpeed;

    // Bob
    [HideInInspector] public Vector3 bobAxis;
    [HideInInspector] public float bobDistance, bobSpeed;

    private Vector3 initPos;

    void Awake() {
        initPos = transform.localPosition;
    }

    void Update() {
        switch(type) {
            case MovementType.Rotate:
                transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
                break;
            case MovementType.Bob:
                transform.localPosition = initPos + bobAxis * bobDistance * 
                    Mathf.Sin(Time.timeSinceLevelLoad * bobSpeed);
                break;
        }
    }
}
