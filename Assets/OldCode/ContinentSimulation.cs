using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace OLDCODE
{
    public class ContinentSimulation : MonoBehaviour
    {
        public int Height = 100;
        public int Width = 164;
        public int octaves = 4;
        public float lac = 2f;
        public float pers = 0.5f;
        public float multi = 2.5f;
        public int age = 0;
        public int lastContBreak = 0;
        public int ContBreakCD = 20;
        public Dictionary<Vector2, WorldPointContinent> WorldPointContinents = new Dictionary<Vector2, WorldPointContinent>();
        List<Continent> Continents = new List<Continent>();
        public int ContinentCount = 7;
        public List<Color> colors = new List<Color>() { Color.green, Color.red, Color.blue, Color.black, Color.grey, Color.yellow, Color.cyan };
        public GameObject cubePrefab;
        List<Continent> DeletedContinents = new List<Continent>();
        // Start is called before the first frame update
        void Start()
        {
            bool booleanCheck = new Vector2(0, 1) == new Vector2(0, 1);
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    WorldPointContinents.Add(new Vector2(x, y), new WorldPointContinent(new Vector2(x, y)));
                }
            CreateContinents();
        }
        void Update()
        {
            //if (Input.GetKeyDown(KeyCode.Space))
            {
                foreach (Continent plate in Continents)
                {
                    if (DeletedContinents.Contains(plate))
                        continue;
                    Move(plate);
                    RecalculateWorldPointContinents(plate);
                    plate.RecalculateEdges(this);
                }
                foreach (Continent cont in DeletedContinents)
                    Continents.Remove(cont);
                DeletedContinents.Clear();
                age++;
                ColorMap();
                if (Continents.Count < 7 && age > lastContBreak + ContBreakCD)
                    SplitContinent(Continents.OrderByDescending(p => p.Points.Count).First());

                List<Continent> badConts = new List<Continent>();
                foreach (Continent c in Continents)
                    if (c.Points.Count < 20)
                        badConts.Add(c);
                foreach (Continent c in badConts)
                    Continents.Remove(c);
                //TakeScreenshot();
            }
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
        public void SplitContinent(Continent c)
        {
            //first split
            Dictionary<Vector2, List<Vector2>> voronoiCenters = new Dictionary<Vector2, List<Vector2>>();
            int worldXMin = (int)c.Points.Min(p => p.Key.x) + (int)c.WorldPointCenter.Pos.x;
            int worldXMax = (int)c.Points.Max(p => p.Key.x) + (int)c.WorldPointCenter.Pos.x;
            int worldYMin = (int)c.Points.Min(p => p.Key.y) + (int)c.WorldPointCenter.Pos.y;
            int worldYMax = (int)c.Points.Max(p => p.Key.y) + (int)c.WorldPointCenter.Pos.y;

            //now split the continent
            Vector2 cont1 = new Vector2(Random.Range(worldXMin, worldXMax), Random.Range(worldYMin, worldYMax));
            Vector2 cont2 = new Vector2(Random.Range(worldXMin, worldXMax), Random.Range(worldYMin, worldYMax));
            if (!WorldPointContinents.ContainsKey(cont1) || !WorldPointContinents.ContainsKey(cont2))
                return;

            Continent newCont1 = new Continent(c.WorldPointCenter);
            Continent newCont2 = new Continent(c.WorldPointCenter);
            for (int i = 0; i < 20; i++)
            {
                Vector2 voronoi = new Vector2(Random.Range(worldXMin, worldXMax), Random.Range(worldYMin, worldYMax));
                voronoiCenters.Add(voronoi, new List<Vector2>());
            }
            foreach (ContinentPoint cp in c.Points.Values)
            {
                Vector2 cpWorldPos = cp.Pos + c.WorldPointCenter.Pos;
                float shortestDist = 9999;
                Vector2 voronoi = Vector2.zero;
                foreach (var pair in voronoiCenters)
                {
                    float dist = Vector2.Distance(cpWorldPos, pair.Key);
                    if (dist < shortestDist)
                    {
                        shortestDist = dist;
                        voronoi = pair.Key;
                    }
                }
                voronoiCenters[voronoi].Add(cp.Pos);
            }

            List<ContinentPoint> con1Points = new List<ContinentPoint>();
            List<ContinentPoint> con2Points = new List<ContinentPoint>();
            foreach (var pair in voronoiCenters)
            {
                float dist1 = Vector2.Distance(cont1, pair.Key);
                float dist2 = Vector2.Distance(cont2, pair.Key);
                if (dist1 < dist2)
                {
                    foreach (Vector2 v in pair.Value)
                    {
                        ContinentPoint newCP = new ContinentPoint(v - cont1, newCont1);
                        newCP.height = c.Points[v].height;
                        con1Points.Add(newCP);
                    }
                }
                else
                {
                    foreach (Vector2 v in pair.Value)
                    {
                        ContinentPoint newCP = new ContinentPoint(v - cont2, newCont2);
                        newCP.height = c.Points[v].height;
                        con2Points.Add(newCP);
                    }
                }
            }
            newCont1.driftDirection = (cont1 - cont2).normalized;
            newCont1.driftDirection = new Vector2((int)newCont1.driftDirection.x + 1, (int)newCont1.driftDirection.y + 1);
            newCont2.driftDirection = (cont2 - cont1).normalized;
            newCont2.driftDirection = new Vector2((int)newCont2.driftDirection.x + 1, (int)newCont2.driftDirection.y + 1);
            newCont1.driftDuration = 15;
            newCont2.driftDuration = 15;
            Continents.Add(newCont1);
            Continents.Add(newCont2);
            Continents.Remove(c);
            lastContBreak = age;
        }
        public void TakeScreenshot()
        {
            Camera camera = Camera.main;
            RenderTexture rt = new RenderTexture(1000, 700, 24);
            camera.targetTexture = rt;
            Texture2D screenShot = new Texture2D(1000, 700, TextureFormat.RGB24, false);
            camera.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, 1000, 700), 0, 0);
            camera.targetTexture = null;
            RenderTexture.active = null; // JC: added to avoid errors
            Destroy(rt);
            byte[] bytes = screenShot.EncodeToPNG();
            string filename = age.ToString() + ".png";
            System.IO.File.WriteAllBytes(filename, bytes);
        }
        public void RecalculateWorldPointContinents(Continent Cont)
        {
            //foreach (Continent plate in Plates)
            {
                Dictionary<ContinentPoint, ContinentPoint> Collisions = new Dictionary<ContinentPoint, ContinentPoint>();
                foreach (ContinentPoint point in Cont.Points.Values)
                {
                    WorldPointContinent wp = Cont.ContinentPointToWorldPoint(point, this);
                    if (wp != null)
                    {
                        if (wp.contPoint != null)
                        {
                            Collisions.Add(wp.contPoint, point);
                        }
                        else
                        {
                            wp.contPoint = point;
                        }
                    }
                }
                foreach (Continent collCont in Collisions.Select(p => p.Key.cont).Distinct())
                {
                    Dictionary<ContinentPoint, ContinentPoint> ContinentCollisionPoints = Collisions.Where(p => p.Key.cont == collCont).ToDictionary(p => p.Key, p => p.Value);


                    if (ContinentCollisionPoints.Count < 10)
                    //if ((float)ContinentCollisionPoints.Count / (float)collCont.Points.Count < 0.025f)
                    {
                        foreach (var pair in ContinentCollisionPoints)
                        {
                            //not big enough to be collision, smaller one subducts
                            WorldPointContinent wp = collCont.ContinentPointToWorldPoint(pair.Key, this);
                            ContinentPoint theirPoint = pair.Key;
                            ContinentPoint ourPoint = pair.Value;
                            if (Cont.GetNeighbors(ourPoint).Count < 3)
                            {
                                //island or something, we give it to them
                                theirPoint.height += 2;
                                Cont.Points.Remove(ourPoint.Pos);
                                /*Vector2 worldPosition = collCont.WorldPointCenter.Pos + theirPoint.Pos;
                                ContinentPoint theirNewPoint = new ContinentPoint(worldPosition - Cont.WorldPointCenter.Pos, Cont);
                                theirNewPoint.height = theirPoint.height;
                                if (!collCont.Points.ContainsKey(theirNewPoint.Pos))
                                {
                                    collCont.Points.Add(theirNewPoint.Pos, theirNewPoint);
                                    WorldPointContinent wp2 = collCont.ContinentPointToWorldPoint(theirNewPoint, this);
                                    if (wp2 != null)
                                        wp2.contPoint = theirNewPoint;
                                }*/
                                // if(Random.Range(0,2) == 0)
                                AdditionalSubduction(wp, Cont, collCont);
                            }
                            else if (collCont.GetNeighbors(theirPoint).Count < 3)
                            {
                                //we are colliding with an island
                                ourPoint.height += 2;
                                wp.contPoint = ourPoint;
                                collCont.Points.Remove(theirPoint.Pos);
                                /* Vector2 worldPosition = collCont.WorldPointCenter.Pos + theirPoint.Pos;
                                 ContinentPoint ourNewPoint = new ContinentPoint(worldPosition - Cont.WorldPointCenter.Pos, Cont);
                                 ourNewPoint.height = theirPoint.height;
                                 if (!Cont.Points.ContainsKey(ourNewPoint.Pos))
                                 {
                                     Cont.Points.Add(ourNewPoint.Pos, ourNewPoint);
                                     WorldPointContinent wp2 = Cont.ContinentPointToWorldPoint(ourNewPoint, this);
                                     if (wp2 != null)
                                         wp2.contPoint = ourNewPoint;
                                 }*/
                                AdditionalSubduction(wp, collCont, Cont);
                            }
                            else if (ourPoint.height > theirPoint.height) //cont already there subducts
                            {
                                ourPoint.height += 2;
                                wp.contPoint = ourPoint;
                                collCont.Points.Remove(theirPoint.Pos);
                                // if (Random.Range(0, 2) == 0)
                                AdditionalSubduction(wp, collCont, Cont);
                            }
                            else //our cont subducts
                            {
                                theirPoint.height += 2;
                                Cont.Points.Remove(ourPoint.Pos);
                                //if (Random.Range(0, 2) == 0)
                                AdditionalSubduction(wp, Cont, collCont);
                            }
                        }
                    }
                    else
                    {
                        //big enough, the continents become one
                        //first fix collision points
                        foreach (var pair in ContinentCollisionPoints)
                        {
                            WorldPointContinent wp = collCont.ContinentPointToWorldPoint(pair.Key, this);
                            ContinentPoint theirPoint = pair.Key;
                            ContinentPoint ourPoint = pair.Value;
                            wp.contPoint = ourPoint;
                            ourPoint.height++;
                            collCont.Points.Remove(theirPoint.Pos);
                            AdditionalSubduction(wp, collCont, Cont);
                        }
                        //then move their cont points to our continent
                        foreach (ContinentPoint theirPoint in collCont.Points.Values)
                        {
                            Vector2 worldPosition = collCont.WorldPointCenter.Pos + theirPoint.Pos;
                            ContinentPoint ourNewPoint = new ContinentPoint(worldPosition - Cont.WorldPointCenter.Pos, Cont);
                            ourNewPoint.height = theirPoint.height;
                            if (!Cont.Points.ContainsKey(ourNewPoint.Pos))
                            {
                                Cont.Points.Add(ourNewPoint.Pos, ourNewPoint);
                                WorldPointContinent wp = Cont.ContinentPointToWorldPoint(ourNewPoint, this);
                                if (wp != null)
                                    wp.contPoint = ourNewPoint;
                            }
                        }
                        DeletedContinents.Add(collCont);
                    }
                }
            }
        }
        public void AdditionalSubduction(WorldPointContinent origSubductionPoint, Continent subductingContinent, Continent growingContinent)
        {
            //if (Random.Range(0, 2) == 0)
            {
                if (WorldPointContinents.ContainsKey(origSubductionPoint.Pos + subductingContinent.driftDirection))
                {
                    WorldPointContinent AddedHeightPoint = WorldPointContinents[origSubductionPoint.Pos + subductingContinent.driftDirection];
                    ContinentPoint cp = WorldPointContinentToContinentPoint(AddedHeightPoint, growingContinent);
                    if (cp != null)
                        cp.height++;
                }
            }
        }
        public void Move(Continent continent)
        {
            Vector2 direction = Vector2.zero;
            int offWorldPoints = 0;
            int offWorldPointDirection = 0;
            foreach (ContinentPoint point in continent.Points.Values)
            {
                WorldPointContinent wpc = continent.ContinentPointToWorldPoint(point, this);
                if (wpc == null)
                {
                    offWorldPoints++;
                    offWorldPointDirection += (point.Pos + continent.WorldPointCenter.Pos).y > 0 ? -1 : 1;
                    continue;
                }
                wpc.contPoint = null;
            }

            if (offWorldPoints > 10)
            {
                continent.driftDuration = Random.Range(5, 15);
                continent.driftDirection = new Vector2(Random.Range(-1, 2), offWorldPointDirection > 0 ? 1 : -1);
            }
            if (continent.driftDuration == 0)
            {
                float val = 0;
                continent.driftDuration = Random.Range(5, 15);
                val = Random.Range(0f, 100f) / 100f;
                if (val < 0.12f)
                    direction = new Vector2(-1, 0);
                else if (val < 0.25f)
                    direction = new Vector2(-1, -1);
                else if (val < 0.37f)
                    direction = new Vector2(0, -1);
                else if (val < 0.5f)
                    direction = new Vector2(1, -1);
                else if (val < 0.62f)
                    direction = new Vector2(1, 0);
                else if (val < 0.75f)
                    direction = new Vector2(1, 1);
                else if (val < 0.87f)
                    direction = new Vector2(0, 1);
                else
                    direction = new Vector2(-1, 1);
                continent.driftDirection = direction;
            }
            else
            {
                continent.driftDuration--;
                direction = continent.driftDirection;
            }
            //float val = MantleData.GetData(age, octaves, lac, pers, multi)[continent.WorldPointCenter.Pos];


            Vector2 newPoint = continent.WorldPointCenter.Pos + direction;
            if (newPoint.x > 99)
                newPoint += new Vector2(-Width, 0);
            if (newPoint.x < 0)
                newPoint += new Vector2(Width, 0);
            if (newPoint.y < 0 || newPoint.y > 99)
                return;//continue;
            continent.WorldPointCenter = WorldPointContinents[newPoint];
        }
        public void CreateContinents()
        {
            int Ocean = WorldPointContinents.Count;
            int Land = 0;
            //for (int i = 0; i < 7; i++)
            while (true)
            {
                Continent cont = new Continent(new WorldPointContinent(new Vector2(Random.Range(0, Width), Random.Range(0, Height))));
                Continents.Add(cont);
                Dictionary<Vector2, bool> Map = new ZoomLandCreation(5, 5).Create(3);
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
                        Land++;
                        Ocean--;
                    }
                }
                if ((float)Land / (float)(Ocean + Land) > 0.3f)
                    break;
            }
            foreach (WorldPointContinent wp in WorldPointContinents.Values)
            {
                GameObject go = Instantiate(cubePrefab);
                go.transform.position = wp.Pos;
                go.GetComponent<MeshRenderer>().material = Resources.Load<Material>("green");
                go.GetComponent<MeshRenderer>().material.color = wp.contPoint == null ? Color.blue : Color.green;
                wp.go = go;
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
    public class WorldPointContinent
    {
        public WorldPointContinent(Vector2 pos_)
        {
            Pos = pos_;
        }
        public ContinentPoint contPoint;
        public Vector2 Pos;
        public GameObject go;
    }
}

