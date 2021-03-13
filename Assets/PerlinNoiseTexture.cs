using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoiseTexture : MonoBehaviour
{
    public int octaves = 4;
    public float lac = 2f;
    public float pers = 0.5f;
    public float multi = 2.5f;
    public int age = 0;
    public float zoom = 1;
    List<GameObject> gameobjects = new List<GameObject>();
    public GameObject prefab;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        foreach (GameObject go in gameobjects)
            Destroy(go);
        Vector2 totalDirection = Vector2.zero;
        Dictionary<Vector2, float> noiseHeightVals = new Dictionary<Vector2, float>();
        for (float x = 0; x < 100; x++)
            for (float y = 0; y < 100; y++)
            {
                float amp = 1;
                float freq = 1;
                float noiseHeight = 0;
                for (int i = 0; i < octaves; i++)
                {
                    var per = Mathf.PerlinNoise((x + 60) / 50f / zoom* freq, (y + 60) / 50f / zoom* freq) * 2.5f - 1;
                    noiseHeight += per * amp;
                    amp *= pers;
                    freq *= lac;
                }
                if (noiseHeight > 1)
                    noiseHeight = 1;
                if (noiseHeight < 0)
                    noiseHeight = 0;
                float val = noiseHeight;
                noiseHeightVals.Add(new Vector2(x, y), noiseHeight);

            }
        foreach (var pair in noiseHeightVals)
        {
            GameObject go = Instantiate(prefab);
            go.transform.position = pair.Key;
            go.GetComponent<MeshRenderer>().material = Resources.Load<Material>("green");
            go.GetComponent<MeshRenderer>().material.color = new Color(pair.Value, pair.Value, pair.Value);
            gameobjects.Add(go);
        }
    }
}
