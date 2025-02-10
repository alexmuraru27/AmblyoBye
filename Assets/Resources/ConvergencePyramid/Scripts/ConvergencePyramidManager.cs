using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.SceneManagement;

public class ConvergencePyramidManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(CommonUsages.menuButton, out bool primaryButton) && primaryButton)
        {
            MainMenuManagerScript.LoadMainMenuScene();
        }
    }
}
