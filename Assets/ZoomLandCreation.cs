using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ZoomLandCreation
{
    Dictionary<Vector2, bool> Map = new Dictionary<Vector2, bool>();
    int xSize;
    int ySize;
    public ZoomLandCreation(int xSize_ = 10, int ySize_ = 10)
    {
        xSize = xSize_;
        ySize = ySize_;
    }
    // Start is called before the first frame update
    public Dictionary<Vector2, bool> Create(int enhancements)
    {

        for (int x = 0; x < xSize; x++)
            for (int y = 0; y < ySize; y++)
            {
                Map.Add(new Vector2(x, y), false);
            }


        for (int x = xSize / 4; x < xSize * 3 / 4; x++)
            for (int y = ySize / 4; y < ySize * 3 / 4; y++)
                Map[new Vector2(x, y)] = Random.Range(0, 2) == 0;
        for (int x = xSize * 2 / 5; x < xSize * 3 / 5; x++)
            for (int y = ySize * 2 / 5; y < ySize * 3 / 5; y++)
                Map[new Vector2(x, y)] = true;
        for (int i = 0; i < enhancements; i++)
        {
            Enhance();
        }
        return Map;
    }
    public Dictionary<Vector2, bool> CreateEarth(int enhancements)
    {
        for (int x = 0; x < 16; x++)
            for (int y = 0; y < 10; y++)
            {
                Map.Add(new Vector2(x, y), false);
            }
        Map[new Vector2(0, 8)] = true;
        Map[new Vector2(1, 8)] = true;
        Map[new Vector2(2, 8)] = true;
        Map[new Vector2(3, 8)] = true;
        Map[new Vector2(8, 8)] = true;
        Map[new Vector2(9, 8)] = true;
        Map[new Vector2(10, 8)] = true;
        Map[new Vector2(11, 8)] = true;
        Map[new Vector2(12, 8)] = true;
        Map[new Vector2(13, 8)] = true;
        Map[new Vector2(14, 8)] = true;
        Map[new Vector2(15, 8)] = true;

        Map[new Vector2(2, 7)] = true;
        Map[new Vector2(3, 7)] = true;
        Map[new Vector2(4, 7)] = true;
        Map[new Vector2(8, 7)] = true;
        Map[new Vector2(9, 7)] = true;
        Map[new Vector2(10, 7)] = true;
        Map[new Vector2(11, 7)] = true;
        Map[new Vector2(12, 7)] = true;
        Map[new Vector2(13, 7)] = true;

        Map[new Vector2(2, 6)] = true;
        Map[new Vector2(3, 6)] = true;
        Map[new Vector2(4, 6)] = true;
        Map[new Vector2(7, 6)] = true;
        Map[new Vector2(9, 6)] = true;
        Map[new Vector2(10, 6)] = true;
        Map[new Vector2(11, 6)] = true;
        Map[new Vector2(12, 6)] = true;
        Map[new Vector2(13, 6)] = true;

        Map[new Vector2(2, 5)] = true;
        Map[new Vector2(3, 5)] = true;
        Map[new Vector2(7, 5)] = true;
        Map[new Vector2(8, 5)] = true;
        Map[new Vector2(9, 5)] = true;
        Map[new Vector2(10, 5)] = true;
        Map[new Vector2(11, 5)] = true;
        Map[new Vector2(12, 5)] = true;

        Map[new Vector2(3, 4)] = true;
        Map[new Vector2(4, 4)] = true;
        Map[new Vector2(7, 4)] = true;
        Map[new Vector2(8, 4)] = true;
        Map[new Vector2(9, 4)] = true;
        Map[new Vector2(11, 4)] = true;
        Map[new Vector2(12, 4)] = true;

        Map[new Vector2(4, 3)] = true;
        Map[new Vector2(5, 3)] = true;
        Map[new Vector2(8, 3)] = true;
        Map[new Vector2(9, 3)] = true;

        Map[new Vector2(4, 2)] = true;
        Map[new Vector2(5, 2)] = true;
        Map[new Vector2(8, 2)] = true;
        Map[new Vector2(13, 2)] = true;
        Map[new Vector2(14, 2)] = true;

        Map[new Vector2(5, 1)] = true;

        for (int i = 0; i < enhancements; i++)
        {
            Enhance();
        }
        return Map;
    }
    public void Enhance()
    {
        Dictionary<Vector2, bool> NewMap = new Dictionary<Vector2, bool>();
        int newXSize = 2 * (xSize - 1);
        int newYSize = 2 * (ySize - 1);
        for (int x = 0; x < xSize - 1; x++)
            for (int y = 0; y < ySize - 1; y++)
            {
                KeyValuePair<Vector2, bool> a = Map.First(p => p.Key == new Vector2(x, y + 1));
                KeyValuePair<Vector2, bool> b = Map.First(p => p.Key == new Vector2(x + 1, y + 1));
                KeyValuePair<Vector2, bool> c = Map.First(p => p.Key == new Vector2(x, y));
                KeyValuePair<Vector2, bool> d = Map.First(p => p.Key == new Vector2(x + 1, y));
                //set in stone 
                int newX = 2 * x;
                int newY = 2 * y;
                if (!NewMap.ContainsKey(new Vector2(newX, newY)))
                    NewMap.Add(new Vector2(newX, newY), c.Value);
                if (!NewMap.ContainsKey(new Vector2(newX + 2, newY)))
                    NewMap.Add(new Vector2(newX + 2, newY), d.Value);
                if (!NewMap.ContainsKey(new Vector2(newX, newY + 2)))
                    NewMap.Add(new Vector2(newX, newY + 2), a.Value);
                if (!NewMap.ContainsKey(new Vector2(newX + 2, newY + 2)))
                    NewMap.Add(new Vector2(newX + 2, newY + 2), b.Value);

                //a or b
                KeyValuePair<Vector2, bool> newd = new KeyValuePair<Vector2, bool>(new Vector2(newX + 1, newY), Random.Range(0, 2) == 0 ? c.Value : d.Value);
                KeyValuePair<Vector2, bool> newa = new KeyValuePair<Vector2, bool>(new Vector2(newX + 1, newY + 2), Random.Range(0, 2) == 0 ? a.Value : b.Value);
                KeyValuePair<Vector2, bool> newb = new KeyValuePair<Vector2, bool>(new Vector2(newX, newY + 1), Random.Range(0, 2) == 0 ? a.Value : c.Value);
                KeyValuePair<Vector2, bool> newc = new KeyValuePair<Vector2, bool>(new Vector2(newX + 2, newY + 1), Random.Range(0, 2) == 0 ? b.Value : d.Value);
                if (!NewMap.ContainsKey(new Vector2(newX + 1, newY)))
                    NewMap.Add(newd.Key, newd.Value);
                if (!NewMap.ContainsKey(new Vector2(newX + 1, newY + 2)))
                    NewMap.Add(newa.Key, newa.Value);
                if (!NewMap.ContainsKey(new Vector2(newX, newY + 1)))
                    NewMap.Add(newb.Key, newb.Value);
                if (!NewMap.ContainsKey(new Vector2(newX + 2, newY + 1)))
                    NewMap.Add(newc.Key, newc.Value);
                //center
                bool val = true;
                if (newa.Value == newd.Value && newc.Value == newb.Value)
                    val = Random.Range(0, 2) == 0 ? newa.Value : newb.Value;
                else if (newa.Value == newd.Value)
                    val = newa.Value;
                else if (newb.Value == newc.Value)
                    val = newb.Value;
                else
                    val = new List<bool>() { newa.Value, newb.Value, newc.Value, newd.Value }[Random.Range(0, 4)];
                NewMap.Add(new Vector2(newX + 1, newY + 1), val);
            }
        Dictionary<Vector2, bool> newmapForUpdate = new Dictionary<Vector2, bool>();

        foreach (var pair in NewMap)
            newmapForUpdate.Add(pair.Key, pair.Value);
        foreach (var pair in newmapForUpdate)
            if (pair.Key.x == 0 || pair.Key.x == newXSize - 1 || pair.Key.y == 0 || pair.Key.y == newYSize)
                NewMap[pair.Key] = false;
        xSize = newXSize;
        ySize = newYSize;
        Map = NewMap;

    }
    /*
      List<Vector2> vectors = Map.Keys.ToList();
        for(int i = 0; i < growthPass; i++)
        {
            List<Vector2> growth = new List<Vector2>();
            foreach(var pair in Map)
            {
                if(pair.Value)
                    for (int x = -1; x <= 1; x++)
                        for (int y = -1; y <= 1; y++)
                        {
                            if (x == 0 && y == 0)
                                continue;
                            Vector2 check = new Vector2(x + pair.Key.x, y + pair.Key.y);
                            if (!Map.ContainsKey(check))
                                continue;
                            if (!Map[check])
                                if (Random.Range(0, 2) == 0)
                                    growth.Add(check);
                        }
            }
            foreach (Vector2 g in growth)
                Map[g] = true;
        }
     */
}
