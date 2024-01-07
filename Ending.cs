using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Ending : MonoBehaviour
{
    public enum Types
    {
        Good,
        Bad,
        Ugly,
        Fail
    }

    [SerializeField] private Animator blackFade;
    [Header("Endings")]
    [SerializeField] private GameObject goodEnding;
    [SerializeField] private GameObject badEnding;
    [SerializeField] private GameObject uglyEnding;
    [SerializeField] private GameObject failEnding;

    private void Start()
    {
        goodEnding.SetActive(false);

        if (Shuttle.Instance != null)
        {
            switch (Shuttle.Instance.endingType)
            {
                case Types.Good:
                    goodEnding.SetActive(true);
                    break;
                case Types.Bad:
                    badEnding.SetActive(true);
                    break;
                case Types.Ugly:
                    uglyEnding.SetActive(true);
                    break;
                case Types.Fail:
                    failEnding.SetActive(true);
                    break;
                default:
                    Debug.LogError("what? no ending?");
                    break;
            }
        }
        
        
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
