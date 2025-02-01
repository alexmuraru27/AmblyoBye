using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManagerScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StorageHandler.InitAllFS();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void HandleDichopticMovieButton()
    {
        SceneManager.LoadScene("Scenes/DichopticMovieScene");
    }
}
