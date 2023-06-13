using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "InventoryAndCrafting/SimpleItem")]
public class Item : ScriptableObject
{
    public new string name;
    [TextArea] public string description;
    public Sprite image;
    public int maxStack = 25;
}
