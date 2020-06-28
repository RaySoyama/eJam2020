using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FadeManager : MonoBehaviour
{
    [SerializeField]
    private string startMenuName = "StartScene";

    [SerializeField]
    private Image fadeImg = null;

    [SerializeField]
    private float fadeSpeed = 1.0f;

    [SerializeField]
    private Vector2 fadeStarScale = Vector2.one;

    [SerializeField]
    private AnimationCurve curveIn = null;

    [SerializeField]
    private AnimationCurve curveOut = null;

    void Start()
    {
        StartCoroutine(FadeIntoCardMaker(false));
    }

    public void OnPlay(Action action)
    {
        StartCoroutine(FadeIntoCardMaker(true, action));
    }


    public void OnQuitToMenu()
    {
        StartCoroutine(FadeIntoCardMaker(true, () =>
        {
            SceneManager.LoadScene(startMenuName);
        }));
    }


    private IEnumerator FadeIntoCardMaker(bool isFadeIn, Action postAction = null)
    {
        Material mat = fadeImg.material;
        float t = 0.0f;

        if (isFadeIn)
        {
            //fade small
            while (t < 1.0f)
            {
                //fadeUI.transform.localScale = Vector3.Lerp(Vector3.one * fadeStarScale.x, Vector3.one * fadeStarScale.y, t);

                float fuck = Mathf.Lerp(fadeStarScale.x, fadeStarScale.y, curveIn.Evaluate(t));

                mat.SetTextureScale("_MainTex", Vector2.one * fuck);
                mat.SetTextureOffset("_MainTex", Vector2.one * ((fuck - 1.0f) / 2.0f) * -1.0f);

                t += Time.deltaTime * 1.0f / fadeSpeed;
                yield return new WaitForEndOfFrame();
            }

            mat.SetTextureScale("_MainTex", Vector2.one * fadeStarScale.y);
            mat.SetTextureOffset("_MainTex", Vector2.one * ((fadeStarScale.y - 1.0f) / 2.0f) * -1.0f);

        }
        else
        {

            t = 0.0f;
            //fade big
            while (t < 1.0f)
            {
                float fuck = Mathf.Lerp(fadeStarScale.y, fadeStarScale.x, curveOut.Evaluate(t));

                mat.SetTextureScale("_MainTex", Vector2.one * fuck);
                mat.SetTextureOffset("_MainTex", Vector2.one * ((fuck - 1.0f) / 2.0f) * -1.0f);

                t += Time.deltaTime * 1.0f / fadeSpeed;
                yield return new WaitForEndOfFrame();
            }
            mat.SetTextureScale("_MainTex", Vector2.one * fadeStarScale.x);
            mat.SetTextureOffset("_MainTex", Vector2.one * ((fadeStarScale.x - 1.0f) / 2.0f) * -1.0f);
        }

        postAction?.Invoke();
    }
}
