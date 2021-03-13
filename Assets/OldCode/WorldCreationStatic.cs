using OLDCODE;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace OLDCODE
{
    public class WorldCreationStatic : MonoBehaviour
    {
        public int Height = 100;
        public int Width = 164;
        public int octaves = 4;
        public float lac = 2f;
        public float pers = 0.5f;
        public float multi = 2.5f;
        public int age = 0;
        public float zoom = 1;
        public Dictionary<Vector2, WorldPointContinent> WorldPointContinents = new Dictionary<Vector2, WorldPointContinent>();
        List<Continent> Continents = new List<Continent>();
        public int ContinentCount = 7;
        public List<Color> colors = new List<Color>() { Color.green, Color.red, Color.blue, Color.black, Color.grey, Color.yellow, Color.cyan };
        public GameObject cubePrefab;
        List<Continent> DeletedContinents = new List<Continent>();
        List<GameObject> gos = new List<GameObject>();
        // Start is called before the first frame update
        void Start()
        {

        }
        void Update()
        {
            WorldPointContinents.Clear();
            foreach (GameObject go in gos)
                Destroy(go);
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    WorldPointContinents.Add(new Vector2(x, y), new WorldPointContinent(new Vector2(x, y)));
                }
            CreateContinents();
            Dictionary<Vector2, float> md = MantleData.GetData(0, octaves, lac, pers, multi, Height, Width, zoom); ;
            foreach (Continent c in Continents)
            {
                foreach (ContinentPoint cp in c.Points.Values)
                {
                    WorldPointContinent wp = c.ContinentPointToWorldPoint(cp, this);
                    float val = md[wp.Pos];
                    if (val < 0.12f)
                        cp.height = 0;
                    else if (val < 0.25f)
                        cp.height = 1;
                    else if (val < 0.37f)
                        cp.height = 2;
                    else if (val < 0.5f)
                        cp.height = 3;
                    else if (val < 0.62f)
                        cp.height = 4;
                    else if (val < 0.75f)
                        cp.height = 5;
                    else if (val < 0.87f)
                        cp.height = 6;
                    else
                        cp.height = 8;
                }
            }
            ColorMap();
        }
        public void ColorMap()
        {
            foreach (WorldPointContinent wp in WorldPointContinents.Values)
            {
                Color c = Color.blue;
                if (wp.contPoint != null)
                {
                    int height = wp.contPoint.height;
                    if (height < 2)
                        c = new Color(102f / 255f, 204 / 255f, 0);
                    else if (height < 4)
                        c = new Color(102f / 255f, 102 / 255f, 0);
                    else if (height < 6)
                        c = new Color(102f / 255f, 51 / 255f, 0);
                    else if (height < 8)
                        c = new Color(51 / 255f, 25f / 255f, 0);
                    else
                        c = Color.white;
                }
                wp.go.GetComponent<MeshRenderer>().material.color = c;
                if (wp.contPoint != null && wp.contPoint.cont.WorldPointCenter == wp)
                    wp.go.GetComponent<MeshRenderer>().material.color = Color.black;
            }
        }
        public void CreateContinents()
        {

            List<Vector2> ContinentPoints = new List<Vector2>() { new Vector2(30, 30), new Vector2(51, 65), new Vector2(106, 53), new Vector2(106, 23), new Vector2(140, 34), new Vector2(138, 74) };
            for (int i = 0; i < 6; i++)
            {
                Continent cont = new Continent(new WorldPointContinent(new Vector2(ContinentPoints[i].x, Height - ContinentPoints[i].y)));
                Continents.Add(cont);
                Dictionary<Vector2, bool> Map = new Dictionary<Vector2, bool>();
                while (Map.Count < 2300)
                    Map = new ZoomLandCreation(5, 5).Create(4);
                int width = (int)Map.Select(p => p.Key.x).Max() / (int)2;
                List<Vector2> LandPoints = new List<Vector2>();
                foreach (var pair in Map)
                {
                    if (pair.Value)
                        LandPoints.Add(pair.Key);
                }
                foreach (var p in LandPoints)
                {
                    ContinentPoint cp = new ContinentPoint(p - new Vector2(width, width), cont);
                    if (cont.Points.ContainsKey(p - new Vector2(width, width)))
                        continue;
                    cont.Points.Add(p - new Vector2(width, width), cp);
                    WorldPointContinent wp = cont.ContinentPointToWorldPoint(cp, this);
                    if (wp != null && wp.contPoint == null)
                    {
                        wp.contPoint = cp;
                    }
                }
            }
            foreach (WorldPointContinent wp in WorldPointContinents.Values)
            {
                GameObject go = Instantiate(cubePrefab);
                go.transform.position = wp.Pos;
                go.GetComponent<MeshRenderer>().material = Resources.Load<Material>("green");
                go.GetComponent<MeshRenderer>().material.color = wp.contPoint == null ? Color.blue : Color.green;
                wp.go = go;
                gos.Add(go);
            }
        }
        public ContinentPoint WorldPointContinentToContinentPoint(WorldPointContinent wp, Continent plate)
        {
            if (!plate.Points.ContainsKey(wp.Pos - plate.WorldPointCenter.Pos))
                return null;
            return plate.Points[wp.Pos - plate.WorldPointCenter.Pos];
        }
        public Vector2 WorldPointContinentToContinentPointVector2(WorldPointContinent wp, Continent plate)
        {
            return wp.Pos - plate.WorldPointCenter.Pos;
        }
    }
}

