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
    public VideoPlayer moviePlayer;
    [SerializeField]
    public Material dichopticFilterMaterial;

    [SerializeField]
    public TMP_Dropdown movieListDropdown;

    [SerializeField]
    public GameObject eyeBiasSlider;

    [SerializeField]
    public GameObject blobScaleSlider;

    [SerializeField]
    public GameObject blobGrayColorSlider;

    [SerializeField]
    public GameObject blobTimerSlider;

    [SerializeField]
    public GameObject settingsUI;

    [SerializeField]
    public TextMeshProUGUI versionTextBox;

    private DichopticMovieSettingsManager settingsManager = null;

    private float timerValue;
    private float timerStep = 10;

    private bool wasMenuButtonPressed = false;

    void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        versionTextBox.text = "v" + Application.version;
        RestoreInitialSettingsFromPersistance();
        PopulateMovieDropdown();
        StartCoroutine(RunBlobChangeTimer());
    }

    // Update is called once per frame
    void Update()
    {
        // If Menu button -> Show Settings UI
        bool isPressedLeft;
        bool isPressedRight;
        InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.primary2DAxisClick, out isPressedRight);
        InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(CommonUsages.primary2DAxisClick, out isPressedLeft);
        if ((isPressedRight || isPressedLeft) && !wasMenuButtonPressed)
        {
            settingsUI.SetActive(!settingsUI.activeSelf);
            wasMenuButtonPressed = true;
        }
        wasMenuButtonPressed = isPressedRight || isPressedLeft;

    }

    private void RestoreInitialSettingsFromPersistance()
    {
        settingsManager = new DichopticMovieSettingsManager(eyeBiasSlider.GetComponent<Slider>().value,
                                                    blobScaleSlider.GetComponent<Slider>().value,
                                                    blobGrayColorSlider.GetComponent<Slider>().value,
                                                    blobTimerSlider.GetComponent<Slider>().value);
        settingsManager.TryRestore();
        RestoreSettingsPanelFromManager(settingsManager);
    }

    public void ResetSettings()
    {
        settingsManager = new DichopticMovieSettingsManager(0.5F, 1.0F, 70.0F, 5.0F);
        settingsManager.StoreSettings();
        RestoreSettingsPanelFromManager(settingsManager);
    }

    private void RestoreSettingsPanelFromManager(DichopticMovieSettingsManager settingsManager)
    {
        HandleChangeEyeBiasValue(settingsManager.GetEyeBiasValue());
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

    public void ChangeEyeBiasValue()
    {
        float eyeBiasValue = eyeBiasSlider.GetComponent<Slider>().value;
        settingsManager?.SetEyeBiasValue(eyeBiasValue);
        HandleChangeEyeBiasValue(eyeBiasValue);
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

    private void HandleChangeEyeBiasValue(float eyeBiasValue)
    {
        eyeBiasSlider.GetComponentInChildren<TextMeshProUGUI>().text = eyeBiasValue.ToString("0.00");
        eyeBiasSlider.GetComponent<Slider>().value = eyeBiasValue;
        dichopticFilterMaterial.SetFloat("_BlobClipping", eyeBiasValue);
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
        blobTimerSlider.GetComponentInChildren<TextMeshProUGUI>().text = blobTimerValue.ToString("0.0");
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
                    moviePlayer.source = VideoSource.Url;
                    // Send audio directly to Quest audio hw
                    moviePlayer.audioOutputMode = VideoAudioOutputMode.Direct;
                    // At least 1 audio track controlled
                    moviePlayer.controlledAudioTrackCount = 1;
                    moviePlayer.GetComponent<AudioSource>().volume = 1.0f;
                    moviePlayer.url = filepath;
                    moviePlayer.Play();
                    settingsUI.SetActive(false);
                    break;
                }
            }
        }
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
        System.Random random = new System.Random();
        while (true)
        {
            yield return new WaitForSeconds(timerStep);
            timerValue += timerStep;

            float f1 = (float)(32748 * 2.0 * (random.NextDouble() - 0.5));
            float f2 = (float)(32748 * 2.0 * (random.NextDouble() - 0.5));
            dichopticFilterMaterial.SetVector("_BlobOffset", new Vector2(f1, f2));
        }
    }
}
