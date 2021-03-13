using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
namespace OLDCODE
{
    public class TectonicSimulation : MonoBehaviour
    {
        int age = 0;
        public MantleData md;
        public Dictionary<Vector2, WorldPoint> WorldPoints = new Dictionary<Vector2, WorldPoint>();
        List<TectonicPlate> Plates = new List<TectonicPlate>();
        public int PlatCount = 7;
        public List<Color> colors = new List<Color>() { Color.green, Color.red, Color.blue, Color.black, Color.grey, Color.yellow, Color.cyan };
        public GameObject cubePrefab;
        // Start is called before the first frame update
        void Start()
        {
            md = new MantleData();
            bool booleanCheck = new Vector2(0, 1) == new Vector2(0, 1);
            for (int x = 0; x < 100; x++)
                for (int y = 0; y < 100; y++)
                {
                    WorldPoints.Add(new Vector2(x, y), new WorldPoint(new Vector2(x, y)));
                }
            CreateVoronoi();
        }
        void Update()
        {
            //if (Input.GetKeyDown(KeyCode.Space))
            {
                foreach (TectonicPlate plate in Plates)
                {
                    Move(plate);
                    RecalculateWorldPoints(plate);
                    plate.RecalculateEdges();
                }
                //BlackenVoids();
                AddToVoids();
                age++;
                //TakeScreenshot();
            }
        }
        public void TakeScreenshot()
        {
            Camera camera = Camera.main;
            RenderTexture rt = new RenderTexture(700, 700, 24);
            camera.targetTexture = rt;
            Texture2D screenShot = new Texture2D(700, 700, TextureFormat.RGB24, false);
            camera.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, 700, 700), 0, 0);
            camera.targetTexture = null;
            RenderTexture.active = null; // JC: added to avoid errors
            Destroy(rt);
            byte[] bytes = screenShot.EncodeToPNG();
            string filename = age.ToString() + ".png";
            System.IO.File.WriteAllBytes(filename, bytes);
        }
        public void BlackenVoids()
        {
            foreach (WorldPoint wp in WorldPoints.Values.Where(p => p.TecPoint == null))
            {
                wp.go.GetComponent<MeshRenderer>().material.color = Color.black;
            }
        }
        public void AddToVoids()
        {
            foreach (WorldPoint wp in WorldPoints.Values.Where(p => p.TecPoint == null))
            {
                if (wp.OldTectonicPoint == null)
                    continue;
                TectonicPlate tp = wp.OldTectonicPoint.plate;
                TectonicPoint createdTecPoint = new TectonicPoint(wp.Pos - tp.WorldPointCenter.Pos, -age, tp);
                wp.TecPoint = createdTecPoint;
                tp.Points.Add(createdTecPoint.Pos, createdTecPoint);
                wp.go.GetComponent<MeshRenderer>().material.color = tp.c;
            }
        }
        public void RecalculateWorldPoints(TectonicPlate plate)
        {
            //foreach (TectonicPlate plate in Plates)
            {
                List<TectonicPoint> badPoints = new List<TectonicPoint>();
                foreach (TectonicPoint point in plate.Points.Values)
                {
                    WorldPoint wp = plate.TectonicPointToWorldPoint(point);
                    if (wp != null)
                    {
                        bool continueAfter = true;
                        if (wp.TecPoint != null)
                        {
                            //one plate subducts
                            TectonicPoint otherPlatePoint = wp.TecPoint;
                            if (otherPlatePoint.age > point.age)
                            {
                                //the other plate is older and more dense, and that one subducts
                                otherPlatePoint.plate.Points.Remove(otherPlatePoint.Pos);
                                List<TectonicPoint> neighbors = otherPlatePoint.plate.GetNeighbors(otherPlatePoint);
                                foreach (TectonicPoint n in neighbors)
                                {
                                    otherPlatePoint.plate.SubductionForce += n.Pos - otherPlatePoint.Pos;
                                }
                            }
                            else
                            {
                                //this plate is older and more dense, this one subducts
                                badPoints.Add(point);
                                continueAfter = false;
                                List<TectonicPoint> neighbors = plate.GetNeighbors(point);
                                foreach (TectonicPoint n in neighbors)
                                {
                                    plate.SubductionForce += n.Pos - point.Pos;
                                }
                            }
                        }
                        if (continueAfter)
                        {
                            wp.TecPoint = point;
                        }
                    }
                }
                foreach (TectonicPoint badPoint in badPoints)
                    plate.Points.Remove(badPoint.Pos);
            }
            foreach (TectonicPoint point in plate.Points.Values)
            {
                WorldPoint wp = plate.TectonicPointToWorldPoint(point);
                if (wp != null)
                    wp.go.GetComponent<MeshRenderer>().material.color = point.plate.c;
            }
            plate.WorldPointCenter.go.GetComponent<MeshRenderer>().material.color = Color.black;
        }

        public void Move(TectonicPlate plate)
        {
            foreach (TectonicPoint tp in plate.Points.Values)
            {
                WorldPoint wp = plate.TectonicPointToWorldPoint(tp);
                if (wp == null)
                    continue;
                wp.OldTectonicPoint = tp;
                wp.TecPoint = null;
            }
            Vector2 plateDirection = plate.SubductionForce.normalized;
            foreach (TectonicPoint point in plate.Points.Values.Where(p => p.Edge))
            {
                WorldPoint wp = plate.TectonicPointToWorldPoint(point);
                if (wp == null)
                    continue;
                // if (md.MantleHeat[wp.Pos]) //if the mantle is hot, move plates away
                {
                    List<TectonicPoint> neighbors = plate.GetNeighbors(point);
                    foreach (TectonicPoint n in neighbors)
                    {
                        plateDirection += (n.Pos - point.Pos) / 10f;
                    }
                }
            }
            float maxVal = Mathf.Max(Mathf.Abs(plateDirection.normalized.x), Mathf.Abs(plateDirection.normalized.y));
            if (maxVal == 0)
                return;
            Vector2 direction = plateDirection.normalized * 1 / maxVal;//new Vector2(Random.Range(-1, 2), Random.Range(-1, 2));
            direction = new Vector2((int)direction.x, (int)direction.y);
            Vector2 newPoint = plate.WorldPointCenter.Pos + direction;
            if (newPoint.x > 99)
                newPoint += new Vector2(-100, 0);
            if (newPoint.x < 0)
                newPoint += new Vector2(100, 0);
            if (newPoint.y < 0 || newPoint.y > 99)
                return;//continue;
            plate.WorldPointCenter = WorldPoints[newPoint];
            plate.SubductionForce = Vector2.zero;
        }
        public void CreateVoronoi()
        {
            int voronoiCount = 7;
            List<int> ages = new List<int>();
            for (int i = 0; i < voronoiCount; i++)
            {
                Vector2 point = new Vector2(Random.Range(0, 100), Random.Range(0, 100));
                WorldPoint center = WorldPoints[point];
                TectonicPlate tp = new TectonicPlate(this, center);
                Plates.Add(tp);
                Color c = new Color(Random.Range(0, 255f) / 255f, Random.Range(0, 255f) / 255f, Random.Range(0, 255f) / 255f);
                tp.c = c;
                ages.Add(Random.Range(1, 100));
            }
            foreach (WorldPoint wp in WorldPoints.Values)
            {
                float shortestDist = 999999;
                TectonicPlate tp = null;
                Color finalColor = Color.black;
                for (int i = 0; i < voronoiCount; i++)
                {
                    Vector2 center = Plates[i].WorldPointCenter.Pos;
                    float dist = Vector2.Distance(center, wp.Pos);
                    if (dist < shortestDist)
                    {
                        tp = Plates[i];
                        finalColor = colors[i];
                        shortestDist = dist;
                    }
                }
                if (wp.Pos - tp.WorldPointCenter.Pos != Vector2.zero)
                    tp.Points.Add(wp.Pos - tp.WorldPointCenter.Pos, new TectonicPoint(wp.Pos - tp.WorldPointCenter.Pos, ages[Plates.IndexOf(tp)], tp));
            }
            foreach (TectonicPlate plate in Plates)
            {
                foreach (TectonicPoint point in plate.Points.Values)
                {
                    WorldPoint wp = plate.TectonicPointToWorldPoint(point);
                    wp.TecPoint = point;
                    GameObject go = Instantiate(cubePrefab);
                    go.transform.position = wp.Pos;
                    go.GetComponent<MeshRenderer>().material = Resources.Load<Material>("green");
                    go.GetComponent<MeshRenderer>().material.color = plate.c;
                    wp.go = go;
                }
                plate.RecalculateEdges();
            }
        }
        public List<WorldPoint> GetNeighbors(WorldPoint point, bool LookForUnnocupied)
        {
            List<WorldPoint> Neighbors = new List<WorldPoint>();
            for (int x = -1; x <= 1; x++)
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                        continue;
                    if (WorldPoints.ContainsKey(new Vector2(x + point.Pos.x, y + point.Pos.y)))
                        continue;
                    WorldPoint newpoint = WorldPoints[new Vector2(x + point.Pos.x, y + point.Pos.y)];
                    if (LookForUnnocupied && newpoint.TecPoint == null)
                        Neighbors.Add(newpoint);
                    else if (LookForUnnocupied)
                        continue;
                    else
                        Neighbors.Add(newpoint);

                }
            return Neighbors;
        }
        public TectonicPoint WorldPointToTectonicPoint(WorldPoint wp, TectonicPlate plate)
        {
            if (!plate.Points.ContainsKey(wp.Pos - plate.WorldPointCenter.Pos))
                return null;
            return plate.Points[wp.Pos - plate.WorldPointCenter.Pos];
        }
        public T DeepClone<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }
    }
    public class WorldPoint
    {
        public WorldPoint(Vector2 pos_)
        {
            Pos = pos_;
        }
        public Vector2 Pos;
        public TectonicPoint? OldTectonicPoint;
        public TectonicPoint? TecPoint;
        public GameObject go;
    }
}

