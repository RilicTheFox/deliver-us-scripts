using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Tutorial : MonoBehaviour
{
    [SerializeField] private GameObject[] pages;
    [SerializeField] private Animator blackFade;

    private int _currentPage;
    
    private void Start()
    {
        foreach (GameObject page in pages)
        {
            page.SetActive(false);
        }
        
        pages[0].SetActive(true);
        
        blackFade.Play("FadeFromBlackAnim");
    }

    public void NextPage()
    {
        pages[_currentPage].SetActive(false);

        if (_currentPage + 1 >= pages.Length)
            _currentPage = 0;
        else
            _currentPage++;
        
        pages[_currentPage].SetActive(true);
    }

    public void PreviousPage()
    {
        pages[_currentPage].SetActive(false);

        if (_currentPage - 1 < 0)
            _currentPage = pages.Length - 1;
        else
            _currentPage--;
        
        pages[_currentPage].SetActive(true);
    }

    public void OnPlayPressed()
    {
        StartCoroutine(PlayGame());
    }

    private IEnumerator PlayGame()
    {
        blackFade.Play("FadeToBlackAnim");
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(blackFade.GetCurrentAnimatorClipInfo(0)[0].clip.length);
        SceneManager.LoadScene("GameLevel");
    }
}
