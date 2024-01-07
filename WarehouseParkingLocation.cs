using System;
using UnityEngine;

public class WarehouseParkingLocation : ParkingLocation
{
    public static Action WarehouseVisited;
    
    protected override void OnPlayerVisited()
    {
        Debug.Log("Warehouse visited!");
        WarehouseVisited?.Invoke();
    }
}
