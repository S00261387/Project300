using UnityEngine;
using Cinemachine;

public class CameraSwitcher : MonoBehaviour
{
    public CinemachineVirtualCamera topDownVC;
    public CinemachineVirtualCamera firstPersonVC;

    private static CinemachineVirtualCamera _activeCamera;

    void Start()
    {
        // Start in top-down by default
        ActivateCamera(topDownVC);
    }

    void Update()
    {
        // Toggle with C key
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (_activeCamera == topDownVC)
            {
                ActivateCamera(firstPersonVC);
            }
            else
            {
                ActivateCamera(topDownVC);
            }
        }
    }

    public static void ActivateCamera(CinemachineVirtualCamera cam)
    {
        if (_activeCamera != null)
            _activeCamera.Priority = 10;

        cam.Priority = 20;
        _activeCamera = cam;
    }

    public static bool IsActiveCamera(CinemachineVirtualCamera cam)
    {
        return _activeCamera == cam;
    }
}
