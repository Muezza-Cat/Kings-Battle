using UnityEngine.SceneManagement;



//Under Construction;
public static class SceneLoader
{
    //public enum Scene
    //{
    //    Level1,
    //    Level2,
    //    Level3,
    //    LoadingScene,
    //}
    //public static Scene targetScene;


    public static bool directlyPlayGame = false;
    //Testing
    public static int targetSceneIndex = 1;

    public static void LoadScene(/*Scene targetScene*/int index)
    {
        if (index > SceneManager.sceneCountInBuildSettings - 1)
        {
            index = SceneManager.sceneCountInBuildSettings - 2;
        }
        //SceneLoader.targetScene = targetScene;
        targetSceneIndex = index;
        SceneManager.LoadScene(/*SceneLoader.Scene.LoadingScene.ToString()*/ 0);
    }
    public static void ReloadScene()
    {
        SceneManager.LoadScene(/*SceneLoader.Scene.LoadingScene.ToString()*/ SceneManager.GetActiveScene().buildIndex);
    }


    //If pressed the Restart Button after losing or winning a game;
    public static void ReloadSceneAsRestartGame()
    {
        SceneManager.LoadScene(/*SceneLoader.Scene.LoadingScene.ToString()*/ SceneManager.GetActiveScene().buildIndex);
        directlyPlayGame = true;
    }


    public static void LoaderCallback() 
    {
        SceneManager.LoadScene(/*SceneLoader.targetScene.ToString()*/ SceneLoader.targetSceneIndex);
    }
}
