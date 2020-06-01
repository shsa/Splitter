using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    public GameMap map;
    public int index;

    private Vector2 _velocity;
    public Vector2 velocity {
        get {
            return _velocity;
        }
        set {
            _velocity = value;
            var rb = GetComponent<Rigidbody2D>();
            rb.velocity = _velocity;
        }
    }


    void Start()
    {
        index = 0;
    }

    void _Update()
    {
        //var newPos = Vector2.Lerp(transform.localPosition, (Vector2)transform.localPosition + velocity, Time.deltaTime);
        var v = Vector2.Lerp(Vector2.zero, velocity, Time.deltaTime);
        var hit = Physics2D.Raycast(transform.localPosition, velocity);
        if (hit.collider != null)
        {
            var dist = hit.collider.transform.localPosition - transform.localPosition;
            if (dist.sqrMagnitude > v.sqrMagnitude)
            {
                transform.localPosition = (Vector2)transform.localPosition + v;
            }
        }
    }

    private Collider2D lastCollider;
    private void _OnTriggerEnter2D(Collider2D collider)
    {
        Debug.Log($"index: {index}");
        index++;
        if (lastCollider == collider)
        {
            return;
        }
        lastCollider = collider;

        var pos = collider.transform.localPosition;
        var dir = transform.localPosition - pos;
        var pos0 = new Vector2(pos.x, pos.y + dir.y).ToInt();
        var pos1 = new Vector2(pos.x + dir.x, pos.y).ToInt();
        if (map.Contains(pos0) && map.Contains(pos1))
        {
            Debug.LogWarning($"{pos0}, {pos1}");
        }
        else
        if (map.Contains(pos0))
        {
            dir = new Vector3(dir.x, 0);
        }
        else
        {
            dir = new Vector2(0, dir.y);
        }
        velocity = Vector3.Reflect(velocity, dir.normalized);
    }
}
