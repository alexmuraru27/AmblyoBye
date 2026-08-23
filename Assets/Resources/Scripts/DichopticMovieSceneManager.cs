using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.UI;
using Unity.VRTemplate;

public class DichopticMovieSceneManager : MonoBehaviour
{
    private const float DISTANCE_TO_SCREEN_IN_M = 2.0f;
    private string EMPTY_MOVIE_NAME = "";

    // Generic Input System XR controller bindings (device-agnostic: works with any
    // OpenXR-mapped controller exposing the XRController layout, e.g. Quest, Pico, Vive, Index, WMR).
    private const string LEFT_THUMBSTICK_CLICK_BINDING = "<XRController>{LeftHand}/{Primary2DAxisClick}";
    private const string RIGHT_THUMBSTICK_CLICK_BINDING = "<XRController>{RightHand}/{Primary2DAxisClick}";

    private static readonly HashSet<string> AllowedMovieExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".asf", ".avi", ".dv", ".m4v", ".mkv", ".mp4", ".mov", ".mpg", ".mpeg", ".ogv", ".vp8",
        ".webm", ".wmv"
    };

    public static DichopticMovieSceneManager Instance;

    [SerializeField]
    public GameObject moviePlayerObject;

    [SerializeField]
    public VideoPlayer videoPlayer;

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

    [SerializeField]
    public ConfirmationDialog confirmationDialog;

    [SerializeField]
    public TextMeshProUGUI totalPlayedTimeTextBox;

    private float sessionSecondsWatched = 0f;
    private float blobTimerDuration = 10;
    private bool isCameraInit = false;
    private DichopticMovieSettingsManager settingsManager = null;
    private TMP_Dropdown renderResolutionDropdown;
    private Toggle showFrameRateToggle;
    private TextMeshProUGUI frameRateTextBox;
    private VideoPlayerRenderTexture videoPlayerRenderTexture;

    private DailyUsageTracker usageTracker = null;

    // Toggle-settings-UI input actions, built entirely in code so no manual Editor
    // wiring is required and any XRController-compatible device works out of the box.
    private InputAction leftThumbstickClickAction;
    private InputAction rightThumbstickClickAction;

    void Awake()
    {
        Instance = this;
        usageTracker = new DailyUsageTracker();

        leftThumbstickClickAction = new InputAction(name: "ToggleSettingsUI (Left)", type: InputActionType.Button, binding: LEFT_THUMBSTICK_CLICK_BINDING);
        rightThumbstickClickAction = new InputAction(name: "ToggleSettingsUI (Right)", type: InputActionType.Button, binding: RIGHT_THUMBSTICK_CLICK_BINDING);

        leftThumbstickClickAction.performed += OnToggleSettingsUIRequested;
        rightThumbstickClickAction.performed += OnToggleSettingsUIRequested;
    }

    void OnEnable()
    {
        leftThumbstickClickAction.Enable();
        rightThumbstickClickAction.Enable();
    }

    void OnDisable()
    {
        leftThumbstickClickAction.Disable();
        rightThumbstickClickAction.Disable();
    }

    void OnDestroy()
    {
        videoPlayer.prepareCompleted -= OnVideoPrepared;
        leftThumbstickClickAction.performed -= OnToggleSettingsUIRequested;
        rightThumbstickClickAction.performed -= OnToggleSettingsUIRequested;
        leftThumbstickClickAction.Dispose();
        rightThumbstickClickAction.Dispose();
    }

    private void OnToggleSettingsUIRequested(InputAction.CallbackContext context)
    {
        settingsUI.SetActive(!settingsUI.activeSelf);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        versionTextBox.text = "v" + Application.version;
        UpdateTimeWatchedText(0);
        InitializeRenderResolutionUI();
        InitializeFrameRateUI();
        RestoreInitialSettingsFromPersistance();
        PopulateMovieDropdown();
        StartCoroutine(RunBlobChangeTimer());
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCamera();

        if (videoPlayer.isPlaying)
        {
            usageTracker?.Tick(Time.deltaTime);
            sessionSecondsWatched += Time.deltaTime;
            UpdateTimeWatchedText((int)sessionSecondsWatched);
        }
    }

    private void UpdateTimeWatchedText(int seconds)
    {
        TimeSpan t = TimeSpan.FromSeconds(seconds);
        totalPlayedTimeTextBox.text = $"Watched: {t.Hours}h {t.Minutes:D2} min";
    }

    private void InitializeRenderResolutionUI()
    {
        videoPlayerRenderTexture = videoPlayer.GetComponent<VideoPlayerRenderTexture>();
        renderResolutionDropdown = FindNamedComponent<TMP_Dropdown>(settingsUI, "Render Resolution Dropdown");

        PopulateRenderResolutionDropdown();
        renderResolutionDropdown.onValueChanged.AddListener(ChangeRenderResolution);
    }

    private void InitializeFrameRateUI()
    {
        showFrameRateToggle = FindNamedComponent<Toggle>(settingsUI, "Show Frame Rate Toggle");
        showFrameRateToggle.onValueChanged.AddListener(ChangeShowFrameRate);
        CreateFrameRateTextBox();
        videoPlayer.prepareCompleted += OnVideoPrepared;
    }

    private static T FindNamedComponent<T>(GameObject root, string objectName) where T : Component
    {
        return Array.Find(root.GetComponentsInChildren<T>(true), component => component.gameObject.name == objectName);
    }

    private void PopulateRenderResolutionDropdown()
    {
        renderResolutionDropdown.ClearOptions();
        var options = new List<string>(RenderResolutionPresets.All.Length);
        foreach (RenderResolutionPreset preset in RenderResolutionPresets.All)
        {
            options.Add(preset.Label);
        }

        renderResolutionDropdown.AddOptions(options);
        renderResolutionDropdown.RefreshShownValue();
    }

    private void CreateFrameRateTextBox()
    {
        frameRateTextBox = Instantiate(versionTextBox, moviePlayerObject.transform, false);
        frameRateTextBox.gameObject.name = "Frame Rate Text";
        frameRateTextBox.text = "-- FPS";
        frameRateTextBox.fontSize = 20f;
        frameRateTextBox.enableAutoSizing = false;
        frameRateTextBox.alignment = TextAlignmentOptions.TopRight;
        frameRateTextBox.raycastTarget = false;

        RectTransform rectTransform = frameRateTextBox.rectTransform;
        rectTransform.anchorMin = Vector2.one;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.pivot = Vector2.one;
        rectTransform.anchoredPosition = new Vector2(-20f, -20f);
        rectTransform.sizeDelta = new Vector2(160f, 36f);
        rectTransform.localScale = Vector3.one;
        rectTransform.localRotation = Quaternion.identity;
        rectTransform.SetAsLastSibling();
        frameRateTextBox.gameObject.SetActive(false);
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

    public void ResetSettingsButtonHandler()
    {
        confirmationDialog.Show("Reset settings to their default values?", yes =>
        {
            if (yes)
            {
                ResetSettings();
            }
        });
    }

    public void DeleteSelectedMovieButtonHandler()
    {
        string movieToDelete = movieListDropdown.captionText.text;
        string deletionConfirmationMessage = $"Are you sure you want to delete {movieToDelete} ?";
        confirmationDialog.Show(deletionConfirmationMessage, yes =>
        {
            if (yes)
            {
                DeleteSelectedMovie(movieToDelete);
            }
        });
    }

    private void ResetSettings()
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
        HandleChangeRenderResolution(settingsManager.GetRenderTextureWidth(), settingsManager.GetRenderTextureHeight());
        HandleChangeShowFrameRate(settingsManager.GetShowFrameRate());
    }

    private void PopulateMovieDropdown()
    {
        StorageHandler.InitDirectoryTree();
        List<string> availableMovies = StorageHandler.GetFileNamesFromDir(TypeSafeDir.Movies);
        // Order by name ignoring the case
        availableMovies.Sort(StringComparer.OrdinalIgnoreCase);

        movieListDropdown.ClearOptions();
        movieListDropdown.AddOptions(new List<string> { EMPTY_MOVIE_NAME });
        foreach (string movieFileName in availableMovies)
        {
            if (AllowedMovieExtensions.Contains(Path.GetExtension(movieFileName)))
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

    public void ChangeRenderResolution(int presetIndex)
    {
        if (presetIndex < 0 || presetIndex >= RenderResolutionPresets.All.Length)
        {
            return;
        }

        RenderResolutionPreset preset = RenderResolutionPresets.All[presetIndex];
        settingsManager?.SetRenderTextureResolution(preset.Width, preset.Height);
        HandleChangeRenderResolution(preset.Width, preset.Height);
    }

    public void ChangeShowFrameRate(bool showFrameRate)
    {
        settingsManager?.SetShowFrameRate(showFrameRate);
        HandleChangeShowFrameRate(showFrameRate);
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
        blobTimerDuration = blobTimerValue;
        blobTimerSlider.GetComponentInChildren<TextMeshProUGUI>().text = blobTimerValue.ToString("0.0");
    }

    private void HandleChangeRenderResolution(int width, int height)
    {
        int presetIndex = RenderResolutionPresets.FindIndex(width, height);
        videoPlayerRenderTexture.SetResolution(width, height);
        renderResolutionDropdown.SetValueWithoutNotify(presetIndex);
        renderResolutionDropdown.RefreshShownValue();
    }

    private void HandleChangeShowFrameRate(bool showFrameRate)
    {
        showFrameRateToggle.SetIsOnWithoutNotify(showFrameRate);
        frameRateTextBox.gameObject.SetActive(showFrameRate);
        if (showFrameRate && videoPlayer.isPrepared)
        {
            UpdateFrameRateText(videoPlayer);
        }
    }

    private void OnVideoPrepared(VideoPlayer preparedVideoPlayer)
    {
        if (frameRateTextBox.gameObject.activeSelf)
        {
            UpdateFrameRateText(preparedVideoPlayer);
        }
    }

    private void UpdateFrameRateText(VideoPlayer preparedVideoPlayer)
    {
        frameRateTextBox.text = preparedVideoPlayer.frameRate > 0
            ? $"{preparedVideoPlayer.frameRate:0.##} FPS"
            : "-- FPS";
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
                    settingsUI.SetActive(false);
                    break;
                }
            }
        }
    }

    private void DeleteSelectedMovie(string movieSelectedFilename)
    {
        if (movieSelectedFilename != EMPTY_MOVIE_NAME)
        {
            StorageHandler.DeleteFileFromDir(TypeSafeDir.Movies, movieSelectedFilename);
            PopulateMovieDropdown();
        }
    }

    IEnumerator RunBlobChangeTimer()
    {
        System.Random random = new System.Random();
        while (true)
        {
            yield return new WaitForSeconds(blobTimerDuration);

            float f1 = (float)(32748 * 2.0 * (random.NextDouble() - 0.5));
            float f2 = (float)(32748 * 2.0 * (random.NextDouble() - 0.5));
            dichopticFilterMaterial.SetVector("_BlobOffset", new Vector2(f1, f2));
        }
    }

    private XRInputSubsystem GetXRSubsystem()
    {
        var list = new List<XRInputSubsystem>();
        SubsystemManager.GetSubsystems(list);
        return (list.Count > 0) ? list[0] : null;
    }

    void OnTrackingOriginUpdated(XRInputSubsystem _)
    {
        isCameraInit = false;
    }

    private void UpdateCamera()
    {
        // Only on Init request
        if (!isCameraInit)
        {
            // Wait for XR system to init(Camera.main is null until then)
            if (!Camera.main)
            {
                return;
            }

            // forward direction without head tilt
            Vector3 forwardFlat = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up).normalized;
            if (forwardFlat.sqrMagnitude < 0.001f)
            {
                forwardFlat = Camera.main.transform.forward;
            }

            // move the screen in front of the user
            Vector3 p2 = Camera.main.transform.position + forwardFlat * DISTANCE_TO_SCREEN_IN_M;
            p2.y = Camera.main.transform.position.y;
            moviePlayerObject.transform.position = p2;

            // after xr init -> register origin updated delegate
            XRInputSubsystem xr = GetXRSubsystem();
            if (xr != null)
            {
                xr.trackingOriginUpdated -= OnTrackingOriginUpdated;
                xr.trackingOriginUpdated += OnTrackingOriginUpdated;
                isCameraInit = true;
            }
        }

        // every Update
        if (Camera.main)
        {
            // rotation - lazy follow behaviour
            Vector3 lookDir = moviePlayerObject.transform.position - Camera.main.transform.position;
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.001f)
            {
                moviePlayerObject.transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
            }
        }

    }
}
