using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public static class ViewHelper
{
    public static GameObject SetBorder(this GameMap map, Vector2Int pos)
    {
        //var prefab = Resources.Load<GameObject>("Prefabs/Border");
        var prefab = map.Setup.BorderPrefab;
        var obj = Object.Instantiate(prefab, map.Parent);
        obj.transform.localPosition = new Vector3(pos.x, pos.y, 0);
        obj.transform.localScale = Vector3.one;
        obj.name = "b " + pos.ToString();
        map.Set(obj, pos);
        return obj;
    }

    public static GameObject SetFiller(this GameMap map, Vector2Int pos)
    {
        //var prefab = Resources.Load<GameObject>("Prefabs/Filler");
        var prefab = map.Setup.FillerPrefab;
        var obj = Object.Instantiate(prefab, map.Parent);
        obj.transform.localPosition = new Vector3(pos.x, pos.y, 0);
        obj.transform.localScale = Vector3.one;
        obj.name = "f " + pos.ToString();
        map.Set(obj, pos);
        return obj;
    }

    public static GameObject SetCover(this GameMap map, Vector2Int pos)
    {
        var prefab = map.Setup.CoverPrefab;
        var obj = Object.Instantiate(prefab, map.Parent);
        obj.transform.localPosition = new Vector3(pos.x, pos.y, 0);
        obj.transform.localScale = Vector3.one;
        obj.name = "c " + pos.ToString();
        map.Set(obj, pos);
        return obj;
    }
}
