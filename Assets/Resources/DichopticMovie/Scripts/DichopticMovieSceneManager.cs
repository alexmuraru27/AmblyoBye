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
        List<string> availableMovies = StorageHandler.GetFilePathsFromDir(TypeSafeDir.Movies);
        Debug.Log(availableMovies);
        Debug.Log(availableMovies.Count);
        foreach (string movie in availableMovies)
        {
            Debug.Log(movie);
        }
        // Populate Settings with the menu
    }

    public void ChangeBlobClipping(float clippingValue)
    {

    }

    public void ChangeBlobScale(float scaleValue)
    {

    }

    public void ChangeBlobGreyValue(int greyValue)
    {

    }

    public void ChangeBlobTimerValue(int timerValue)
    {

    }

    public void ChangeEyeFilterToggle(GameObject eyeFilterValue)
    {

    }

    public void ToggleSettingsMenuVisibility(GameObject settingsMenu)
    {

    }

    public void LoadMovieButtonHandle()
    {

    }
}
