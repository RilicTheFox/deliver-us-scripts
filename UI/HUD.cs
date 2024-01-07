using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [SerializeField] private Animator blackFade;
    [SerializeField] private Animator whiteFlash;
    [SerializeField] private TMP_Text money;
    [SerializeField] private TMP_Text clock;
    [SerializeField] private TMP_Text parcelsLeft;
    [SerializeField] private Slider stressSlider;
    [SerializeField] private GameObject moneyTicker;
    [SerializeField] private TMP_Text speedometerText;
    [SerializeField] private ParcelListItem parcelItem;
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private TMP_Text messageText;

    private MoneyTickerText _moneyTickerPrefab;

    public Action FinishedFadeToBlack;
    
    public void Init(StressManager stressManager, PlayerVehicle player)
    {
        stressManager.StressUpdated += SetStress;
        SetStress(stressManager.Stress);

        _moneyTickerPrefab = Resources.Load<MoneyTickerText>("UI/MoneyTickerText");
        
        MoneyManager.Instance.MoneyChanged += SetMoney;
        SetMoney(0, MoneyManager.Instance.CurrentMoney);

        player.GetComponent<CarController>().SpeedChanged += SetSpeedometer;
        player.NextParcelUpdated += SetCurrentParcel;
        SetCurrentParcel(player.Parcels[0].GetComponent<Parcel>());
        
        messagePanel.SetActive(false);
        
        blackFade.Play("FadeFromBlackAnim");
        whiteFlash.StopPlayback();

        SpeedTrap.PlayerCaughtSpeeding += Flash;
    }

    public void SetParcelsDelivered(int deliveredParcels, int numParcelsTotal)
    {
        parcelsLeft.text = $"{deliveredParcels} / {numParcelsTotal}";
    }

    public void SetClock(int count)
    {
        clock.text = $"{9 + Mathf.FloorToInt(count / 60f):00}:{count % 60:00}";
    }

    public void SetMoney(int oldMoney, int newMoney)
    {
        money.text = $"¤{newMoney}";
        
        // Spawn popup
        MoneyTickerText moneyPopup = Instantiate(_moneyTickerPrefab, moneyTicker.transform);
        moneyPopup.SetAmount(newMoney - oldMoney);
    }

    public void SetStress(float stress)
    {
        stressSlider.value = stress;
    }

    public void SetSpeedometer(float speed)
    {
        float convertedSpeed = speed * 5f; // Not actual conversion  to kmh, it's just for fun
        speedometerText.text = $"{convertedSpeed:000}";
    }

    public void SetCurrentParcel(Parcel parcel)
    {
        parcelItem.Init(parcel, false, false);
    }

    public void FadeToBlack()
    {
        StartCoroutine(FadeToBlackEnum());
    }

    public void DisplayMessage(string message)
    {
        StartCoroutine(DisplayMessageEnum(message));
    }

    public void Flash(float speed)
    {
        whiteFlash.Play("FlashAnim");
    }

    private IEnumerator DisplayMessageEnum(string message)
    {
        messagePanel.SetActive(true);
        messageText.text = message;
        yield return new WaitForSeconds(5);
        messagePanel.SetActive(false);
    }

    private IEnumerator FadeToBlackEnum()
    {
        blackFade.Play("FadeToBlackAnim");
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(blackFade.GetCurrentAnimatorClipInfo(0)[0].clip.length);
        FinishedFadeToBlack?.Invoke();
    }

    private void OnDestroy()
    {
        SpeedTrap.PlayerCaughtSpeeding -= Flash;
    }
}
