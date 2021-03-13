using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ZoomLandCreationGameObject : MonoBehaviour
{
    Dictionary<Vector2, bool> Map = new Dictionary<Vector2, bool>();
    List<GameObject> mapDrawn = new List<GameObject>();
    public GameObject prefab;
    public int growths = 3;
    public int zooms = 3;
    public int width = 3;
    public void Start()
    {
      
    }
    public void Update()
    {
        Map = new ZoomLandCreation(16, 10).CreateEarth(zooms);
        //Map = new ZoomLandCreation(width, width).Create(zooms);
        DrawMap();
    }
    public void DrawMap()
    {
        foreach (GameObject go in mapDrawn)
            Destroy(go);
        mapDrawn.Clear();
        foreach (var pair in Map)
        {
            GameObject go = Instantiate(prefab);
            go.transform.position = pair.Key;
            go.GetComponent<MeshRenderer>().material.color = pair.Value ? Color.green : Color.blue;
            mapDrawn.Add(go);
        }
    }
}
