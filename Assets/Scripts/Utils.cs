using UnityEngine;

public static class Utils
{
    public static Rect GetWorldRect(RectTransform t)
    {
        var fourCorners = new Vector3[4];
        t.GetLocalCorners(fourCorners);
        var bl = t.TransformPoint(fourCorners[0]);
        //var tl = fourCorners[1];
        var tr = t.TransformPoint(fourCorners[2]);
        //var br = fourCorners[3];
        return new Rect(bl.x, bl.y, tr.x - bl.x, tr.y - bl.y);
    }

    public static Vector2 CalcStartPos(RectTransform t, Vector2 pos, Vector2 dir)
    {
        var rect = GetWorldRect(t);
        if (dir == Vector2.left)
        {
            return new Vector2(rect.x, pos.y);
        }
        if (dir == Vector2.right)
        {
            return new Vector2(rect.xMax, pos.y);
        }
        if (dir == Vector2.down)
        {
            return new Vector2(pos.x, rect.yMax);
        }
        return new Vector2(pos.x, rect.y);
    }

    public static Vector2 Max(RectTransform t, Vector2 pos, Vector2 dir)
    {
        var rect = GetWorldRect(t);
        if (dir == Vector2.left)
        {
            return new Vector2(Mathf.Min(pos.x, rect.x), pos.y);
        }
        if (dir == Vector2.right)
        {
            return new Vector2(Mathf.Max(pos.x, rect.xMax), pos.y);
        }
        if (dir == Vector2.down)
        {
            return new Vector2(pos.x, Mathf.Min(pos.y, rect.y));
        }
        return new Vector2(pos.x, Mathf.Max(pos.y, rect.yMax));
    }

    //public static GameObject CreateBorder()
    //{
    //    var texture = Resources.Load<Texture2D>("Border");
    //    var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0), texture.width);
    //    var border = new GameObject();
    //    var sr = border.AddComponent<SpriteRenderer>();
    //    sr.sprite = sprite;
    //    var collider = border.AddComponent<BoxCollider2D>();
    //    collider.size = Vector2.one;
    //    collider.offset = new Vector2(0.5f, 0.5f);
    //    collider.usedByEffector = true;
    //    collider.sharedMaterial = Resources.Load<PhysicsMaterial2D>("Material");
    //    var effector = border.AddComponent<PlatformEffector2D>();
    //    effector.useOneWay = false;
    //    effector.useSideBounce = true;
    //    return border;
    //}

    public static Vector2 ToFloat(this Vector2Int a)
    {
        return new Vector2(a.x, a.y);
    }

    public static Vector2Int Rotate90(this Vector2Int a)
    {
        // x = x0 * 0 — y0 * 1;
        // y = x0 * 1 + y0 * 0;
        // 
        // x = -y0;
        // y = x0;
        return new Vector2Int(-a.y, a.x);
    }

    static readonly Vector2 vHalf = new Vector2(0.49f, 0.49f);
    public static Vector2Int ToInt(this Vector2 v)
    {
        return Vector2Int.FloorToInt(v + vHalf);
    }
}
