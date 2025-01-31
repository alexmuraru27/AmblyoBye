using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using System.Collections.Generic;
using System.IO;

public class DichopticMovieSceneManager : MonoBehaviour
{
    public static DichopticMovieSceneManager Instance;

    [SerializeField]
    public VideoPlayer moviePlayer;
    [SerializeField]
    public Material dichopticFilterMaterial;

    [SerializeField]
    public TMP_Dropdown movieListDropdown;

    private float timerValue;
    private float timerStep = 10;

    void Awake()
    {
        Instance = this;
        StartCoroutine(RunBlobChangeTimer());
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PopulateMovieDropdown();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void PopulateMovieDropdown()
    {
        StorageHandler.InitFS(TypeSafeDir.Movies);
        List<string> availableMovies = StorageHandler.GetFileNamesFromDir(TypeSafeDir.Movies);
        List<string> allowedExtensions = new() { ".asf", ".avi", ".dv", ".m4v", ".mp4", ".mov", ".mpg", ".mpeg", ".m4v", ".ogv", ".vp8", ".webm", ".wmv" };
        movieListDropdown.ClearOptions();
        foreach (string movieFileName in availableMovies)
        {
            if (allowedExtensions.Contains(Path.GetExtension(movieFileName)))
            {
                movieListDropdown.AddOptions(new List<string> { movieFileName });
            }
        }
    }

    public void ChangeBlobClipping(GameObject clippingObject)
    {
        float blobClipValue = clippingObject.GetComponent<Slider>().value;
        clippingObject.GetComponentInChildren<TextMeshProUGUI>().text = blobClipValue.ToString("0.00");
        dichopticFilterMaterial.SetFloat("_BlobClipping", blobClipValue);
    }

    public void ChangeBlobScale(GameObject scaleObject)
    {
        float blobScaleValue = scaleObject.GetComponent<Slider>().value;
        scaleObject.GetComponentInChildren<TextMeshProUGUI>().text = blobScaleValue.ToString("0.00");
        dichopticFilterMaterial.SetFloat("_BlobScale", blobScaleValue);
    }

    public void ChangeBlobGreyValue(GameObject greyObject)
    {
        float greyColorValue = greyObject.GetComponent<Slider>().value;
        greyObject.GetComponentInChildren<TextMeshProUGUI>().text = greyColorValue.ToString("0");
        dichopticFilterMaterial.SetColor("_BlobColor", new Color(greyColorValue / 255, greyColorValue / 255, greyColorValue / 255, 0));
    }

    public void ChangeBlobTimerValue(GameObject timerObject)
    {
        float blobTimerValue = timerObject.GetComponent<Slider>().value;
        timerStep = blobTimerValue;
        timerObject.GetComponentInChildren<TextMeshProUGUI>().text = blobTimerValue.ToString("0");
    }

    public void ChangeEyeFilterToggle(GameObject eyeFilterObject)
    {
        bool isFilterEyeRight = eyeFilterObject.GetComponent<Toggle>().isOn;
        dichopticFilterMaterial.SetInt("_SupressingEyeIndex", isFilterEyeRight ? 1 : 0);
    }

    public void ToggleSettingsMenuVisibility(GameObject settingsMenuObject)
    {
        settingsMenuObject.SetActive(!settingsMenuObject.activeSelf);
    }

    public void LoadMovieButtonHandle()
    {
        List<string> availableMovies = StorageHandler.GetFilePathsFromDir(TypeSafeDir.Movies);
        string selectedFilename = movieListDropdown.captionText.text;
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
                break;
            }
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
