using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameMap
{
    Dictionary<Vector2Int, GameObject> map = new Dictionary<Vector2Int, GameObject>();
    RectInt rect;
    GameSetup setup;
    public GameSetup Setup {
        get {
            return setup;
        }
        set {
            setup = value;
            rect = new RectInt(0, 0, setup.Width, setup.Height);
        }
    }
    public Transform Parent;

    public void Clear()
    {
        foreach (var obj in map.Values)
        {
            Object.Destroy(obj);
        }
        map.Clear();
    }

    public void Set(GameObject obj, Vector2Int pos, ItemType type)
    {
        var mapInfo = obj.AddComponent<ItemMapInfo>();
        mapInfo.position = pos;
        mapInfo.map = this;
        mapInfo.type = type;
        if (map.TryGetValue(pos, out var oldObj))
        {
            Object.Destroy(oldObj);
        }
        map[pos] = obj;
    }

    public ItemMapInfo Get(Vector2Int pos)
    {
        if (map.TryGetValue(pos, out var obj))
        {
            return obj.GetComponent<ItemMapInfo>();
        }
        return null;
    }

    public ItemType GetItemType(Vector2Int pos)
    {
        if (map.TryGetValue(pos, out var obj))
        {
            return obj.GetComponent<ItemMapInfo>().type;
        }
        if (rect.Contains(pos))
        {
            return ItemType.Empty;
        }
        else
        {
            return ItemType.Outside;
        }
    }

    public IEnumerable<ItemMapInfo> GetObjects()
    {
        foreach (var obj in map.Values.ToArray())
        {
            yield return obj.GetComponent<ItemMapInfo>();
        }
    }

    public IEnumerable<ItemMapInfo> GetObjects(ItemType type)
    {
        foreach (var obj in map.Values.ToArray())
        {
            var info = obj.GetComponent<ItemMapInfo>();
            if (info.type == type)
            {
                yield return info;
            }
        }
    }

    public bool IsEmpty(Vector2Int pos)
    {
        if (map.TryGetValue(pos, out var obj))
        {
            return obj.GetComponent<ItemMapInfo>().type == ItemType.Cover;
        }
        return true;
    }

    public bool Contains(Vector2Int pos)
    {
        return !IsEmpty(pos);
    }

    public bool IsBorder(Vector2Int pos)
    {
        if (map.TryGetValue(pos, out var obj))
        {
            return obj.GetComponent<ItemMapInfo>().type == ItemType.Border;
        }
        return false;
    }

    public IEnumerable<Vector2Int> GetFillArea(Vector2Int pos)
    {
        if (Contains(pos))
        {
            yield break;
        }

        var stack = new Stack<Vector2Int>();
        var cache = new HashSet<Vector2Int>();

        stack.Push(pos);
        cache.Add(pos);

        var rect = new Rect(0, 0, Setup.Width, Setup.Height);

        void add(Vector2Int p)
        {
            if (cache.Contains(p))
            {
                return;
            }
            if (rect.Contains(p) && IsEmpty(p))
            {
                stack.Push(p);
                cache.Add(p);
            }
        }

        while (stack.Count > 0)
        {
            var p0 = stack.Pop();
            yield return p0;

            add(p0 + Vector2Int.up);
            add(p0 + Vector2Int.right);
            add(p0 + Vector2Int.down);
            add(p0 + Vector2Int.left);
        }
        yield break;
    }
}
