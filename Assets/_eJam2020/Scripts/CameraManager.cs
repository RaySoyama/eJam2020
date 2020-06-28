using Cinemachine;
using UnityEngine;

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
        buildCam.Priority = 100;
    }

    public void OnExitCardMaker()
    {
        buildCam.Priority = 0;
    }
}
