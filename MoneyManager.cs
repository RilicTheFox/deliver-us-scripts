using System;
using UnityEngine;

public class MoneyManager : MonoBehaviour
{

    public static MoneyManager Instance;
    
    public int CurrentMoney
    {
        get => _currentMoney;
        set
        {
            MoneyChanged?.Invoke(_currentMoney, value);
            
            if (value > _currentMoney)
                Debug.Log("Earned ¤" + (value - _currentMoney));
            else if (value < _currentMoney)
                Debug.Log("Lost ¤" + (_currentMoney - value));             
            
            _currentMoney = value;
            
            Debug.Log("You now have: ¤" + _currentMoney);
        }
    }

    private int _currentMoney;

    public Action<int, int> MoneyChanged; // Old money, new money

    private void OnDestroy()
    {
        Shuttle.Instance.money = _currentMoney;
    }

    public void Init()
    {
        // If Instance isn't null and isn't me, destroy myself.
        if (Instance != null && Instance != this)
            Destroy(this.gameObject);
        else
            Instance = this;
        
        if (Shuttle.Instance != null)
            _currentMoney = Shuttle.Instance.money;
    }

    public void SpawnPopup(float amount)
    {
        
    }
}
