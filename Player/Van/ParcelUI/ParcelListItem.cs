using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ParcelListItem : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text addressText;
    [SerializeField] private GameObject deliveredIcon;
    [SerializeField] private GameObject damagedIcon;
    [SerializeField] private Animator blinkAnimator;

    private Parcel _parcel;
    
    public Action<Parcel> OnClicked;
    
    public void Init(Parcel parcel, bool isNextParcel, bool notInVan)
    {
        if (parcel != _parcel)
        {
            parcel.Damaged += UpdateParcel;
            
            if (_parcel != null)
                _parcel.Damaged -= UpdateParcel;
        }

        _parcel = parcel;

            // If outside the van
        if (parcel.isActiveAndEnabled)
        {
            
            if (button != null && button.enabled)
                button.interactable = false;
        }
        
        addressText.text = parcel.RecipientName + "\n" + parcel.DeliveryLocation.GetAddress();

        if (isNextParcel)
        {
            blinkAnimator.enabled = true;
        }

        if (damagedIcon != null)
            damagedIcon.SetActive(parcel.IsDamaged);

        if (deliveredIcon != null && button != null)
        {
            deliveredIcon.SetActive(parcel.IsDelivered);
            button.interactable = !parcel.IsDelivered;
        }

        if (notInVan)
        {
            button.interactable = false;
        }
    }

    public void UpdateParcel(Parcel parcel)
    {
        Init(_parcel, false, false);
    }

    public void SelectParcel()
    {
        OnClicked?.Invoke(_parcel);
    }
}
