using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameScript : MonoBehaviour
{
    public GameSetup Setup;

    public struct WayPoint
    {
        public Vector2Int point;
        public Vector2Int dir;

        public WayPoint(Vector2Int point, Vector2Int dir)
        {
            this.point = point;
            this.dir = dir;
        }
    }

    RectInt gameSize;
    RectInt gameInside;
    GameMap map = new GameMap();
    List<WayPoint> way = new List<WayPoint>();
    RectTransform gameUI;
    Transform game;
    Transform start;
    Transform finish;
    Transform enemy;
    GameObject image;

    // Start is called before the first frame update
    void Start()
    {
        gameUI = GameObject.Find("GameUI").GetComponent<RectTransform>();
        game = GameObject.Find("Game").transform;
        start = GameObject.Find("Start").transform;
        finish = GameObject.Find("Finish").transform;
        enemy = GameObject.Find("Enemy").transform;
        image = GameObject.Find("Image");

        map.Setup = Setup;
        map.Parent = GameObject.Find("Borders").transform;
        enemy.GetComponent<EnemyScript>().map = map;
        GameInit();
    }

    // Update is called once per frame
    void Update()
    {
        ViewSetup();
        UpdateInput();
        UpdatePlayer();

        if (isStarted)
        {
            curTime += Time.deltaTime;
            if (curTime > maxTime)
            {
                curTime = maxTime;
            }
            var scale = maxTime > 0 ? curTime / maxTime : 0;
            if (startPos == finishPos)
            {
                curPos = finishPos;
            }
            else
            {
                curPos = Vector2.Lerp(startPos, finishPos, scale);
            }
            var p = lastPos;
            var d = curPos - p;
            while (Vector2.Dot(dir, d) >= 0)
            {
                map.SetBorder(p);
                way.Add(new WayPoint(p, dir));

                lastPos = p;
                p += dir;
                d = curPos - p;
            }

            start.localPosition = curPos;

            if (curTime == maxTime)
            {
                if (nextDir == Vector2Int.zero)
                {
                    isStarted = false;
                    OnFinish();
                }
                else
                {
                    dir = nextDir;
                    nextDir = Vector2Int.zero;

                    startPos = curPos.ToInt();
                    lastPos = startPos;
                    start.localPosition = new Vector2(startPos.x, startPos.y);
                    finishPos = GetFinishPos(startPos, dir);
                    finish.localPosition = new Vector2(finishPos.x, finishPos.y);
                    curTime = 0;
                    maxTime = (finishPos - startPos).magnitude / Setup.Speed;
                }
            }
        }
    }

    float gameWidth = 0;
    float gameHeight = 0;
    float cellSize = 1;
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

        cellSize = rect.width / Setup.Width;
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
        sc.velocity = sc.velocity.normalized * Setup.Speed * cellSize;
    }

    Vector2Int posBegin;
    Vector2Int posEnd;
    Vector2Int startPos;
    Vector2Int finishPos;
    Vector2Int lastPos;
    Vector2Int dir;
    Vector2Int nextDir;
    Vector2 curPos;
    float maxTime;
    float curTime;
    bool isStarted = false;
    void UpdateInput()
    {
        foreach (Touch touch in InputHelper.GetTouches())
        {
            var pos0 = Camera.main.ScreenToWorldPoint(touch.position);
            pos0 = game.InverseTransformPoint(pos0);
            var pos = Vector2Int.FloorToInt(new Vector2(pos0.x, pos0.y));
            if (!gameInside.Contains(pos))
            {
                return;
            }

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    posBegin = pos;
                    posEnd = pos;
                    break;
                case TouchPhase.Moved:
                    var d = pos - posEnd;
                    if (d.sqrMagnitude <= 4)
                    {
                        return;
                    }
                    posBegin = posEnd;
                    posEnd = pos;

                    if (Mathf.Abs(d.x) > Mathf.Abs(d.y))
                    {
                        if (d.x < 0)
                        {
                            d = Vector2Int.left;
                        }
                        else
                        {
                            d = Vector2Int.right;
                        }
                    }
                    else
                    {
                        if (d.y < 0)
                        {
                            d = Vector2Int.down;
                        }
                        else
                        {
                            d = Vector2Int.up;
                        }
                    }

                    if (isStarted)
                    {
                        // Только если направление меняется под прямым углом
                        if (Vector2.Dot(dir, d) == 0)
                        {
                            nextDir = d;
                            finishPos = curPos.ToInt();
                            maxTime = (finishPos - startPos).magnitude / Setup.Speed;
                        }
                    }
                    else
                    {
                        way.Clear();
                        dir = d;
                        nextDir = Vector2Int.zero;

                        startPos = GetStartPos(posEnd, dir);
                        lastPos = startPos;
                        start.localPosition = new Vector2(startPos.x, startPos.y);
                        finishPos = GetFinishPos(startPos, dir);
                        finish.localPosition = new Vector2(finishPos.x, finishPos.y);
                        curTime = 0;
                        maxTime = (finishPos - startPos).magnitude / Setup.Speed;

                        isStarted = true;
                    }

                    break;
                case TouchPhase.Stationary:
                    break;
                case TouchPhase.Ended:
                    break;
                case TouchPhase.Canceled:
                    break;
                default:
                    break;
            }
        }
    }

    void ChangeDir(Vector2Int dir)
    {

    }

    Vector2Int GetStartPos(Vector2Int start, Vector2Int dir)
    {
        var pos = start;
        // поиск свободного места, в случае, если движение было по бордюру
        while (!map.IsEmpty(pos))
        {
            pos -= dir;
        }

        while (gameInside.Contains(pos) && map.IsEmpty(pos))
        {
            pos -= dir;
        }

        return pos;
    }

    Vector2Int GetFinishPos(Vector2Int start, Vector2Int dir)
    {
        var pos = start + dir;
        while (gameInside.Contains(pos) && map.IsEmpty(pos))
        {
            pos += dir;
        }

        return pos - dir;
    }

    void UpdatePlayer()
    {
    }

    void GameInit()
    {
        gameSize = new RectInt(0, 0, Setup.Width, Setup.Height);
        gameInside = new RectInt(1, 1, Setup.Width - 2, Setup.Height - 2);

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
        sc.velocity = dir * Setup.Speed;
    }

    int Count(Vector2Int pos)
    {
        var sum = 0;
        foreach (var p in map.GetInternal(pos))
        {
            sum++;
        }
        return sum;
    }

    IEnumerator Fill(Vector2Int pos, Vector2Int dir)
    {
        var list = new List<Vector2Int>();
        foreach (var p in map.GetInternal(pos))
        {
            map.SetFiller(p);
            list.Add(p);
        }

        var borders = GameObject.FindObjectsOfType<BorderComponent>();
        foreach (var border in borders)
        {
            var positionComponent = border.GetComponent<GameMapComponent>();
            var p = positionComponent.position;
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
                iTween.ColorTo(obj, iTween.Hash("color", Color.clear, "time", time, "delay", pause));
            }
            //yield return pause;
            pause += pauseTime;

            i0 += step;
        }


        yield break;
    }

    void OnFinish()
    {
        foreach (var w in way)
        {
            var d = w.dir.Rotate90();
            var p0 = w.point + d;
            var p1 = w.point - d;

            if (map.IsEmpty(p0) && map.IsEmpty(p1))
            {
                var s0 = Count(p0);
                var s1 = Count(p1);

                var pos = p1;
                if (s0 < s1)
                {
                    pos = p0;
                }

                StartCoroutine(Fill(pos, w.dir));
                break;
            }
        }
    }
}
