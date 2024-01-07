using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Parcel : MonoBehaviour, IInteractable
{
    [Header("Parcel Settings")]
    [SerializeField] private float damageVelocity;
    [SerializeField] [Range(0, 1)] private float throwDamageChance;
    [SerializeField] private ParcelListItem hudDisplay;
    
    [Header("Contents")]
    public DeliveryLocation DeliveryLocation;
    public string RecipientName;
    public bool IsDelivered { get; private set; }
    public DeliveryLocation DeliveredTo { get; private set; }
    public int Value;
    public bool IsDamaged { get; private set; }
    public bool IsDeliveredToWrongPlace { get; private set; }
    public bool canBePickedUp = true;
    
    public Action<Parcel> Delivered;
    public Action<Parcel> Damaged;

    private void Start()
    {
        hudDisplay.Init(this, false, false);
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Player")) return;
        
        if (col.relativeVelocity.magnitude >= damageVelocity)
            Damage();
    }

    private void Damage()
    {
        if (IsDamaged) return;
        
        IsDamaged = true;
        Debug.Log($"Parcel for {RecipientName} has been damaged!");
        Damaged?.Invoke(this);
    }

    public void Deliver(DeliveryLocation deliveredTo)
    {
        IsDelivered = true;
        DeliveredTo = deliveredTo;
        IsDeliveredToWrongPlace = DeliveredTo != DeliveryLocation;
        canBePickedUp = false;
        gameObject.layer = LayerMask.NameToLayer("DeliveredParcels");

        Debug.Log("Parcel for " + RecipientName +  " delivered!");
        
        Delivered?.Invoke(this);
    }

    public void Interact(PlayerCharacter playerCharacter)
    {
        if (!canBePickedUp) return;
        playerCharacter.PickUpParcel(gameObject);
    }

    public void OnThrown()
    {
        // Chance of damage on throw
        if (Random.value <= throwDamageChance)
            Damage();
    }
}