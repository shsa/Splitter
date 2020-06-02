using System.Linq;
using UnityEngine;


public class GameInput : MonoBehaviour
{
    /// <summary>
    /// start touch position
    /// </summary>
    public Vector2Int position;
    /// <summary>
    /// only up, down, left, right vectors
    /// </summary>
    public Vector2Int direction;

    /// <summary>
    /// true if user make any action: touch, press keyboard
    /// </summary>
    public bool anyKeyDown = false;

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

    void ChangeDirection(Vector2Int dir)
    {
        position = Vector2Int.zero;
        direction = dir;
    }

    void Update()
    {
        UpdateTouchAndMouse();
        UpdateKeyboard();
    }

    Vector2Int touchEnd;
    void UpdateTouchAndMouse()
    {
        foreach (Touch touch in InputHelper.GetTouches())
        {
            var pos0 = Camera.main.ScreenToWorldPoint(touch.position);
            pos0 = transform.InverseTransformPoint(pos0);
            var pos = new Vector2(pos0.x, pos0.y).ToInt();
            if (!rect.Contains(pos))
            {
                return;
            }

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    anyKeyDown = true;
                    touchEnd = pos;
                    break;
                case TouchPhase.Moved:
                    var d = pos - touchEnd;
                    if (d.sqrMagnitude <= 4)
                    {
                        return;
                    }
                    touchEnd = pos;

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

                    ChangeDirection(d);
                    position = pos;

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

    void UpdateKeyboard()
    {
        if (Input.anyKeyDown)
        {
            anyKeyDown = true;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            ChangeDirection(Vector2Int.up);
        }
        else
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ChangeDirection(Vector2Int.down);
        }
        else
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ChangeDirection(Vector2Int.left);
        }
        else
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ChangeDirection(Vector2Int.right);
        }
    }
}
