using UnityEngine;

public abstract class ParkingLocation : MonoBehaviour
{
    private PlayerVehicle _playerVehicle;
    
    // Subscribe to player stopped event when player enters 
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!(_playerVehicle = col.gameObject.GetComponent<PlayerVehicle>())) return;
        _playerVehicle.PlayerStopped += OnPlayerVisited;
        Debug.Log("Player has entered!");
    }

    // Unsubscribe when player leaves.
    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.gameObject.GetComponent<PlayerVehicle>() == null) return;
        _playerVehicle.PlayerStopped -= OnPlayerVisited;
        _playerVehicle = null;
        Debug.Log("Player has left!");
    }
    
    protected abstract void OnPlayerVisited();
}
