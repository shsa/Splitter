using System.Collections.Generic;
using UnityEngine;

public class GameMap
{
    Dictionary<Vector2Int, GameObject> map = new Dictionary<Vector2Int, GameObject>();

    public GameSetup Setup;
    public Transform Parent;

    public void Clear()
    {
        foreach (var obj in map.Values)
        {
            Object.Destroy(obj);
        }
        map.Clear();
    }

    public void Set(GameObject obj, Vector2Int pos)
    {
        var mapPosition = obj.AddComponent<GameMapComponent>();
        mapPosition.position = pos;
        mapPosition.map = this;
        if (map.TryGetValue(pos, out var oldObj))
        {
            Object.Destroy(oldObj);
        }
        map[pos] = obj;
    }

    public GameObject Get(Vector2Int pos)
    {
        if (map.TryGetValue(pos, out var obj))
        {
            return obj;
        }
        return null;
    }

    public IEnumerable<GameMapComponent> GetObjects()
    {
        foreach (var obj in map.Values)
        {
            yield return obj.GetComponent<GameMapComponent>();
        }
    }

    public bool IsEmpty(Vector2Int pos)
    {
        if (map.TryGetValue(pos, out var obj))
        {
            return obj.GetComponent<CoverComponent>() != null;
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
            return obj.GetComponent<BorderComponent>() != null;
        }
        return false;
    }

    public IEnumerable<Vector2Int> GetInternal(Vector2Int pos)
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
