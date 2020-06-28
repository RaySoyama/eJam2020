using Cinemachine;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour
{
    [SerializeField]
    private GameObject pivotPoint = null;

    [SerializeField]
    private CinemachineVirtualCamera spinnyCam = null;

    [SerializeField]
    private CinemachineVirtualCamera buildCam = null;

    [SerializeField]
    private float horiSpeed = 10.0f;

    [SerializeField]
    private float zoomSpeed = 10.0f;

    [SerializeField]
    private Vector2 zoomClamp = new Vector2();

    [SerializeField]
    private CameraState currentCamState = CameraState.Spin;

    [SerializeField]
    private Image fadeMat = null;

    [SerializeField]
    private float fadeSpeed = 1.0f;

    [SerializeField]
    private Vector2 fadeStarScale = Vector2.one;

    [SerializeField]
    private AnimationCurve curveIn = null;

    [SerializeField]
    private AnimationCurve curveOut = null;

    public enum CameraState
    {
        Spin,
        Card,
        Build
    }

    void Start()
    {
        if (pivotPoint == null)
        {
            Debug.LogError("Missing reference to Pivot Point");
        }
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.D))
        {
            switch (currentCamState)
            {
                case CameraState.Spin:
                    pivotPoint.transform.eulerAngles -= (Vector3.up * horiSpeed * Time.deltaTime);
                    break;

                case CameraState.Card:
                    break;

                case CameraState.Build:
                    break;
            }
        }

        if (Input.GetKey(KeyCode.A))
        {
            switch (currentCamState)
            {
                case CameraState.Spin:
                    pivotPoint.transform.eulerAngles += (Vector3.up * horiSpeed * Time.deltaTime);
                    break;

                case CameraState.Card:
                    break;

                case CameraState.Build:
                    break;
            }
        }

        if (Input.GetKey(KeyCode.W))
        {
            switch (currentCamState)
            {
                case CameraState.Spin:
                    spinnyCam.transform.localPosition += Vector3.forward * zoomSpeed * Time.deltaTime;

                    if (spinnyCam.transform.localPosition.z > zoomClamp.x)
                    {
                        spinnyCam.transform.localPosition = new Vector3(spinnyCam.transform.localPosition.x, spinnyCam.transform.localPosition.y, zoomClamp.x);
                    }
                    break;

                case CameraState.Card:
                    break;

                case CameraState.Build:
                    break;
            }
        }

        if (Input.GetKey(KeyCode.S))
        {
            switch (currentCamState)
            {
                case CameraState.Spin:
                    spinnyCam.transform.localPosition -= Vector3.forward * zoomSpeed * Time.deltaTime;

                    if (spinnyCam.transform.localPosition.z < zoomClamp.y)
                    {
                        spinnyCam.transform.localPosition = new Vector3(spinnyCam.transform.localPosition.x, spinnyCam.transform.localPosition.y, zoomClamp.y);
                    }
                    break;

                case CameraState.Card:
                    break;

                case CameraState.Build:
                    break;
            }
        }
    }

    public void OnStartCardMaker()
    {
        StartCoroutine(FadeIntoCardMaker(() =>
        {
            buildCam.Priority = 100;
        })); ;
    }

    private IEnumerator FadeIntoCardMaker(Action cameraAction = null)
    {
        Material mat = fadeMat.material;

        float t = 0.0f;
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

        cameraAction?.Invoke();

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


    public void OnExitCardMaker()
    {
        StartCoroutine(FadeIntoCardMaker(() =>
        {
            buildCam.Priority = 0;
        })); ;
    }
}
