using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VanRearInteract : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject parcelMenu;
    [SerializeField] private PlayerVehicle playerVehicle;

    public void Interact(PlayerCharacter playerCharacter)
    {
        List<Parcel> parcels = playerVehicle.Parcels.Select(parcel => parcel.GetComponent<Parcel>()).ToList();

        parcelMenu.GetComponent<ParcelMenu>().Init(parcels, playerCharacter);
        parcelMenu.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<Parcel>() != null)
            playerVehicle.StoreParcel(collision.gameObject);
    }
}
