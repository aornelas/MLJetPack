using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextFader : MonoBehaviour {
    public float DEFAULT_FADE_TIME = 1.0f;
	public float fadeTime = 1;

	// Use this for initialization
	public void SetText (string text, float newFadeTime = -1) {
        fadeTime = newFadeTime > -1 ? newFadeTime : DEFAULT_FADE_TIME;
        GetComponentInChildren<Text>().text = text;
	}

	// Update is called once per frame
	void Update () {
        if (fadeTime > 0)
        {
            fadeTime -= Time.deltaTime;

            if (fadeTime <= 0) {
                fadeTime = 0;
                GetComponentInChildren<Text>().text = null;
                return;
            }
            Color color = GetComponentInChildren<Text>().color;
            float alpha = Mathf.Max(0, Mathf.Min(fadeTime, 1));
            color.a = alpha;

            GetComponentInChildren<Text>().color = color;
        }
	}
}
