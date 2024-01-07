using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ParcelMenu : MonoBehaviour
{
    [SerializeField] private Transform scrollContent;
    [SerializeField] private ParcelListItem parcelButtonPrefab;
    [SerializeField] private Button takeButton;
    
    private Parcel _selectedParcel;
    private List<Parcel> _parcels;
    private ParcelListItem[] _parcelListItems;
    private PlayerCharacter _player;
    
    public Action<Parcel> ParcelSelected;
    
    private void OnEnable()
    {
        _selectedParcel = null;
        
        _parcelListItems = new ParcelListItem[_parcels.Count];
        
        // Add the parcel buttons
        bool firstParcelPassed = false;
        for (int i = 0; i < _parcels.Count; i++)
        {
            _parcelListItems[i] = Instantiate(parcelButtonPrefab, scrollContent);
            bool notInVan = _parcels[i].isActiveAndEnabled;
            
            if (_parcels[i].IsDelivered == false && firstParcelPassed == false)
            {
                _parcelListItems[i].Init(_parcels[i], true, notInVan);
                firstParcelPassed = true;
            }
            else
            {
                _parcelListItems[i].Init(_parcels[i], false, notInVan);
            }
            
            _parcelListItems[i].OnClicked += OnParcelSelected;
        }
        
        _player.canMove = false;
    }

    private void OnDisable()
    {
        // Remove the parcel buttons
        for (int i = 0; i < _parcels.Count; i++)
        {
            _parcelListItems[i].OnClicked -= OnParcelSelected;
            Destroy(_parcelListItems[i].gameObject);
        }

        takeButton.interactable = false;
        
        _player.canMove = true;
        _player = null;
        
    }

    private void OnParcelSelected(Parcel parcel)
    {
        _selectedParcel = parcel;
        
        if (takeButton.interactable == false)
            takeButton.interactable = true;
    }

    public void OnParcelTaken()
    {
        Debug.Log("You took a parcel belonging to: " + _selectedParcel.RecipientName);

        _player.PickUpParcel(_selectedParcel.gameObject);

        gameObject.SetActive(false);
    }

    public void OnCancelled()
    {
        gameObject.SetActive(false);
    }

    public void Init(List<Parcel> parcels, PlayerCharacter player)
    {
        _parcels = parcels;
        _player = player;
    }
}
