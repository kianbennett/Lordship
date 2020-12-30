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

    public Coroutine FadeIn(UnityAction onComplete = null, float duration = 0.25f, Color color = default) {
        color.a = 1;
        if (fadeInCoroutine != null) StopCoroutine(fadeInCoroutine);
        fadeInCoroutine = StartCoroutine(fadeInIEnum(onComplete, duration, color));
        return fadeInCoroutine;
    }

    public Coroutine FadeOut(UnityAction onComplete = null, float duration = 0.25f) {
        if (fadeOutCoroutine != null) StopCoroutine(fadeOutCoroutine);
        fadeOutCoroutine = StartCoroutine(fadeOutIEnum(onComplete, duration));
        return fadeOutCoroutine;
    }

    private IEnumerator fadeInIEnum(UnityAction onComplete, float duration, Color color) {
        canvasGroup.alpha = 1;
        image.color = color;
        while(canvasGroup.alpha > 0) {
            canvasGroup.alpha -= Time.deltaTime * (1f / duration);
            yield return null;
        }
        if(onComplete != null) onComplete();
    }

    private IEnumerator fadeOutIEnum(UnityAction onComplete, float duration) {
        canvasGroup.alpha = 0;
        while (canvasGroup.alpha < 1) {
            canvasGroup.alpha += Time.deltaTime * (1f / duration);
            yield return null;
        }
        if(onComplete != null) onComplete();
    }

    public void SetValues(Color color, float alpha) {
        image.color = color;
        canvasGroup.alpha = alpha;
    }
}
