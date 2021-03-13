using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OLDCODE
{
    public class Continent
    {
        public Vector2 SubductionForce = Vector2.zero;
        public WorldPointContinent WorldPointCenter;
        public Dictionary<Vector2, ContinentPoint> Points = new Dictionary<Vector2, ContinentPoint>();
        public Color c;
        public int driftDuration = 0;
        public Vector2 driftDirection;
        public Continent(WorldPointContinent center)
        {
            WorldPointCenter = center;
            Points.Add(new Vector2(0, 0), new ContinentPoint(new Vector2(0, 0), this));
        }
        public void RecalculateEdges(ContinentSimulation Sim)
        {
            foreach (ContinentPoint point in Points.Values)
            {
                List<ContinentPoint> neighbors = GetNeighbors(point);
                WorldPointContinent worldPoint = ContinentPointToWorldPoint(point, Sim);
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

        public WorldPointContinent ContinentPointToWorldPoint(ContinentPoint point, ContinentSimulation Sim)
        {
            Vector2 mod = Vector2.zero;
            Vector2 newPos = point.Pos + WorldPointCenter.Pos;
            if (newPos.x > Sim.Width - 1)
                mod = new Vector2(-Sim.Width, 0);
            if (newPos.x > Sim.Width * 2 - 1)
                mod = new Vector2(-Sim.Width * 2, 0);
            if (newPos.x < 0)
                mod = new Vector2(Sim.Width, 0);
            if (newPos.x < -Sim.Width)
                mod = new Vector2(Sim.Width * 2, 0);
            if (newPos.y >= 100)
                return null;
            if (newPos.y < 0)
                return null;
            if (!Sim.WorldPointContinents.ContainsKey(newPos + mod))
            {
                Debug.Log("Bad Vector: " + (point.Pos + WorldPointCenter.Pos + mod) + " points: " + point.Pos + WorldPointCenter.Pos + mod);
            }
            return Sim.WorldPointContinents[point.Pos + WorldPointCenter.Pos + mod];
        }
        public WorldPointContinent ContinentPointToWorldPoint(ContinentPoint point, WorldCreationStatic Sim)
        {
            Vector2 mod = Vector2.zero;
            Vector2 newPos = point.Pos + WorldPointCenter.Pos;
            if (newPos.x > Sim.Width - 1)
                mod = new Vector2(-Sim.Width, 0);
            if (newPos.x > Sim.Width * 2 - 1)
                mod = new Vector2(-Sim.Width * 2, 0);
            if (newPos.x < 0)
                mod = new Vector2(Sim.Width, 0);
            if (newPos.x < -Sim.Width)
                mod = new Vector2(Sim.Width * 2, 0);
            if (newPos.y >= 100)
                return null;
            if (newPos.y < 0)
                return null;
            if (!Sim.WorldPointContinents.ContainsKey(newPos + mod))
            {
                Debug.Log("Bad Vector: " + (point.Pos + WorldPointCenter.Pos + mod) + " points: " + point.Pos + WorldPointCenter.Pos + mod);
            }
            return Sim.WorldPointContinents[point.Pos + WorldPointCenter.Pos + mod];
        }
        public List<ContinentPoint> GetNeighbors(ContinentPoint point)
        {
            List<ContinentPoint> Neighbors = new List<ContinentPoint>();
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
        public List<ContinentPoint> GetXNeighbors(ContinentPoint point, int width)
        {
            List<ContinentPoint> Neighbors = new List<ContinentPoint>();
            for (int x = -width; x <= width; x++)
                for (int y = -width; y <= width; y++)
                {
                    if (x == 0 && y == 0)
                        continue;
                    if (Points.ContainsKey(new Vector2(x + point.Pos.x, y + point.Pos.y)))
                        Neighbors.Add(Points[new Vector2(x + point.Pos.x, y + point.Pos.y)]);

                }
            return Neighbors;
        }
    }
    public class ContinentPoint
    {
        public ContinentPoint(Vector2 pos, Continent cont_)
        {
            Pos = pos;
            cont = cont_;
        }
        public Continent cont;
        public Vector2 Pos;
        public bool Edge = true;
        public int height = 0;
    }
}