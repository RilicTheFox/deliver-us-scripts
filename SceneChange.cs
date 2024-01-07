using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{
    public int targetSceneIndex;

    public void ChangeToScene()
    {
        SceneManager.LoadScene(targetSceneIndex);
    }
}
