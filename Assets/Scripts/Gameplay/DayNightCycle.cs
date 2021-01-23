using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Controls the colour, skybox and lamppost lights depending on the progress throughout the day

public class DayNightCycle : MonoBehaviour 
{
    public float skyRotSpeed;
    public Light dayLight;
    public Color lightDay, lightDusk, lightNight;
    public Color ambientDay, ambientNight;
    public Material skyboxDay, skyboxDusk, skyboxNight;

    [SerializeField, ReadOnly] 
    private float progress;

    void Update() 
    {
        RenderSettings.skybox.SetFloat("_Rotation", Time.time * skyRotSpeed);
        UpdateLampLights();
    }

    public void UpdateDayTime(float timeElapsed, float dayDuration) 
    {
        HUD.instance.clock.UpdateClock(timeElapsed, dayDuration);
        progress = timeElapsed / dayDuration;

        if(progress < 0.5f) 
        {
            dayLight.color = lightDay;
            RenderSettings.ambientLight = ambientDay;
        } 
        else if(progress < 0.7f) 
        {
            dayLight.color = Color.Lerp(lightDay, lightDusk, (progress - 0.5f) / 0.2f);
        }
        else if(progress < 0.8f) 
        {
            dayLight.color = Color.Lerp(lightDusk, lightNight, (progress - 0.7f) / 0.1f);
        } 
        else 
        {
            dayLight.color = lightNight;
        }

        if(progress < 0.6f) 
        {
            RenderSettings.skybox = skyboxDay;
            RenderSettings.ambientLight = ambientDay;
        } 
        else if(progress < 0.75f) 
        {
            RenderSettings.skybox = skyboxDusk;
            RenderSettings.ambientLight = Color.Lerp(ambientDay, ambientNight, (progress - 0.6f) / 0.15f);
        } 
        else 
        {
            RenderSettings.skybox = skyboxNight;
            RenderSettings.ambientLight = ambientNight;
        }

        // Set the x rotation of directional light to give the effect of the sun setting and the moon rising
        Vector3 lightRot = dayLight.transform.rotation.eulerAngles;
        if(progress > 0.5f && progress < 0.7f) 
        {
            lightRot.x = 50 - 15 * (progress - 0.5f) / 0.2f;
        } 
        else if(progress > 0.7f && progress < 0.8f) 
        {
            lightRot.x = 35 + 15 * (progress - 0.7f) / 0.1f;
        }
        dayLight.transform.rotation = Quaternion.Euler(lightRot);
    }

    // Set the strengths of each lamp light and deactivate if outside camera bounds
    public void UpdateLampLights() 
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(CameraController.instance.Camera);

        foreach(Light light in TownGenerator.instance.LampLights) 
        {
            if(!light) continue;
            // Test if bounds of light is within the camera's view frustum
            Bounds bounds = new Bounds(light.transform.position, Vector3.one * light.range);
            bool inView = GeometryUtility.TestPlanesAABB(planes, bounds);
            light.gameObject.SetActive(progress > 0.7f && inView);
        }
    }
}
