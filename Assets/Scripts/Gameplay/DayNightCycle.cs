using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle : MonoBehaviour {

    public float skyRotSpeed;

    void Update() {
        RenderSettings.skybox.SetFloat("_Rotation", Time.time * skyRotSpeed);
    }
}
