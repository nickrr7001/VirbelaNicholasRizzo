using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Jobs;
/// <summary>
/// Holds the main functionality for the item distance coding challenge,
/// processes all distance data and results.
/// </summary>
public class ItemDistanceManager : MonoBehaviour
{
    private List<Transform> m_itemTransforms = new List<Transform>();
    private List<ItemData> m_itemMR = new List<ItemData>();
    private Transform m_player;
    private static ItemDistanceManager s_instance;

    [SerializeField] private Color m_botBaseColor = Color.white;
    [SerializeField] private Color m_botClosestColor = Color.blue;
    [SerializeField] private Color m_baseColor = Color.white;
    [SerializeField] private Color m_closestColor = Color.red;

    //For loading data and ItemOnKeyPress object
    public GameObject m_bot;
    public GameObject m_item;
    public Transform m_botRoot;
    public Transform m_itemRoot;

    private static bool s_didLoad = false;
    private static Vector3 s_savedPlayerPos;
    private void Awake()
    {
        //Check for duplicates, only one IDM should exist
        if (s_instance != null)
            Destroy(this);
        s_instance = this;
        if (SaveLoad.HasSavedData())
        {
            LoadSavedData();
        }
    }
    /// <summary>
    /// Loads all saved data if it exists
    /// </summary>
    private void LoadSavedData()
    {
        SaveData sd = SaveLoad.LoadAllData();
        if (sd == null)
            return;
        s_didLoad = true;
        s_savedPlayerPos = sd.playerPosition;
        for (int i = 0; i < m_botRoot.childCount; i++)
        {
            Destroy(m_botRoot.GetChild(i).gameObject);
        }
        for (int i = 0; i < m_itemRoot.childCount; i++)
        {
            Destroy(m_itemRoot.GetChild(i).gameObject);
        }
        foreach (SaveVector3 item in sd.Items)
        {
            Instantiate(m_item, item, Quaternion.identity).transform.parent = m_itemRoot;
        }
        foreach (SaveVector3 bot in sd.Bots)
        {
            Instantiate(m_bot, bot, Quaternion.identity).transform.parent = m_botRoot;
        }
    }
    /// <summary>
    /// Getter for player position
    /// </summary>
    /// <returns>The current players position</returns>
    public static Vector3 GetPlayerPosition() { return s_instance.m_player.position; }
    /// <summary>
    /// Getter for the list of item and bot data
    /// </summary>
    /// <returns>Returns current list of item and bot data</returns>
    public static List<ItemData> GetItemData() { return s_instance.m_itemMR; }
    /// <summary>
    /// Getter for Item Distance Manager singleton
    /// </summary>
    /// <returns>Item Distance Manager Instance</returns>
    public static ItemDistanceManager GetInstance() { return s_instance; }
    /// <summary>
    /// Sets the current player for distance calculations
    /// </summary>
    /// <param name="p">Player objec to calcuate distance with</param>
    public static void SetPlayer(Player p)
    {
        if (s_instance.m_player != null)
        {
            Debug.LogWarning("Player has already been set, overriding.");
        }
        s_instance.m_player = p.transform;
        if (s_didLoad)
            s_instance.m_player.position = s_savedPlayerPos;
    }
    /// <summary>
    /// Adds and item or bot for distance calculations
    /// </summary>
    /// <param name="i">The item or bot to add</param>
    public static void AddItem(Item i)
    {
        s_instance.m_itemTransforms.Add(i.transform);
        MeshRenderer mr = i.GetComponent<MeshRenderer>();
        if (mr == null)
        {
            Debug.LogError("Mesh renderer missing on " + i.name);
            return;
        }
        ItemData id = new ItemData();
        id.mr = mr;
        id.isBot = typeof(Bot) == i.GetType();
        s_instance.m_itemMR.Add(id);
    }
    private void Update()
    {
        //We require at least one player and one item for the script to work
        if (m_player == null || m_itemTransforms.Count == 0)
        {
            Debug.LogError("Player or Item is missing!");
            return;
        }
        //We use unity jobs because if we calcuate multiple distances it's better to do it on multiple threads
        NativeArray<float> distances = new NativeArray<float>(new float[m_itemTransforms.Count],Allocator.TempJob);
        DistanceCalculation job = new DistanceCalculation() { 
            m_distances = distances,
            m_playerPosition = m_player.position
        };
        TransformAccessArray transAccess = new TransformAccessArray(m_itemTransforms.ToArray());
        JobHandle jh = job.Schedule(transAccess);
        jh.Complete();
        //In O(n) time find the closest can't do this in jobs because the operation would not be thread safe
        float closestVal = distances[0];
        int closestIndex = 0;
        for (int i = 1; i < distances.Length; i++)
        {
            if (closestVal > distances[i])
            {
                closestIndex = i;
                closestVal = distances[i];
            }
        } 
        //No longer needed, clear out the memory
        distances.Dispose();
        transAccess.Dispose();
        //Set each items respective color in O(n) time
        for (int i = 0; i < m_itemMR.Count; i++)
        {
            if (m_itemMR[i].isBot)
                m_itemMR[i].mr.material.SetColor("_Color", i == closestIndex ? m_botClosestColor : m_botBaseColor);
            else
                m_itemMR[i].mr.material.SetColor("_Color",i == closestIndex ? m_closestColor : m_baseColor);
        }
    }
}
/// <summary>
/// Holds an items mesh renderer and if the item is a bot or not.
/// </summary>
public struct ItemData
{
    public MeshRenderer mr;
    public bool isBot;
}
/// <summary>
/// Simple unity job for distance calculation
/// </summary>
public struct DistanceCalculation : IJobParallelForTransform
{
    public NativeArray<float> m_distances;
    public Vector3 m_playerPosition;
    public void Execute(int i, TransformAccess transform)
    {
        //Using square magnitude is MUCH faster then vector3.distance or magnitude as we don't have to use a square root operation
        //We also only care about what object is closest, not the specific distance so sqrMagnitude is best in this situtation
        m_distances[i] = (m_playerPosition - transform.position).sqrMagnitude;
    }
}
