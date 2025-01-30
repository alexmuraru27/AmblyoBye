using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using System.Collections.Generic;

public class DichopticMovieSceneManager : MonoBehaviour
{
    public static DichopticMovieSceneManager Instance;

    [SerializeField]
    public VideoPlayer moviePlayer;
    [SerializeField]
    public GameObject dichopticFilter;

    [SerializeField]
    public TMP_Dropdown movieListDropdown;

    void Awake()
    {
        Instance = this;
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
        movieListDropdown.ClearOptions();
        movieListDropdown.AddOptions(availableMovies);
        // Populate Settings with the menu
    }

    public void ChangeBlobClipping(GameObject clippingObject)
    {

    }

    public void ChangeBlobScale(GameObject scaleObject)
    {

    }

    public void ChangeBlobGreyValue(GameObject greyObject)
    {

    }

    public void ChangeBlobTimerValue(GameObject timerObject)
    {

    }

    public void ChangeEyeFilterToggle(GameObject eyeFilterObject)
    {

    }

    public void ToggleSettingsMenuVisibility(GameObject settingsMenuObject)
    {
        settingsMenuObject.SetActive(!settingsMenuObject.activeSelf);
    }

    public void LoadMovieButtonHandle(TMP_Dropdown movieListDropdownObject)
    {

    }
}
