using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace OLDCODE
{
    public class MantleData
    {
        //public Dictionary<Vector2, bool> MantleHeat = new Dictionary<Vector2, bool>();
        // Start is called before the first frame update
        public static Dictionary<Vector2, float> GetData(int offset, int octaves, float lac, float pers, float multi, float height, float width, float zoom)
        {
            Dictionary<float, int> directionCounts = new Dictionary<float, int>();
            Dictionary<Vector2, float> MantleHeat = new Dictionary<Vector2, float>();
            directionCounts.Add(0.12f, 0);
            directionCounts.Add(0.25f, 0);
            directionCounts.Add(0.37f, 0);
            directionCounts.Add(0.50f, 0);
            directionCounts.Add(0.62f, 0);
            directionCounts.Add(0.75f, 0);
            directionCounts.Add(0.87f, 0);
            directionCounts.Add(1.00f, 0);
            float max = 0;
            for (float x = 0; x < width; x++)
                for (float y = 0; y < height; y++)
                {
                    float amp = 1;
                    float freq = 1;
                    float noiseHeight = 0;
                    for (int i = 0; i < octaves; i++)
                    {
                        var per = Mathf.PerlinNoise((x + offset) / 50f / zoom * freq, (y + offset) / 50f / zoom * freq) * multi - 1;
                        noiseHeight += per * amp;
                        amp *= pers;
                        freq *= lac;
                        if (noiseHeight > max)
                            max = noiseHeight;
                    }
                }
            for (float x = 0; x < width; x++)
                for (float y = 0; y < height; y++)
                {
                    float amp = 1;
                    float freq = 1;
                    float noiseHeight = 0;
                    for (int i = 0; i < octaves; i++)
                    {
                        var per = Mathf.PerlinNoise((x + offset) / 50f / zoom * freq, (y + offset) / 50f / zoom * freq) * multi - 1;
                        noiseHeight += per * amp;
                        amp *= pers;
                        freq *= lac;
                    }

                    float val = noiseHeight;
                    Vector2 direction = Vector2.zero;
                    if (val < 0.12f)
                        directionCounts[0.12f]++;
                    else if (val < 0.25f)
                        directionCounts[0.25f]++;
                    else if (val < 0.37f)
                        directionCounts[0.37f]++;
                    else if (val < 0.5f)
                        directionCounts[0.5f]++;
                    else if (val < 0.62f)
                        directionCounts[0.62f]++;
                    else if (val < 0.75f)
                        directionCounts[0.75f]++;
                    else if (val < 0.87f)
                        directionCounts[0.87f]++;
                    else
                        directionCounts[1f]++;

                    MantleHeat.Add(new Vector2(x, y), noiseHeight / max);
                }
            string output = string.Join
    (
        ",",
        directionCounts.Select(pair => string.Format("{0}={1}", pair.Key.ToString(), pair.Value.ToString())).ToArray()
    );
            Debug.Log(output);
            return MantleHeat;
        }
        // Update is called once per frame
        void Update()
        {

        }
    }
}
