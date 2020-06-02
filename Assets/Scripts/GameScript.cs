using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameScript : MonoBehaviour
{
    public GameSetup Setup;

    public struct WayPoint
    {
        public Vector2Int position;
        public Vector2Int dir;

        public WayPoint(Vector2Int pos, Vector2Int dir)
        {
            this.position = pos;
            this.dir = dir;
        }
    }
    List<WayPoint> way = new List<WayPoint>();

    static readonly Vector2Int imposiblePosition = new Vector2Int(int.MaxValue, int.MaxValue);
    RectInt gameRect;
    GameMap map = new GameMap();
    RectTransform gameUI;
    Transform game;
    GameInput input;
    Transform player;
    Transform finish;
    Transform enemy;
    GameObject image;
    Vector2 speed = Vector2.one;
    Vector2Int startPos;
    Vector2Int finishPos;
    Vector2Int dir;
    Vector2Int nextDir;


    // Start is called before the first frame update
    void Start()
    {
        gameUI = GameObject.Find("GameUI").GetComponent<RectTransform>();
        game = GameObject.Find("Game").transform;
        input = game.gameObject.AddComponent<GameInput>();
        input.Setup = Setup;

        player = GameObject.Find("Player").transform;
        finish = GameObject.Find("Finish").transform;
        enemy = GameObject.Find("Enemy").transform;
        image = GameObject.Find("Image");

        map.Setup = Setup;
        map.Parent = GameObject.Find("Borders").transform;
        enemy.GetComponent<EnemyScript>().map = map;
        GameInit();
    }

    void Update()
    {
        ViewSetup();
        UpdateInput();
        UpdateGame();
    }

    void UpdateGame()
    {
        void update(Vector2 delta)
        {
            var curNext = (Vector2)player.localPosition + delta;

            if (Vector2.Dot(finishPos - curNext, dir) < 0)
            {
                delta = curNext - finishPos; 
                curNext = finishPos;
            }
            player.localPosition = curNext;

            var mapPos = player.localPosition.ToInt();
            var pos = mapPos;
            while (Vector2.Dot(curNext - pos, dir) >= 0)
            {
                if (map.GetItemType(pos) == ItemType.Cover)
                {
                    map.SetLine(pos);
                    way.Add(new WayPoint(pos, dir));
                }
                pos += dir;
            }

            if (Vector2.Dot(finishPos - curNext, dir) <= 0)
            {
                startPos = finishPos;
                if (nextDir != Vector2Int.zero)
                {
                    ChangeDir(nextDir);
                    nextDir = Vector2Int.zero;

                    if (dir.sqrMagnitude > 0 && delta.sqrMagnitude > 0)
                    {
                        update((Vector2)dir * delta.magnitude);
                    }
                }
                else
                {
                    OnFinish();
                }
            }
        }

        if (dir != Vector2Int.zero)
        {
            var delta = Vector2.Lerp(Vector2.zero, dir * Setup.Speed, Time.deltaTime);
            update(delta);
        }
    }

    void ChangeDir(Vector2Int newDir)
    {
        dir = newDir;
        finishPos = startPos;
        var wayType = map.GetItemType(startPos + dir);
        switch (wayType)
        {
            case ItemType.Outside:
                {
                    OnFinish();
                }
                break;
            default:
                {
                    if (wayType == ItemType.Border && map.GetItemType(startPos) == ItemType.Line)
                    {
                        OnFinish();
                    }
                    else
                    {
                        finishPos = startPos;
                        while (map.GetItemType(finishPos + dir) == wayType)
                        {
                            finishPos += dir;
                        }
                        player.localPosition = (Vector2)startPos;
                        finish.localPosition = (Vector2)finishPos;
                    }
                }
                break;
        }
    }

    Vector2Int FindStartPos(Vector2Int start, Vector2Int dir)
    {
        // find nearest start point to direction
        var pos = start;
        if (dir == Vector2Int.up)
        {
            pos = new Vector2Int(start.x, 0);
        }
        else
        if (dir == Vector2Int.down)
        {
            pos = new Vector2Int(start.x, gameRect.yMax - 1);
        }
        else
        if (dir == Vector2Int.left)
        {
            pos = new Vector2Int(gameRect.xMax - 1, start.y);
        }
        else
        if (dir == Vector2Int.right)
        {
            pos = new Vector2Int(0, start.y);
        }

        var result = imposiblePosition;
        while (gameRect.Contains(pos))
        {
            if (map.IsBorder(pos) && map.IsEmpty(pos + dir))
            {
                if ((start - result).sqrMagnitude > (start - pos).sqrMagnitude)
                {
                    result = pos;
                }
            }

            pos += dir;
        }

        if (result == imposiblePosition)
        {
            return start;
        }
        return result;
    }

    void UpdateInput()
    {
        if (input.direction != Vector2Int.zero)
        {
            if (dir == Vector2Int.zero)
            {
                if (input.position == Vector2Int.zero)
                {
                    // start from current position
                    startPos = player.localPosition.ToInt();
                }
                else
                {
                    // find from touch
                    startPos = FindStartPos(input.position, input.direction);
                }
                ChangeDir(input.direction);
            }
            else
            if (dir != input.direction)
            {
                if (Vector2.Dot(dir, input.direction) < 0)
                {
                    // if change direction back to front
                    if (map.GetItemType(finishPos) == ItemType.Cover)
                    {
                        return;
                    }
                }

                finishPos = player.localPosition.ToInt();
                if (Vector2.Dot(finishPos - (Vector2)player.localPosition, dir) < 0)
                {
                    finishPos += dir;
                }
                nextDir = input.direction;
            }
        }
    }

    float gameWidth = 0;
    float gameHeight = 0;
    void ViewSetup()
    {
        if (gameUI.rect.width == gameWidth && gameUI.rect.height == gameHeight)
        {
            return;
        }

        gameWidth = gameUI.rect.width;
        gameHeight = gameUI.rect.height;

        var arf = gameUI.GetComponent<AspectRatioFitter>();
        arf.aspectRatio = Setup.Width * 1.0f / Setup.Height;

        var rect = Utils.GetWorldRect(gameUI);

        var cellSize = rect.width / Setup.Width;
        game.localScale = new Vector3(cellSize, cellSize, 1);
        game.position = new Vector3(-rect.width / 2 + cellSize / 2, -rect.height / 2 + cellSize / 2, 0);

        var imageTexture = Resources.Load<Texture2D>("Images/Level-001");
        var k = imageTexture.width * 1.0f / imageTexture.height;
        var imageWidth = imageTexture.width;
        var imageHeight = imageTexture.height;
        if (k < arf.aspectRatio)
        {
            imageHeight = Mathf.RoundToInt(imageWidth / arf.aspectRatio);
        }
        else
        {
            imageWidth = Mathf.RoundToInt(imageHeight * arf.aspectRatio);
        }

        var sprite = Sprite.Create(imageTexture,
            new Rect(
                (imageTexture.width - imageWidth) * 0.5f,
                (imageTexture.height - imageHeight) * 0.5f,
                imageWidth, imageHeight),
            new Vector2(0.5f, 0.5f), 1);
        sprite.name = imageTexture.name;
        var sr = image.GetComponent<SpriteRenderer>();
        sr.sprite = sprite;
        var scale = rect.width * 1.0f / imageWidth;
        image.transform.localScale = new Vector3(scale, scale, 1);

        var sc = enemy.GetComponent<EnemyScript>();
        speed = Vector2.one * Setup.Speed * cellSize;
        sc.velocity = sc.velocity.normalized * speed;
        var rb = enemy.GetComponent<Rigidbody2D>();
        if (rb.velocity.sqrMagnitude == 0)
        {
            rb.velocity = Vector2.one;
        }
        rb.velocity = rb.velocity.normalized * speed;
    }

    void GameInit()
    {
        gameRect = new RectInt(0, 0, Setup.Width, Setup.Height);

        map.Clear();
        var maxX = Setup.Width - 1;
        var maxY = Setup.Height - 1;
        for (int j = 0; j <= maxY; j++)
        {
            for (int i = 0; i <= maxX; i++)
            {
                if (i == 0 || j == 0 || i == maxX || j == maxY)
                {
                    map.SetBorder(new Vector2Int(i, j));
                }
                else
                {
                    map.SetCover(new Vector2Int(i, j));
                }
            }
        }

        enemy.localPosition = new Vector2(maxX / 2, maxY / 2);
        var sc = enemy.GetComponent<EnemyScript>();
        var rb = enemy.GetComponent<Rigidbody2D>();
        var dir = Quaternion.Euler(0, 0, 45) * Vector3.right;
        rb.velocity = Vector2.one * speed;
        //sc.velocity = dir * Setup.Speed;
    }

    int Count(Vector2Int pos)
    {
        var sum = 0;
        foreach (var p in map.GetFillArea(pos))
        {
            sum++;
        }
        return sum;
    }

    void Fill(Vector2Int pos, Vector2Int dir)
    {
        var list = new List<Vector2Int>();
        foreach (var p in map.GetFillArea(pos))
        {
            map.SetFiller(p);
            list.Add(p);
        }

        foreach (var border in map.GetObjects(ItemType.Border))
        {
            var p = border.position;
            var borderIn = map.Contains(p + Vector2Int.up)
                && map.Contains(p + Vector2Int.up + Vector2Int.right)
                && map.Contains(p + Vector2Int.right)
                && map.Contains(p + Vector2Int.down + Vector2Int.right)
                && map.Contains(p + Vector2Int.down)
                && map.Contains(p + Vector2Int.down + Vector2Int.left)
                && map.Contains(p + Vector2Int.left)
                && map.Contains(p + Vector2Int.up + Vector2Int.left);
            if (borderIn)
            {
                map.SetFiller(p);
                list.Add(p);
            }
        }

        var min = new Vector2Int(int.MaxValue, int.MaxValue);
        var max = new Vector2Int(int.MinValue, int.MinValue);
        foreach (var p in list)
        {
            min = Vector2Int.Min(min, p);
            max = Vector2Int.Max(max, p);
        }

        var i0 = min.x;
        var i1 = max.x;
        var step = 1;
        if (dir == Vector2Int.up)
        {
            i0 = min.y;
            i1 = max.y;
            step = 1;
        }
        else
        if (dir == Vector2Int.down)
        {
            i0 = max.y;
            i1 = min.y;
            step = -1;
        }
        else
        if (dir == Vector2Int.left)
        {
            i0 = max.x;
            i1 = min.x;
            step = -1;
        }

        var pauseTime = 0.01f;
        var pause = 0f;
        //var pause = new WaitForSeconds(0.01f);
        while (i0 != (i1 + step))
        {
            foreach (var p in list)
            {
                if (dir == Vector2Int.up || dir == Vector2Int.down)
                {
                    if (i0 != p.y)
                    {
                        continue;
                    }
                }
                else
                {
                    if (i0 != p.x)
                    {
                        continue;
                    }
                }

                var time = 0.5f;
                //iTween.ScaleFrom(obj, Vector3.zero, time);
                var obj = map.Get(p);
                //iTween.ColorFrom(obj, iTween.Hash("color", Color.clear, "time", time, "delay", pause));
                iTween.ColorTo(obj.gameObject, iTween.Hash("color", Color.clear, "time", time, "delay", pause));
            }
            //yield return pause;
            pause += pauseTime;

            i0 += step;
        }
    }

    void OnFinish()
    {
        dir = Vector2Int.zero;
        input.direction = Vector2Int.zero;

        foreach (var w in way)
        {
            map.SetBorder(w.position);
        }

        foreach (var w in way)
        {
            var d = w.dir.Rotate90();
            var p0 = w.position + d;
            var p1 = w.position - d;

            if (map.IsEmpty(p0) && map.IsEmpty(p1))
            {
                var s0 = Count(p0);
                var s1 = Count(p1);

                var pos = p1;
                if (s0 < s1)
                {
                    pos = p0;
                }

                Fill(pos, w.dir);
            }
        }
        way.Clear();
    }
}
