using UnityEngine;

public class EyeMaterialPropertyUpdater : MonoBehaviour
{
    public Material sharedMaterial;
    public string propertyName = "_SupressingEyeIndex";
    private bool eyeToggleFlag = false;
    public Camera targetCamera;

    private void Start()
    {
        if (targetCamera != null)
        {
            Camera.onPreRender += OnCameraPreRender;
        }
    }

    private void OnCameraPreRender(Camera cam)
    {
        if (cam.stereoActiveEye == Camera.MonoOrStereoscopicEye.Left)
        {
            sharedMaterial.SetInt(propertyName, eyeToggleFlag?1:0);
        }
        else if (cam.stereoActiveEye == Camera.MonoOrStereoscopicEye.Right)
        {
            sharedMaterial.SetInt(propertyName, eyeToggleFlag?0:1);
        }
    }

    private void OnDestroy()
    {
        if (targetCamera != null)
        {
            Camera.onPreRender -= OnCameraPreRender;
        }
    }
}