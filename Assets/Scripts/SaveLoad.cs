using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
/// <summary>
/// Saves and loads item and bot states
/// </summary>
public class SaveLoad : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            SaveData();
    }
    /// <summary>
    /// Saves current player/bot/item states
    /// </summary>
    public static void SaveData()
    {
        List<ItemData> allItems = ItemDistanceManager.GetItemData();
        SaveData saveData = new SaveData();
        saveData.Items = new List<SaveVector3>();
        saveData.Bots = new List<SaveVector3>();
        saveData.playerPosition = ItemDistanceManager.GetPlayerPosition();
        foreach (ItemData item in allItems)
        {
            SaveVector3 sv = item.mr.transform.position;
            if (item.isBot)
                saveData.Bots.Add(sv);
            else
                saveData.Items.Add(sv);
        }
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        MemoryStream ms = new MemoryStream();
        binaryFormatter.Serialize(ms, saveData);
        File.WriteAllBytes("savedata.sv", ms.ToArray());
        Debug.Log("Saved Data");
    }
    public static bool HasSavedData()
    {
        return File.Exists("savedata.sv");
    }
    /// <summary>
    /// Loads saved data
    /// </summary>
    /// <returns>SaveData object from savedata.sv binary file</returns>
    public static SaveData LoadAllData()
    {
        if (!HasSavedData())
        {
            Debug.LogError("NO SAVE DATA FOUND!");
            return null;
        }
        byte[] data = File.ReadAllBytes("savedata.sv");
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        MemoryStream ms = new MemoryStream();
        ms.Write(data, 0, data.Length);
        ms.Seek(0, SeekOrigin.Begin);
        return (SaveData)binaryFormatter.Deserialize(ms);
    }
}
/// <summary>
/// Save data container
/// </summary>
[Serializable]
public class SaveData
{
    public List<SaveVector3> Items;
    public List<SaveVector3> Bots;
    public SaveVector3 playerPosition;
}
/// <summary>
/// Serializable version of Unity's vector3 class
/// </summary>
[Serializable]
public class SaveVector3
{
    public float x;
    public float y;
    public float z;
    /// <summary>
    /// Constructor for a SaveVector3
    /// </summary>
    /// <param name="x">x position</param>
    /// <param name="y">y position</param>
    /// <param name="z">z position</param>
    public SaveVector3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z; 
    }
    /// <summary>
    /// Returns a SaveVector3 version of provided Vector3
    /// </summary>
    /// <param name="v3">Vector3 to convert to SaveVector3</param>
    public static implicit operator SaveVector3(Vector3 v3)
    {
        return new SaveVector3(v3.x, v3.y, v3.z);
    }
    /// <summary>
    /// Returns a Vector3 converted from a SaveVector3
    /// </summary>
    /// <param name="sv3">SaveVector3 to convert to Vector3</param>
    public static implicit operator Vector3(SaveVector3 sv3)
    {
        return new Vector3(sv3.x, sv3.y, sv3.z);
    }
}