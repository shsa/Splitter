using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSetup : MonoBehaviour
{
    public int Width = 100;
    public int Height = 100;
    public int Speed = 10;
    public int EnemyCount = 2;

    public GameObject PlayerPrefab;
    public GameObject EnemyPrefab;
    public GameObject BorderPrefab;
    public GameObject FillerPrefab;
    public GameObject CoverPrefab;
    public GameObject LinePrefab;
}
