using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class DeliveryLocation : MonoBehaviour
{
    [SerializeField] [Multiline] private string address;
    [SerializeField] private List<string> recipients;
    
    public string GetAddress() { return address; }
    public List<string> GetRecipients() { return recipients; }

    private bool _isLocked;

    public void Lock()
    {
        _isLocked = true;
    }

    private void Awake()
    {
        _isLocked = false;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        Parcel parcel = col.GetComponent<Parcel>();
        if (parcel == null) return;

        if (_isLocked)
        {
            Debug.Log("You are out of time - you cannot deliver parcels!");
            return;
        }
        
        // Set parcel as delivered
        parcel.Deliver(this);
    }
    
    
}
