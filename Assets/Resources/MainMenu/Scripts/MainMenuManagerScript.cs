using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManagerScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StorageHandler.InitDirectoryTree();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public static void LoadMainMenuScene()
    {
        SceneManager.LoadScene("Scenes/MainMenu");
    }
    public void HandleDichopticMovieSceneButton()
    {
        SceneManager.LoadScene("Scenes/DichopticMovie");
    }

    public void HandleConvergencePyramidSceneButton()
    {
        SceneManager.LoadScene("Scenes/ConvergencePyramid");
    }

}
