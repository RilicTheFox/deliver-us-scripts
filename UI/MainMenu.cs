using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Animator blackCoverAnimator;
    [FormerlySerializedAs("MainMenuGroup")] [SerializeField] private GameObject mainMenuGroup;
    [FormerlySerializedAs("InstructionsGroup")] [SerializeField] private GameObject instructionsGroup;
    [SerializeField] private GameObject creditsGroup;

    private void Start()
    {
        blackCoverAnimator.Play("FadeFromBlackAnim");
        
        if (Shuttle.Instance != null)
            Destroy(Shuttle.Instance.gameObject);
    }

    public void OnPlayPressed()
    {
        StartCoroutine(PlayGame());
    }

    public void OnQuitPressed()
    {
        StartCoroutine(QuitGame());
    }

    public void OnCreditsPressed()
    {
        mainMenuGroup.SetActive(false);
        creditsGroup.SetActive(true);
    }

    public void OnCreditsBackButtonPressed()
    {
        creditsGroup.SetActive(false);
        mainMenuGroup.SetActive(true);
    }

    private IEnumerator PlayGame()
    {
        blackCoverAnimator.Play("FadeToBlackAnim");
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(blackCoverAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.length);
        SceneManager.LoadSceneAsync("Tutorial");
    }

    private IEnumerator QuitGame()
    {
        blackCoverAnimator.Play("FadeToBlackAnim");
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(blackCoverAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.length);
        Application.Quit();
    }
}
