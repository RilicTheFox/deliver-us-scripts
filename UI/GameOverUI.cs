using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TMP_Text reasonText;
    [SerializeField] private Animator blackFade;

    public string reason;

    private void Start()
    {
        blackFade.Play("FadeFromBlackAnim");
    }

    public void OnMenuButtonPressed()
    {
        StartCoroutine(FadeToMenu());
    }

    private IEnumerator FadeToMenu()
    {
        blackFade.Play("FadeToBlackAnim");
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(blackFade.GetCurrentAnimatorClipInfo(0)[0].clip.length);
        SceneManager.LoadScene("MainMenu");
    }
}
