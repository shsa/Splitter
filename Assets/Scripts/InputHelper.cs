using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class TouchCreator
{
    static Dictionary<string, FieldInfo> fields;

    object touch;

    public float deltaTime { get { return ((Touch)touch).deltaTime; } set { fields["m_TimeDelta"].SetValue(touch, value); } }
    public int tapCount { get { return ((Touch)touch).tapCount; } set { fields["m_TapCount"].SetValue(touch, value); } }
    public TouchPhase phase { get { return ((Touch)touch).phase; } set { fields["m_Phase"].SetValue(touch, value); } }
    public Vector2 deltaPosition { get { return ((Touch)touch).deltaPosition; } set { fields["m_PositionDelta"].SetValue(touch, value); } }
    public int fingerId { get { return ((Touch)touch).fingerId; } set { fields["m_FingerId"].SetValue(touch, value); } }
    public Vector2 position { get { return ((Touch)touch).position; } set { fields["m_Position"].SetValue(touch, value); } }
    public Vector2 rawPosition { get { return ((Touch)touch).rawPosition; } set { fields["m_RawPosition"].SetValue(touch, value); } }

    public Touch Create()
    {
        return (Touch)touch;
    }

    public TouchCreator()
    {
        touch = new Touch();
    }

    static TouchCreator()
    {
        fields = new Dictionary<string, FieldInfo>();
        foreach (var f in typeof(Touch).GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
        {
            fields.Add(f.Name, f);
            //Debug.Log("name: " + f.Name);
        }
    }
}

public class InputHelper : MonoBehaviour
{

    private static TouchCreator lastFakeTouch;

    public static GameObject IntersectObject(Vector3 position)
    {
        Ray ray = Camera.main.ScreenPointToRay(position);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity);
        if (hit.collider != null)
            return hit.collider.gameObject;
        return null;
    }

    public static GameObject[] IntersectObjects(Vector3 position)
    {
        Ray ray = Camera.main.ScreenPointToRay(position);
        var hits = Physics2D.GetRayIntersectionAll(ray, Mathf.Infinity);
        return hits.Select(h => h.collider.gameObject).ToArray();
    }

    public static bool IntersectObject(Vector3 position, GameObject gameObject)
    {
        Ray ray = Camera.main.ScreenPointToRay(position);
        RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray, Mathf.Infinity);
        foreach (var hit in hits)
        {
            if (hit.collider.gameObject == gameObject)
            {
                return true;
            }
        }
        return false;
    }

    public static List<Touch> GetTouches()
    {
        List<Touch> touches = new List<Touch>();
        touches.AddRange(Input.touches);
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        if (lastFakeTouch == null) lastFakeTouch = new TouchCreator();
        if (Input.GetMouseButtonDown(0))
        {
            lastFakeTouch.phase = TouchPhase.Began;
            lastFakeTouch.deltaPosition = new Vector2(0, 0);
            lastFakeTouch.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            lastFakeTouch.fingerId = 0;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            lastFakeTouch.phase = TouchPhase.Ended;
            Vector2 newPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            lastFakeTouch.deltaPosition = newPosition - lastFakeTouch.position;
            lastFakeTouch.position = newPosition;
            lastFakeTouch.fingerId = 0;
        }
        else if (Input.GetMouseButton(0))
        {
            Vector2 newPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            lastFakeTouch.deltaPosition = newPosition - lastFakeTouch.position;
            lastFakeTouch.position = newPosition;
            lastFakeTouch.fingerId = 0;
            lastFakeTouch.phase = lastFakeTouch.deltaPosition.magnitude == 0 ? TouchPhase.Stationary : TouchPhase.Moved;
        }
        else
        {
            lastFakeTouch = null;
        }
        if (lastFakeTouch != null) touches.Add(lastFakeTouch.Create());
#endif


        return touches;
    }

}