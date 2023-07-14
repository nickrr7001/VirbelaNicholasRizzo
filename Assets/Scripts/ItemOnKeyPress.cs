using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Creates a new item (Q) or bot (E) on it's resepected key press.
/// </summary>
public class ItemOnKeyPress : MonoBehaviour
{
    private ItemDistanceManager idm;
    private void Start()
    {
        idm = ItemDistanceManager.GetInstance();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
            Instantiate(idm.m_bot,GetRandPos(),Quaternion.identity).transform.parent = idm.m_botRoot;
        if (Input.GetKeyDown(KeyCode.Q))
            Instantiate(idm.m_item, GetRandPos(), Quaternion.identity).transform.parent = idm.m_itemRoot;
    }
    /// <summary>
    /// Get a random position between -15.0f and 15.0f on the x,y, and z axis
    /// </summary>
    /// <returns>Randomly generated position position between -15.0f and 15.0f on the x,y, and z axis</returns>
    private Vector3 GetRandPos()
    {
        return new Vector3(Random.Range(-15f, 15f), Random.Range(-15f, 15f), Random.Range(-15f, 15f));
    }
}
