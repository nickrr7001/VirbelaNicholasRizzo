using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// The Item class registers itself with the ItemDistanceManager
/// the item is then destroyed as past the start method it is no longer needed.
/// </summary>
public class Item : MonoBehaviour
{
    private void Start()
    {
        ItemDistanceManager.AddItem(this);
        Destroy(this);
    }
}
