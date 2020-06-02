using DG.Tweening;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GameStatus
{
    Play,
    Paused,
    Failed,
    Won
}

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
    GameStatus status = GameStatus.Paused;
    RectInt gameRect;
    GameMap map = new GameMap();
    RectTransform gameUI;
    Transform game;
    GameInput input;
    Transform player;
    Transform finish;
    List<Transform> enemyList = new List<Transform>();
    GameObject image;
    float enemySpeed = 1;
    Vector2Int startPos;
    Vector2Int finishPos;
    Vector2Int dir;
    Vector2Int nextDir;
    Sequence doTweens;


    // Start is called before the first frame update
    void Start()
    {
        doTweens = DOTween.Sequence();
        gameUI = GameObject.Find("GameUI").GetComponent<RectTransform>();
        game = GameObject.Find("Game").transform;
        input = game.gameObject.AddComponent<GameInput>();

        finish = GameObject.Find("Finish").transform;
        enemyList = GameObject.FindGameObjectsWithTag("Enemy").Select(e => e.transform).ToList();
        image = GameObject.Find("Image");

        map.Setup = Setup;
        map.Parent = GameObject.Find("Borders").transform;
        foreach (var enemy in enemyList)
        {
            enemy.GetComponent<EnemyScript>().map = map;
        }
        GameInit();
    }

    void Update()
    {
        ViewSetup();
        UpdateInput();

        if (status == GameStatus.Play)
        {
            UpdateGame();
        }
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
                    if (map.GetItemType(finishPos) == ItemType.Line)
                    {
                        if (map.GetItemType(finishPos + dir) == ItemType.Border)
                        {
                            OnFinish();
                        }
                        else
                        {
                            OnFailed();
                        }
                    }
                    else
                    {
                        OnEndMove();
                    }
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
                        player.GetComponent<BoxCollider2D>().enabled = wayType == ItemType.Cover;
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
        if (status == GameStatus.Failed)
        {
            if (input.anyKeyDown)
            {
                GameInit();
            }
        }

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
                    else
                    {
                        StopPlayer();
                    }
                }
                else
                {
                    StopPlayer();
                    nextDir = input.direction;
                }
            }
        }
    }

    void StopPlayer()
    {
        finishPos = player.localPosition.ToInt();
        if (Vector2.Dot(finishPos - (Vector2)player.localPosition, dir) < 0)
        {
            finishPos += dir;
        }
        nextDir = Vector2Int.zero;
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


        enemySpeed = Setup.Speed * cellSize;
        foreach (var enemy in enemyList)
        {
            var rb = enemy.GetComponent<Rigidbody2D>();
            if (rb.velocity.sqrMagnitude == 0)
            {
                rb.RandomVelocity(enemySpeed);
            }
            else
            {
                rb.velocity = rb.velocity.normalized * enemySpeed;
            }
        }
    }

    void GameInit()
    {
        DOTween.Clear();

        gameRect = new RectInt(0, 0, Setup.Width, Setup.Height);

        map.Clear();
        way.Clear();
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

        if (player != null)
        {
            Object.Destroy(player.gameObject);
        }
        player = this.CreatePlayer();
        player.localPosition = new Vector2((Setup.Width - 1) / 2, 0);
        player.GetComponent<BoxCollider2D>().enabled = false;

        foreach (var enemy in enemyList)
        {
            Object.Destroy(enemy.gameObject);
        }
        enemyList.Clear();

        for (int i = 0; i < Setup.EnemyCount; i++)
        {
            enemyList.Add(this.CreateEnemy());
        }

        foreach (var enemy in enemyList)
        {
            enemy.localPosition = new Vector2(Random.Range(gameRect.xMin + 1, gameRect.xMax - 1), Random.Range(gameRect.yMin + 1, gameRect.yMax - 1));
            var rb = enemy.GetComponent<Rigidbody2D>();
            rb.RandomVelocity(enemySpeed);
            var enemyScript = enemy.GetComponent<EnemyScript>();
            enemyScript.game = this;
        }

        input.Setup = Setup;
        dir = Vector2Int.zero;

        status = GameStatus.Play;
    }

    int Count(Vector2Int pos)
    {
        var ee = enemyList.Select(e => e.localPosition.ToInt()).ToArray();

        var sum = 0;
        foreach (var p in map.GetFillArea(pos))
        {
            // we cant fill area, which contains a enemy
            foreach (var e in ee)
            {
                if (e == p)
                    return 0;
            }
            sum++;
        }
        return sum;
    }

    void Fill(List<Vector2Int> list, Vector2Int dir)
    {
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
                var obj = map.Get(p);
                var sr = obj.GetComponent<SpriteRenderer>();
                sr.DOColor(new Color(1, 1, 1, 0), time).SetDelay(pause);
            }
            //yield return pause;
            pause += pauseTime;

            i0 += step;
        }
    }

    void Fill(Vector2Int pos, Vector2Int dir)
    {
        var list = new List<Vector2Int>();
        foreach (var p in map.GetFillArea(pos))
        {
            map.SetFiller(p);
            list.Add(p);
        }

        Fill(list, dir);
    }

    void OnEndMove()
    {
        dir = Vector2Int.zero;
        input.direction = Vector2Int.zero;
        player.GetComponent<BoxCollider2D>().enabled = false;
    }

    void OnFinish()
    {
        OnEndMove();

        var list = map.GetObjects(ItemType.Line).ToList();
        foreach (var obj in list)
        {
            map.SetBorder(obj.position);
        }

        list = map.GetObjects(ItemType.Cover).ToList();
        foreach (var obj in list)
        {
            obj.id = 0;
        }

        var id = 1;
        var ids = new HashSet<int>();
        foreach (var obj in list)
        {
            if (obj.id == 0)
            {
                ids.Add(id);
                foreach (var pos in map.GetFillArea(obj.position))
                {
                    var a = map.Get(pos);
                    a.id = id;
                }
                id++;
            }
        }

        foreach (var enemy in enemyList)
        {
            var pos = enemy.localPosition.ToInt();
            var a = map.Get(pos);
            ids.Remove(a.id);
        }
        var fillList = new List<Vector2Int>();
        foreach (var id1 in ids)
        {
            foreach (var obj in list)
            {
                if (obj.id == id1)
                {
                    fillList.Add(obj.position);
                    map.SetFiller(obj.position);
                }
            }
        }
        Fill(fillList, Vector2Int.up);
    }

    public void OnFailed()
    {
        status = GameStatus.Failed;
        input.anyKeyDown = false;
        foreach (var enemy in enemyList)
        {
            var rb = enemy.GetComponent<Rigidbody2D>();
            rb.velocity = Vector2.zero;
        }
        dir = Vector2Int.zero;
        
        var list = map.GetObjects(ItemType.Line).Select(info => info.gameObject).ToList();
        list.Add(player.gameObject);
        foreach (var line in list)
        { 
            var child = line.transform.Find("Failed");
            var sr = child.GetComponent<SpriteRenderer>();
            var tween = sr.DOColor(Color.white, 1.0f)
                .SetLoops(-1, LoopType.Yoyo);
            doTweens.Append(tween);
        }
    }
}
