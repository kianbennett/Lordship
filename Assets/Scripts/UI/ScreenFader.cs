using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/*
 *  Controls a full screen overlay that fades in and out
 */

public class ScreenFader : MonoBehaviour {

    [SerializeField] private Image image;
    [SerializeField] private CanvasGroup canvasGroup;

    private Coroutine fadeInCoroutine, fadeOutCoroutine;

    public Coroutine FadeIn(UnityAction onComplete = null, bool unscaledTime = false, float duration = 0.4f, Color color = default) {
        if(color == default) color = image.color;
        color.a = 1;
        if (fadeInCoroutine != null) StopCoroutine(fadeInCoroutine);
        fadeInCoroutine = StartCoroutine(fadeInIEnum(onComplete, unscaledTime, duration, color));
        return fadeInCoroutine;
    }

    public Coroutine FadeOut(UnityAction onComplete = null, bool unscaledTime = false, float duration = 0.25f) {
        if (fadeOutCoroutine != null) StopCoroutine(fadeOutCoroutine);
        fadeOutCoroutine = StartCoroutine(fadeOutIEnum(onComplete, unscaledTime, duration));
        return fadeOutCoroutine;
    }

    private IEnumerator fadeInIEnum(UnityAction onComplete, bool unscaledTime, float duration, Color color) {
        // Block raycasts until the fade has complete
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1;
        image.color = color;
        while(canvasGroup.alpha > 0) {
            float deltaTime = unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            canvasGroup.alpha -= deltaTime * (1f / duration);
            yield return null;
        }
        canvasGroup.blocksRaycasts = false;
        if(onComplete != null) onComplete();
    }

    private IEnumerator fadeOutIEnum(UnityAction onComplete, bool unscaledTime, float duration) {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 0;
        while (canvasGroup.alpha < 1) {
            float deltaTime = unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            canvasGroup.alpha += deltaTime * (1f / duration);
            yield return null;
        }
        canvasGroup.blocksRaycasts = false;
        if(onComplete != null) onComplete();
    }

    // public void SetValues(Color color, float alpha) {
    //     image.color = color;
    //     canvasGroup.alpha = alpha;
    // }

    public void SetAlpha(float alpha) {
        canvasGroup.alpha = alpha;
    }
}
