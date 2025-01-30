using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;


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
        StorageHandler.InitFS(TypeSafeDir.Movies);
        // TODO create directories if not existent
        // TODO search for all movies available
        // Populate Settings with the menu
    }

    // Update is called once per frame
    void Update()
    {

    }

    void ChangeBlobClipping(int clippingValue)
    {

    }

    void ChangeBlobScale(int scaleValue)
    {

    }

    void ChangeBlobGreyValue(int greyValue)
    {

    }

    void ChangeEyeFilterToggle(bool eyeFilterValue)
    {

    }

    void LoadMovieButtonHandle()
    {

    }
}
