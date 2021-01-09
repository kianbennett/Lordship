using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Clock : MonoBehaviour {

    public TextMeshProUGUI textTime;
    public Image ring;

    public void UpdateClock(float timeElapsed, float dayDuration) {
        ring.fillAmount = 1 - timeElapsed / dayDuration;

        int startHour = 7;
        int endHour = 24;
        int hour = startHour + Mathf.FloorToInt((endHour - startHour) * timeElapsed / dayDuration);
        if(hour > 12) {
            textTime.text = (hour - 12).ToString();
        } else {
            textTime.text = hour.ToString();
        }
        if(hour >= 12) {
            textTime.text += "pm";
        } else {
            textTime.text += "am";
        }
    }
}
