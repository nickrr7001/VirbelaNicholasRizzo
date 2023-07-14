using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Registers the player with the ItemDistanceManager object.
/// </summary>
public class Player : MonoBehaviour
{
    private void Start()
    {
        ItemDistanceManager.SetPlayer(this);
    }
    /// <summary>
    /// Adding in movement controls
    /// Doesn't feel right to not be able to move the player :)
    /// </summary>
    private void Update()
    {
        float x = Input.GetAxis("Horizontal") * Time.deltaTime * 5f;
        float z = Input.GetAxis("Vertical") * Time.deltaTime *5f;
        transform.position += (transform.forward * z) + (transform.right * x);
    }
}
