using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace OLDCODE
{
    public class TectonicPlate
    {
        public Vector2 SubductionForce = Vector2.zero;
        public TectonicSimulation Sim;
        public WorldPoint WorldPointCenter;
        public Dictionary<Vector2, TectonicPoint> Points = new Dictionary<Vector2, TectonicPoint>();
        public Color c;
        public TectonicPlate(TectonicSimulation tm, WorldPoint center)
        {
            Sim = tm;
            WorldPointCenter = center;
            Points.Add(new Vector2(0, 0), new TectonicPoint(new Vector2(0, 0), 0, this));
        }
        public void RecalculateEdges()
        {
            foreach (TectonicPoint point in Points.Values)
            {
                List<TectonicPoint> neighbors = GetNeighbors(point);
                WorldPoint worldPoint = TectonicPointToWorldPoint(point);
                if (worldPoint == null)
                    continue;
                if (neighbors.Count == 8)
                    point.Edge = false;
                //check if edge
                if (worldPoint.Pos.x == 0 && worldPoint.Pos.y == 0)
                {
                    if (neighbors.Count() == 3)
                        point.Edge = false;
                }
                else if (worldPoint.Pos.x == 0 || worldPoint.Pos.y == 0)
                {
                    if (neighbors.Count() == 5)
                        point.Edge = false;
                }
            }
        }

        public WorldPoint TectonicPointToWorldPoint(TectonicPoint point)
        {
            Vector2 mod = Vector2.zero;
            if (point.Pos.x + WorldPointCenter.Pos.x > 99)
                mod = new Vector2(-100, 0);
            if (point.Pos.x + WorldPointCenter.Pos.x < 0)
                mod = new Vector2(100, 0);
            if (point.Pos.y + WorldPointCenter.Pos.y >= 100)
                return null;
            if (point.Pos.y + WorldPointCenter.Pos.y < 0)
                return null;
            return Sim.WorldPoints[point.Pos + WorldPointCenter.Pos + mod];
        }
        public List<TectonicPoint> GetNeighbors(TectonicPoint point)
        {
            List<TectonicPoint> Neighbors = new List<TectonicPoint>();
            for (int x = -1; x <= 1; x++)
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                        continue;
                    if (Points.ContainsKey(new Vector2(x + point.Pos.x, y + point.Pos.y)))
                        Neighbors.Add(Points[new Vector2(x + point.Pos.x, y + point.Pos.y)]);

                }
            return Neighbors;
        }
    }
    public class TectonicPoint
    {
        public TectonicPoint(Vector2 pos, int age_, TectonicPlate plate_)
        {
            Pos = pos;
            age = age_;
            plate = plate_;
        }
        public TectonicPlate plate;
        public int age;
        public Vector2 Pos;
        public bool Edge = true;
        public bool OK = true;
    }
}