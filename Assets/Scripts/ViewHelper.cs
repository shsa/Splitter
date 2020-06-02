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
        map.Set(obj, pos, ItemType.Border);
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
        map.Set(obj, pos, ItemType.Filler);
        return obj;
    }

    public static GameObject SetCover(this GameMap map, Vector2Int pos)
    {
        var prefab = map.Setup.CoverPrefab;
        var obj = Object.Instantiate(prefab, map.Parent);
        obj.transform.localPosition = new Vector3(pos.x, pos.y, 0);
        obj.transform.localScale = Vector3.one;
        obj.name = "c " + pos.ToString();
        map.Set(obj, pos, ItemType.Cover);
        return obj;
    }

    public static GameObject SetLine(this GameMap map, Vector2Int pos)
    {
        var prefab = map.Setup.LinePrefab;
        var obj = Object.Instantiate(prefab, map.Parent);
        obj.transform.localPosition = new Vector3(pos.x, pos.y, 0);
        obj.transform.localScale = Vector3.one;
        obj.name = "l " + pos.ToString();
        map.Set(obj, pos, ItemType.Line);
        return obj;
    }

    public static Transform CreatePlayer(this GameScript game)
    {
        var obj = Object.Instantiate(game.Setup.PlayerPrefab, game.transform);
        obj.transform.localScale = Vector3.one;
        obj.name = "Player";
        obj.tag = "Player";
        return obj.transform;
    }

    public static Transform CreateEnemy(this GameScript game)
    {
        var obj = Object.Instantiate(game.Setup.EnemyPrefab, game.transform);
        obj.transform.localScale = Vector3.one;
        obj.name = "Enemy";
        obj.tag = "Enemy";
        return obj.transform;
    }
}
