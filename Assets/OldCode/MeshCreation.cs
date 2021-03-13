using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace OLDCODE
{
    public class MeshCreation : MonoBehaviour
    {
        public bool oceanic;
        public MeshCreation mcTest;
        public Mesh mesh;
        public Vector3[] vertices;
        public Rigidbody rb;
        public float xSize = 11;
        public float ySize = 6;
        public Vector3 CenterOfGravity;
        public List<Square> Squares = new List<Square>();
        public List<Triangle> Triangles { get { return Squares.SelectMany(p => p.Triangles).ToList(); } }
        float lastUpdate = 0;
        // Start is called before the first frame update
        void Start()
        {
            CenterOfGravity = new Vector3((xSize + 1) / 2, (ySize + 1) / 2, 0);
            rb = GetComponent<Rigidbody>();
            rb.centerOfMass = CenterOfGravity;
            rb.AddForce(new Vector3(1, 0, 0));
            GetComponent<MeshFilter>().mesh = mesh = new Mesh();

            mesh.name = "Procedural Grid";
            for (int i = 0, y = 0; y <= ySize; y++)
            {
                for (int x = 0; x <= xSize; x++, i++)
                {
                    Triangle t1 = new Triangle();
                    t1.points = new List<Vector3>();
                    t1.points.Add(new Vector2(x, y));
                    t1.points.Add(new Vector2(x, y + 1f));
                    t1.points.Add(new Vector2(x + 1f, y + 1f));
                    Triangle t2 = new Triangle();
                    t2.points = new List<Vector3>();
                    t2.points.Add(new Vector2(x, y));
                    t2.points.Add(new Vector2(x + 1f, y + 1f));
                    t2.points.Add(new Vector2(x + 1f, y));
                    //vertices[i] = new Vector3(x, y);

                    List<Triangle> TempTriangles = new List<Triangle>();
                    TempTriangles.Add(t1);
                    TempTriangles.Add(t2);
                    Square s = new Square(transform, TempTriangles);
                    Squares.Add(s);
                }
            }

            RenderMesh();
            /* if (oceanic)
                 rb.angularVelocity = new Vector3(0, 0, .1f);*/

        }
        public void RenderMesh()
        {
            mesh.triangles = null;
            mesh.vertices = Triangles.SelectMany(p => p.points).Distinct().ToArray();
            // Triangles.Clear();
            List<int> triConnections = new List<int>();
            foreach (Triangle tri in Triangles)
            {
                triConnections.Add(Array.IndexOf(mesh.vertices, tri.points[0]));
                triConnections.Add(Array.IndexOf(mesh.vertices, tri.points[1]));
                triConnections.Add(Array.IndexOf(mesh.vertices, tri.points[2]));
            }
            mesh.triangles = triConnections.ToArray();
        }
        // Update is called once per frame
        void Update()
        {
            if (Time.time > lastUpdate + .2f)
            {
                lastUpdate = Time.time;
                StartCoroutine(CheckCol());
            }

            foreach (Square s in Squares)
            {
                if (s.subductionForce.magnitude > 0)
                    rb.AddForceAtPosition(s.subductionForce, transform.TransformPoint(s.Center));
            }
        }
        IEnumerator CheckCol()
        {
            List<Square> badTris = new List<Square>();
            if (oceanic)
            {
                foreach (Square sqr in Squares)
                {
                    //check for close triangles
                    foreach (Square sqrCompare in mcTest.Squares)
                    {
                        Vector3 TriWorldPos = transform.TransformPoint(sqr.Center);
                        Vector3 TriCompareWorldPos = mcTest.transform.TransformPoint(sqrCompare.Center);
                        if (Mathf.Abs(TriCompareWorldPos.x - TriWorldPos.x) < 1.5f && Mathf.Abs(TriCompareWorldPos.y - TriWorldPos.y) < 1.5f)
                        {
                            foreach (Vector3 pos in sqr.Points)
                            {
                                if (!sqr.Inside[pos])
                                {
                                    Vector3 p = transform.TransformPoint(pos);
                                    bool cont = contains(sqrCompare.PointsWorldPos, p);
                                    sqr.Inside[pos] = cont;
                                }
                                if (!sqr.Inside.ContainsValue(false))
                                    badTris.Add(sqr);
                            }
                        }
                    }
                    if (sqr.Inside.ToList().Where(p => p.Value == false).Count() == sqr.Inside.Count)
                    {
                        sqr.subductionForce = Vector3.zero;
                    }
                    if (sqr.Inside.ContainsValue(true) && sqr.subductionForce == Vector3.zero)
                    {
                        Vector3 subductionDirection = rb.velocity;// + rb.angularVelocity.z * Vector3.Distance(sqr.Center, CenterOfGravity);
                        float deltaX = sqr.Center.x - CenterOfGravity.x;
                        float deltaY = sqr.Center.y - CenterOfGravity.y;
                        float Deg1 = Mathf.Tan(deltaY / deltaX);
                        float xAngular = Mathf.Sin(Deg1) * rb.angularVelocity.z;
                        float yAngular = Mathf.Cos(Deg1) * rb.angularVelocity.z;
                        subductionDirection += new Vector3(xAngular, yAngular);
                        sqr.subductionForce = subductionDirection.normalized * 0.01f;
                    }
                }
                foreach (Square badTri in badTris)
                    Squares.Remove(badTri);
                if (badTris.Count > 0)
                    RenderMesh();
            }
            yield return null;
        }
        public bool contains(List<Vector3> points, Vector3 point)
        {
            int i;
            int j;
            bool result = false;
            for (i = 0, j = points.Count - 1; i < points.Count; j = i++)
            {
                if ((points[i].y > point.y) != (points[j].y > point.y) &&
                    (point.x < (points[j].x - points[i].x) * (point.y - points[i].y) / (points[j].y - points[i].y) + points[i].x))
                {
                    result = !result;
                }
            }
            return result;
        }
        /* public bool Inside(Vector2 aV1, Vector2 aV2, Vector2 aV3, Vector2 aP)
         {
             Vector2 a = aV2 - aV3, b = aV1 - aV3, c = aP - aV3;
             float aLen = a.x * a.x + a.y * a.y;
             float bLen = b.x * b.x + b.y * b.y;
             float ab = a.x * b.x + a.y * b.y;
             float ac = a.x * c.x + a.y * c.y;
             float bc = b.x * c.x + b.y * c.y;
             float d = aLen * bLen - ab * ab;
             float u = (aLen * bc - ab * ac) / d;
             float v = (bLen * ac - ab * bc) / d;
             float w = 1.0f - u - v;
             return (u >= 0.0f) && (u <= 1.0f) && (v >= 0.0f) && (v <= 1.0f) && (w >= 0.0f); //(w <= 1.0f)
         }*/
    }
    public class Triangle
    {
        public List<Vector3> points { get; set; }
        public Vector3 Center { get { return new Vector3(points.Sum(p => p.x) / points.Count, points.Sum(p => p.y) / points.Count, 0); } }
    }
    public class Square
    {
        public Square(Transform t_, List<Triangle> trings)
        {
            t = t_;
            Triangles = trings;
            foreach (Vector3 point in Points)
                Inside.Add(point, false);
        }
        private Transform t;
        public List<Triangle> Triangles { get; set; }
        public Vector3 Center { get { return new Vector3(Triangles.Sum(p => p.Center.x) / Triangles.Count, Triangles.Sum(p => p.Center.y) / Triangles.Count); } }
        public List<Vector3> Points { get { return Triangles.SelectMany(p => p.points).Distinct().ToList(); } }
        public List<Vector3> PointsWorldPos { get { return Points.Select(p => t.TransformPoint(p)).ToList(); } }
        public Dictionary<Vector3, bool> Inside = new Dictionary<Vector3, bool>();
        public Vector3 subductionForce = Vector3.zero;
    }
}
