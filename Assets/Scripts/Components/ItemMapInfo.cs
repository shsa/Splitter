using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    Outside,
    Empty,
    Cover,
    Filler,
    Border,
    Line
}

public class ItemMapInfo : MonoBehaviour
{
    public GameMap map;
    public Vector2Int position;
    public ItemType type;
    public int id;
}
