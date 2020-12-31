using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle : MonoBehaviour {

    public float skyRotSpeed;
    public Light dayLight;
    public Color lightDay, lightDusk, lightNight;
    public Color ambientDay, ambientNight;
    public Material skyboxDay, skyboxDusk, skyboxNight;

    [SerializeField, ReadOnly] 
    private float progress;

    void Update() {
        RenderSettings.skybox.SetFloat("_Rotation", Time.time * skyRotSpeed);
        UpdateLampLights();
    }

    public void UpdateDayTime(float timeElapsed, float dayDuration) {
        progress = timeElapsed / dayDuration;

        if(progress < 0.5f) {
            dayLight.color = lightDay;
            RenderSettings.ambientLight = ambientDay;
        } else if(progress < 0.7f) {
            dayLight.color = Color.Lerp(lightDay, lightDusk, (progress - 0.5f) / 0.2f);
        } else if(progress < 0.8f) {
            dayLight.color = Color.Lerp(lightDusk, lightNight, (progress - 0.7f) / 0.1f);
        } else {
            dayLight.color = lightNight;
        }

        if(progress < 0.6f) {
            RenderSettings.skybox = skyboxDay;
            RenderSettings.ambientLight = ambientDay;
        } else if(progress < 0.75f) {
            RenderSettings.skybox = skyboxDusk;
            RenderSettings.ambientLight = Color.Lerp(ambientDay, ambientNight, (progress - 0.6f) / 0.15f);
        } else {
            RenderSettings.skybox = skyboxNight;
            RenderSettings.ambientLight = ambientNight;
        }
    }

    // Set the strengths of each lamp light and deactivate if outside camera bounds
    public void UpdateLampLights() {
        List<Light> lights = TownGenerator.instance.getLampLights();

        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(CameraController.instance.camera);

        foreach(Light light in lights) {
            if(!light) continue;
            // Test if bounds of light is within the camera's view frustum
            Bounds bounds = new Bounds(light.transform.position, Vector3.one * light.range);
            bool inView = GeometryUtility.TestPlanesAABB(planes, bounds);
            light.gameObject.SetActive(progress > 0.7f && inView);
        }
    }
}
