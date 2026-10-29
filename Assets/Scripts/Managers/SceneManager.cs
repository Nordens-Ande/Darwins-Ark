using UnityEngine;

/// <summary>
/// This is supposed to be a global class that handles scene management in the game. 
/// It is a singleton class that can be accessed from anywhere in the project.
/// </summary>


public class SceneManager : SingletonManager<SceneManager>
{



    protected override void Awake()
    {
        base.Awake();

        // Make sure this object is not destroyed when loading a new scene
        // because its the manager responsible for changing scenes and must persist across scenes
        if (Instance == this)
        {
            DontDestroyOnLoad(gameObject);
        }

    }




    // Change scene by name
    public void ChangeSceneByName(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }


    // Change scene by index
    public void ChangeSceneByIndex(int sceneIndex)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
    }


    // quit the application
    public void QuitApplication()
    {
        Application.Quit();

        // this is just testing for the unity editor
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    // Reload the current scene
    public void ReloadCurrentScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }



}
