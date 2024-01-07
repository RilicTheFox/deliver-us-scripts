using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MoneyTickerText : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private Color positiveColor;
    [SerializeField] private Color negativeColor;
    
    private void Start()
    {
        StartCoroutine(AliveTimer());
    }

    private IEnumerator AliveTimer()
    {
        yield return new WaitForSeconds(3);
        Destroy(gameObject);
    }

    public void SetAmount(int amount)
    {
        string op;
        
        if (amount < 0)
        {
            text.color = negativeColor;
            op = "-";
        }
        else
        {
            text.color = positiveColor;
            op = "+";
        }
        
        text.text = $"{op}¤ {Mathf.Abs(amount)}";
    }
}
