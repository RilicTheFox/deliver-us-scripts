using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    private void Start()
    {
        Time.timeScale = 0f;
        GetComponent<PlayerInput>().currentActionMap.FindAction("Pause").performed += PausedPressed;
    }

    public void Quit()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void Resume()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }

    private void PausedPressed(InputAction.CallbackContext context)
    {
        Resume();
    }
}
