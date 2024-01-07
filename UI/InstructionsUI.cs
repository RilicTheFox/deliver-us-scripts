using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InstructionsUI : MonoBehaviour
{
    [SerializeField] private Transform pageContainer;

    private int _currentPageNumber = 0;
    private GameObject[] _pages;

    private void Start()
    {
        _pages = new GameObject[pageContainer.childCount];
        
        int i = 0;
        foreach (Transform child in pageContainer)
        {
            _pages[i] = child.gameObject;
            i++;
        }
    }

    public void OnNextPressed()
    {
        _pages[_currentPageNumber].SetActive(false);
        _currentPageNumber++;

        if (_currentPageNumber >= _pages.Length)
            _currentPageNumber = 0;
        
        _pages[_currentPageNumber].SetActive(true);
    }

    public void OnBackPressed()
    {
        _pages[_currentPageNumber].SetActive(false);
        _currentPageNumber--;
        
        if (_currentPageNumber < 0)
            _currentPageNumber = _pages.Length - 1;
        
        _pages[_currentPageNumber].SetActive(true);
    }
}
