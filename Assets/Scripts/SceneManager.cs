using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
    public string sceneName = "Main";
    public void ChangeScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        Debug.Log("Hi");
    }
}
