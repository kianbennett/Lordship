using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class Tooltip : MonoBehaviour {

    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private TextMeshProUGUI textComponent;

    void LateUpdate() {
        if(EventSystem.current.IsPointerOverGameObject()) {
            Hide();
        }

        rectTransform.position = Input.mousePosition/* + Vector3.up * rectTransform.sizeDelta.y / 2*/;
        float width = Mathf.Lerp(rectTransform.sizeDelta.x, textComponent.preferredWidth + 16, Time.deltaTime * 10);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
    }

    public void Show(string text) {
        if(EventSystem.current.IsPointerOverGameObject()) return;
        textComponent.text = text;
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0);
        gameObject.SetActive(true);
    }

    public void Hide() {
        gameObject.SetActive(false);
    }
}
