using UnityEngine;

[CreateAssetMenu(fileName = "NewAccessory", menuName = "Wormance/Accessory")]
public class AccessoryData : ScriptableObject
{
    public string accessoryName;
    public GameObject prefab;
}