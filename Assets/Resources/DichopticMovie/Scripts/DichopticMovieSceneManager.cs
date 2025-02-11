using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine.XR;

public class DichopticMovieSceneManager : MonoBehaviour
{
    private string EMPTY_MOVIE_NAME = "";
    public static DichopticMovieSceneManager Instance;

    [SerializeField]
    public VideoPlayer videoPlayer;
    [SerializeField]
    public Material dichopticFilterMaterial;

    [SerializeField]
    public TMP_Dropdown movieListDropdown;

    [SerializeField]
    public GameObject blobClippingSlider;

    [SerializeField]
    public GameObject blobScaleSlider;

    [SerializeField]
    public GameObject blobGrayColorSlider;

    [SerializeField]
    public GameObject blobTimerSlider;

    [SerializeField]
    public GameObject settingsUI;

    private DichopticMovieSettingsManager settingsManager = null;

    private float timerValue;
    private float timerStep = 10;

    private bool wasMenuButtonPressed = false;


    void Awake()
    {
        Instance = this;
        StartCoroutine(RunBlobChangeTimer());
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        RestoreInitialSettings();
        PopulateMovieDropdown();
    }

    // Update is called once per frame
    void Update()
    {
        // If Menu button -> Show Settings UI
        if (InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(CommonUsages.menuButton, out bool isPressed))
        {
            if (isPressed && !wasMenuButtonPressed)
            {
                settingsUI.SetActive(!settingsUI.activeSelf);
                wasMenuButtonPressed = true;
            }
            wasMenuButtonPressed = isPressed;
        }
    }

    private void RestoreInitialSettings()
    {
        settingsManager = new DichopticMovieSettingsManager(blobClippingSlider.GetComponent<Slider>().value,
                                                    blobScaleSlider.GetComponent<Slider>().value,
                                                    blobGrayColorSlider.GetComponent<Slider>().value,
                                                    blobTimerSlider.GetComponent<Slider>().value);

        HandleChangeBlobClipping(settingsManager.GetBlobClipValue());
        HandleChangeBlobScale(settingsManager.GetBlobScaleValue());
        HandleChangeBlobGreyValue(settingsManager.GetBlobGreyColorValue());
        HandleChangeBlobTimerValue(settingsManager.GetBlobTimerValue());
    }

    private void PopulateMovieDropdown()
    {
        StorageHandler.InitDirectoryTree();
        List<string> availableMovies = StorageHandler.GetFileNamesFromDir(TypeSafeDir.Movies);
        // Order by name ignoring the case
        availableMovies.Sort(StringComparer.OrdinalIgnoreCase);
        List<string> allowedExtensions = new() { ".asf", ".avi", ".dv", ".m4v", ".mp4", ".mov", ".mpg", ".mpeg", ".m4v", ".ogv", ".vp8", ".webm", ".wmv" };

        movieListDropdown.ClearOptions();
        movieListDropdown.AddOptions(new List<string> { EMPTY_MOVIE_NAME });
        foreach (string movieFileName in availableMovies)
        {
            if (allowedExtensions.Contains(Path.GetExtension(movieFileName)))
            {
                movieListDropdown.AddOptions(new List<string> { movieFileName });
            }
        }
    }

    public void ChangeBlobClipping()
    {
        float blobClippingValue = blobClippingSlider.GetComponent<Slider>().value;
        settingsManager?.SetBlobClipValue(blobClippingValue);
        HandleChangeBlobClipping(blobClippingValue);
    }

    public void ChangeBlobScale()
    {
        float blobScaleValue = blobScaleSlider.GetComponent<Slider>().value;
        settingsManager?.SetBlobScaleValue(blobScaleValue);
        HandleChangeBlobScale(blobScaleValue);
    }

    public void ChangeBlobGreyValue()
    {
        float greyColorValue = blobGrayColorSlider.GetComponent<Slider>().value;
        settingsManager?.SetBlobGreyColorValue(greyColorValue);
        HandleChangeBlobGreyValue(greyColorValue);
    }

    public void ChangeBlobTimerValue()
    {
        float blobTimerValue = blobTimerSlider.GetComponent<Slider>().value;
        settingsManager?.SetBlobTimerValue(blobTimerValue);
        HandleChangeBlobTimerValue(blobTimerValue);
    }

    private void HandleChangeBlobClipping(float blobClipValue)
    {
        blobClippingSlider.GetComponentInChildren<TextMeshProUGUI>().text = blobClipValue.ToString("0.00");
        blobClippingSlider.GetComponent<Slider>().value = blobClipValue;
        dichopticFilterMaterial.SetFloat("_BlobClipping", blobClipValue);
    }


    private void HandleChangeBlobScale(float blobScaleValue)
    {
        blobScaleSlider.GetComponentInChildren<TextMeshProUGUI>().text = blobScaleValue.ToString("0.00");
        blobScaleSlider.GetComponent<Slider>().value = blobScaleValue;
        dichopticFilterMaterial.SetFloat("_BlobScale", blobScaleValue);
    }

    private void HandleChangeBlobGreyValue(float greyColorValue)
    {
        blobGrayColorSlider.GetComponent<Slider>().value = greyColorValue;
        blobGrayColorSlider.GetComponentInChildren<TextMeshProUGUI>().text = greyColorValue.ToString("0");
        dichopticFilterMaterial.SetColor("_BlobColor", new Color(greyColorValue / 255, greyColorValue / 255, greyColorValue / 255, 0));
    }

    private void HandleChangeBlobTimerValue(float blobTimerValue)
    {
        blobTimerSlider.GetComponent<Slider>().value = blobTimerValue;
        timerStep = blobTimerValue;
        blobTimerSlider.GetComponentInChildren<TextMeshProUGUI>().text = blobTimerValue.ToString("0");
    }

    public void LoadMovieButtonHandle()
    {
        string selectedFilename = movieListDropdown.captionText.text;
        if (selectedFilename != EMPTY_MOVIE_NAME)
        {
            List<string> availableMovies = StorageHandler.GetFilePathsFromDir(TypeSafeDir.Movies);
            foreach (string filepath in availableMovies)
            {
                if (filepath.Contains(selectedFilename))
                {
                    videoPlayer.source = VideoSource.Url;
                    // Send audio directly to Quest audio hw
                    videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
                    // At least 1 audio track controlled
                    videoPlayer.controlledAudioTrackCount = 1;
                    videoPlayer.GetComponent<AudioSource>().volume = 1.0f;
                    videoPlayer.url = filepath;
                    videoPlayer.Play();
                    break;
                }
            }
        }
    }

    public void ReturnToMainMenu()
    {
        MainMenuManagerScript.LoadMainMenuScene();
    }

    public void DeleteSelectedMovie()
    {
        string selectedFilename = movieListDropdown.captionText.text;
        if (selectedFilename != EMPTY_MOVIE_NAME)
        {
            StorageHandler.DeleteFileFromDir(TypeSafeDir.Movies, selectedFilename);
            PopulateMovieDropdown();
        }
    }

    IEnumerator RunBlobChangeTimer()
    {
        while (true)
        {
            yield return new WaitForSeconds(timerStep);
            timerValue += 1;
            dichopticFilterMaterial.SetVector("_BlobOffset", new Vector2(timerValue, (int)timerValue / 10));
        }
    }
}
