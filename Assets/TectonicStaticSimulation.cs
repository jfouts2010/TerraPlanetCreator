using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TectonicStaticSimulation : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject cubePrefab;

    int worldWidth = 200;
    int worldHeight = 100;
    public int octaves = 4;
    public float lac = 2f;
    public float pers = 0.5f;
    public float multi = 2.5f;
    public int age = 0;
    public float zoom = 1;
    List<Color> colors = new List<Color>();
    List<GameObject> EarthLandRenderGameObjects = new List<GameObject>();
    List<GameObject> WindRenderGameObjects = new List<GameObject>();
    List<GameObject> OceanCurrentGameObjects = new List<GameObject>();
    public Dictionary<Vector2, EarthPoint> Points = new Dictionary<Vector2, EarthPoint>();
    List<Plate> Plates = new List<Plate>();
    Dictionary<string, Color> ClimateColors = new Dictionary<string, Color>();
    Dictionary<string, Color> GeographicClimateColors = new Dictionary<string, Color>();
    void Start()
    {
        //UGLY
        colors.Add(new Color(22f / 255f, 81f / 255f, 1));
        colors.Add(new Color(18f / 255f, 95f / 255f, 1));
        colors.Add(new Color(14f / 255f, 105f / 255f, 1));
        colors.Add(new Color(14f / 255f, 115f / 255f, 1));
        colors.Add(new Color(18f / 255f, 128f / 255f, 1));
        colors.Add(new Color32(61, 42, 0, 255));
        colors.Add(new Color32(61, 42, 0, 255));
        colors.Add(new Color32(61, 42, 0, 255));
        colors.Add(new Color32(61, 42, 0, 255));
        colors.Add(new Color32(61, 42, 0, 255));

        //UGLY! just setting colors for climate zones and the world map 
        ClimateColors.Add(Climate.TropicalRainforest.ToString(), new Color32(36, 3, 221, 255));
        ClimateColors.Add(Climate.TropicalSavannah.ToString(), new Color32(71, 170, 250, 255));
        ClimateColors.Add(Climate.TropicalMonsoon.ToString(), new Color32(1, 104, 255, 255));
        ClimateColors.Add(Climate.Desert.ToString(), Color.red);
        ClimateColors.Add(Climate.HotSteppe.ToString(), new Color32(244, 164, 7, 255));
        ClimateColors.Add(Climate.HumidContinental.ToString(), new Color32(0, 255, 255, 255));
        ClimateColors.Add(Climate.SubarcticContinental.ToString(), new Color32(0, 126, 200, 255));
        ClimateColors.Add(Climate.Mediterranean.ToString(), new Color32(255, 255, 0, 255));
        ClimateColors.Add(Climate.HumidSubtropical.ToString(), new Color32(149, 255, 148, 255));
        ClimateColors.Add(Climate.Oceanic.ToString(), Color.green);
        ClimateColors.Add(Climate.ColdDesert.ToString(), new Color32(255, 150, 150, 255));
        ClimateColors.Add(Climate.ColdSteppe.ToString(), new Color32(255, 220, 100, 255));
        ClimateColors.Add(Climate.Tundra.ToString(), Color.grey);
        ClimateColors.Add(Climate.IceCap.ToString(), Color.white);
        ClimateColors.Add(Climate.Grassland.ToString(), Color.black);
        GeographicClimateColors.Add(Climate.TropicalRainforest.ToString(), new Color32(28, 46, 20, 255));
        GeographicClimateColors.Add(Climate.TropicalSavannah.ToString(), new Color32(85, 115, 23, 255));
        GeographicClimateColors.Add(Climate.TropicalMonsoon.ToString(), new Color32(99, 132, 63, 255));
        GeographicClimateColors.Add(Climate.Desert.ToString(), new Color32(250, 230, 157, 255));
        GeographicClimateColors.Add(Climate.HotSteppe.ToString(), new Color32(212, 174, 127, 255));
        GeographicClimateColors.Add(Climate.HumidContinental.ToString(), new Color32(49, 76, 35, 255));
        GeographicClimateColors.Add(Climate.SubarcticContinental.ToString(), new Color32(35, 49, 24, 255));
        GeographicClimateColors.Add(Climate.Mediterranean.ToString(), new Color32(86, 83, 40, 255));
        GeographicClimateColors.Add(Climate.HumidSubtropical.ToString(), new Color32(32, 61, 17, 255));
        GeographicClimateColors.Add(Climate.Oceanic.ToString(), new Color32(53, 92, 39, 255));
        GeographicClimateColors.Add(Climate.ColdDesert.ToString(), new Color32(196, 174, 127, 255));
        GeographicClimateColors.Add(Climate.ColdSteppe.ToString(), new Color32(126, 114, 76, 255));
        GeographicClimateColors.Add(Climate.Tundra.ToString(), new Color32(88, 83, 51, 255));
        GeographicClimateColors.Add(Climate.IceCap.ToString(), Color.white);
        GeographicClimateColors.Add(Climate.Grassland.ToString(), Color.black);
        
        //create the points that hold data, everything on an INT level between 0 -> width for x, 0 -> height for y
        for (int x = 0; x < worldWidth; x++)
            for (int y = 0; y < worldHeight; y++)
            {
                EarthPoint ep = new EarthPoint(new Vector2(x, y));
                Points.Add(new Vector2(x, y), ep);
            }
        CreateTectonics();
        DeterminePlateDirection();
        CreateContinents();
        CreateMountains();
        OceanDepth();
        CreateIslands();
        CalcWindDirection();
        CalcOceanDirection();
        CalcPrecipitation();
        CalcClimate();
        ColorWorld();
    }
    void Update()
    {

    }
    public void CalcClimate()
    {
        //pulled data from
        //https://www.youtube.com/watch?v=5lCbxMZJ4zA&t=7s&ab_channel=Artifexian
        //can really expand how well it creates climates based on precipitation, not using precipitation much

        int equator = (int)(worldHeight / 2f);
        int upperPolarFront = (int)(worldHeight * 5f / 6f);
        int lowerPolarFront = (int)(worldHeight * 1f / 6f);

        int upperSubtropic = (int)(worldHeight * 4f / 6f);
        int lowerSubtropic = (int)(worldHeight * 2f / 6f);

        //first pass
        foreach (EarthPoint wp in Points.Values.Where(p => p.land && p.height < 7))
        {
            List<EarthPoint> neighbors = Neighbors(wp.pos).Where(p => p.height < 7).ToList();
            int PositionRelToEquator = (int)Mathf.Abs(wp.pos.y - equator); //we use this to make it easier to translate to N/S

            //We just go down and fill in the world with a climate
            if (wp.Equator && wp.height == 5 && wp.precipitation == 4)
            {
                wp.climate = Climate.TropicalRainforest;
            }
            else if (PositionRelToEquator < EarthDegreesToY(20) && wp.height == 5)
            {
                wp.climate = Climate.TropicalSavannah;
            }
            if (PositionRelToEquator < EarthDegreesToY(30) && wp.height == 5 && wp.precipitation >= 3 && wp.OnshoreWinds)
            {
                wp.climate = Climate.TropicalMonsoon;
            }

            if (PositionRelToEquator < EarthDegreesToY(30) && PositionRelToEquator > EarthDegreesToY(10) && wp.precipitation == 0)
            {
                wp.climate = Climate.Desert;
            }
            if (PositionRelToEquator < EarthDegreesToY(30) && PositionRelToEquator > EarthDegreesToY(10) && wp.climate == Climate.Grassland)
            {
                wp.climate = Climate.HotSteppe;
            }
            if (PositionRelToEquator < EarthDegreesToY(60) && PositionRelToEquator > EarthDegreesToY(30))
            {
                wp.climate = Climate.HumidContinental;
            }
            if (PositionRelToEquator < EarthDegreesToY(75) && PositionRelToEquator > EarthDegreesToY(60))
            {
                //add climate to neighbors
                wp.climate = Climate.SubarcticContinental;
                foreach (var neighbor in neighbors.Where(p => p.land))
                    if (neighbor.climate == Climate.Grassland || neighbor.climate == Climate.HumidContinental)
                        neighbor.climate = Climate.SubarcticContinental;
            }
            if (PositionRelToEquator < EarthDegreesToY(75) && PositionRelToEquator > EarthDegreesToY(40) && (wp.OffshoreWinds || wp.ColdCurrent))
            {
                wp.climate = Climate.SubarcticContinental;
                foreach (var neighbor in neighbors.Where(p => p.land))
                    if (neighbor.climate == Climate.Grassland || neighbor.climate == Climate.HumidContinental)
                        neighbor.climate = Climate.SubarcticContinental;
            }
            if (PositionRelToEquator < EarthDegreesToY(40) && PositionRelToEquator > EarthDegreesToY(30) && wp.ColdCurrent)
            {
                wp.climate = Climate.Mediterranean;
                foreach (var neighbor in neighbors.Where(p => p.land))
                    if (neighbor.climate == Climate.Grassland || neighbor.climate == Climate.HumidContinental)
                        neighbor.climate = Climate.Mediterranean;
            }
            if (PositionRelToEquator < EarthDegreesToY(40) && PositionRelToEquator > EarthDegreesToY(30) && wp.WarmCurrent)
            {
                wp.climate = Climate.HumidSubtropical;
                foreach (var neighbor in neighbors.Where(p => p.land))
                    if (neighbor.climate == Climate.Grassland || neighbor.climate == Climate.HumidContinental)
                        neighbor.climate = Climate.HumidSubtropical;
            }
            if (PositionRelToEquator < EarthDegreesToY(60) && PositionRelToEquator > EarthDegreesToY(40) && wp.WarmCurrent)
            {
                wp.climate = Climate.Oceanic;
                foreach (var neighbor in neighbors.Where(p => p.land))
                    neighbor.climate = Climate.Oceanic;
            }
            if (PositionRelToEquator < EarthDegreesToY(60) && PositionRelToEquator > EarthDegreesToY(30) && wp.LeewardMountain)
            {
                wp.climate = Climate.ColdDesert;
            }
            if (PositionRelToEquator < EarthDegreesToY(80) && PositionRelToEquator > EarthDegreesToY(60))
            {
                wp.climate = Climate.Tundra;
            }
            if (PositionRelToEquator > EarthDegreesToY(75))
            {
                wp.climate = Climate.IceCap;
            }
        }
        //second pass
        //used for transition zones
        foreach (EarthPoint wp in Points.Values.Where(p => p.land))
        {
            int PositionRelToEquator = (int)Mathf.Abs(wp.pos.y - equator);
            List<EarthPoint> neighbors = Neighbors(wp.pos);
            //hot steppe transition zone to savannah, and where there is no desert
            if (wp.climate == Climate.Desert)
            {
                if (neighbors.Any(p => p.climate == Climate.TropicalSavannah))
                    wp.climate = Climate.HotSteppe;
            }
            else if (wp.climate == Climate.Grassland && PositionRelToEquator < EarthDegreesToY(35) && PositionRelToEquator > EarthDegreesToY(10) && wp.precipitation < 2)
                wp.climate = Climate.HotSteppe;
            //monsoon transition
            if (wp.climate == Climate.TropicalSavannah)
            {
                if (neighbors.Any(p => p.climate == Climate.TropicalRainforest))
                    wp.climate = Climate.TropicalMonsoon;
            }
            //cold steppe transition
            if (PositionRelToEquator < EarthDegreesToY(50) && PositionRelToEquator > EarthDegreesToY(25) && wp.climate == Climate.HumidContinental)
            {
                if (neighbors.Any(p => p.climate == Climate.HotSteppe || p.climate == Climate.ColdDesert))
                    wp.climate = Climate.ColdSteppe;
            }
        }
    }
    public int EarthDegreesToY(float deg)
    {
        return (int)(deg / 180f * worldHeight);
    }
    public void CalcPrecipitation()
    {
        //https://www.youtube.com/watch?v=5lCbxMZJ4zA&t=7s&ab_channel=Artifexian <- not this video but this channel for getting precipitation
        //similar to climate
        int equator = (int)(worldHeight / 2f);
        int upperPolarFront = (int)(worldHeight * 5f / 6f);
        int lowerPolarFront = (int)(worldHeight * 1f / 6f);

        int upperSubtropic = (int)(worldHeight * 4f / 6f);
        int lowerSubtropic = (int)(worldHeight * 2f / 6f);
        foreach (EarthPoint wp in Points.Values.Where(p => p.land))
        {
            List<EarthPoint> neighbors = Neighbors(wp.pos);
            //HIGH PREC
            //low pressure areas, ITCZ and PF
            if (wp.pos.y < equator + worldHeight * 0.05f && wp.pos.y > equator - worldHeight * 0.05f)
                wp.Equator = true;
            if (wp.pos.y < upperPolarFront + worldHeight * 0.05f && wp.pos.y > upperPolarFront - worldHeight * 0.05f)
                wp.HighPressure = true;
            if (wp.pos.y < lowerPolarFront + worldHeight * 0.05f && wp.pos.y > lowerPolarFront - worldHeight * 0.05f)
                wp.HighPressure = true;

            //onshore winds
            Vector2 neededOceanLocation = wp.pos - wp.windDirection;
            if (Points.ContainsKey(neededOceanLocation) && !Points[neededOceanLocation].land)
            {
                wp.OnshoreWinds = true;
                //coastal effects hit neighbors too
                foreach (EarthPoint ep in neighbors.Where(p => p.land))
                    ep.OnshoreWinds = true;
            }

            //warm currents
            if (neighbors.Count(p => p.hotWater) > neighbors.Count(p => p.coldWater))
            {
                wp.WarmCurrent = true;
                //coastal effects hit neighbors too
                /* foreach (EarthPoint ep in neighbors.Where(p => p.land))
                     ep.WarmCurrent = true;*/
            }

            //LOW PREC
            //high pressure area
            if (wp.pos.y < upperSubtropic + worldHeight * 0.05f && wp.pos.y > upperSubtropic - worldHeight * 0.05f)
                wp.LowPressure = true;
            if (wp.pos.y < lowerSubtropic + worldHeight * 0.05f && wp.pos.y > lowerSubtropic - worldHeight * 0.05f)
                wp.LowPressure = true;

            //offshore winds, if the wind goes from this point to an ocean, offshore
            neededOceanLocation = wp.pos + wp.windDirection;
            if (Points.ContainsKey(neededOceanLocation) && !Points[neededOceanLocation].land)
            {
                wp.OffshoreWinds = true;
                //coastal effects hit neighbors too
                foreach (EarthPoint ep in neighbors.Where(p => p.land))
                    ep.OffshoreWinds = true;
            }

            //cold currents
            if (neighbors.Count(p => p.hotWater) < neighbors.Count(p => p.coldWater))
            {
                wp.ColdCurrent = true;
                //coastal effects hit neighbors too
                /*foreach (EarthPoint ep in neighbors.Where(p => p.land))
                    ep.ColdCurrent = true;*/
            }

            //interiors of continents
            //TODO

            //dry side of mountain
            Vector2 mountainCeckLocation = wp.pos - wp.windDirection;
            if (Points.ContainsKey(mountainCeckLocation) && Points[mountainCeckLocation].height > 7)
            {
                wp.LeewardMountain = true;
                foreach (EarthPoint ep in neighbors.Where(p => p.land))
                    ep.LeewardMountain = true;
            }

            //wet side of mountain
            mountainCeckLocation = wp.pos + wp.windDirection;
            if (Points.ContainsKey(mountainCeckLocation) && Points[mountainCeckLocation].height > 7)
            {
                wp.WindwardMountain = true;
                foreach (EarthPoint ep in neighbors.Where(p => p.land))
                    ep.WindwardMountain = true;
            }

            wp.temperature = (int)(1 - Vector2.Distance(new Vector2(wp.pos.x, equator), wp.pos) / equator * 100);
        }
    }
    public void CalcOceanDirection()
    {
        //https://www.youtube.com/watch?v=5lCbxMZJ4zA&t=7s&ab_channel=Artifexian <- not this video but this channel for getting precipitation
        //just checks if land is left or right of the ocean and adds warm or cold water if it is at a certain area latitude
        int equator = (int)(worldHeight / 2f);
        foreach (EarthPoint wp in Points.Values.Where(p => !p.land))
        {
            List<EarthPoint> neighbors = Neighbors(wp.pos).Where(p => p.land && !p.island).ToList();
            if (neighbors.Any(p => p.land && !p.island))
            {
                int PositionRelToEquator = (int)Mathf.Abs(wp.pos.y - equator);
                //needs a point either directly left or right
                if (!neighbors.Any(p => p.pos == wp.pos + new Vector2(1, 0) || p.pos == wp.pos + new Vector2(-1, 0)))
                {
                    continue;
                }
                Vector2 landDirection = Vector2.zero;
                foreach (var p in neighbors)
                {
                    landDirection += p.pos - wp.pos;
                }
                landDirection = landDirection.normalized;
                float angle = Vector2.Angle(landDirection, new Vector2(1, 0));
                if (PositionRelToEquator > EarthDegreesToY(65))
                {
                    if (angle <= 45)
                        wp.coldWater = true;
                    if (angle >= 135)
                        wp.hotWater = true;
                }
                if (PositionRelToEquator < EarthDegreesToY(40))
                {
                    if (angle <= 45)
                        wp.coldWater = true;
                    if (angle >= 135)
                        wp.hotWater = true;
                }
                if (PositionRelToEquator <= EarthDegreesToY(65) && PositionRelToEquator >= EarthDegreesToY(40))
                {
                    if (angle <= 45)
                        wp.hotWater = true;
                    if (angle >= 135)
                        wp.coldWater = true;
                }
            }
        }
    }
    public void CalcWindDirection()
    {
        //https://www.youtube.com/watch?v=5lCbxMZJ4zA&t=7s&ab_channel=Artifexian <- not this video but this channel for getting wind direction
        //apparently wind direction is super easy
        foreach (EarthPoint wp in Points.Values)
        {
            if (wp.pos.y > 5f / 6f * worldHeight)
            {
                wp.windDirection = new Vector2(-1, -1);
            }
            else if (wp.pos.y > 4f / 6f * worldHeight)
            {
                wp.windDirection = new Vector2(1, 1);
            }
            else if (wp.pos.y > 3f / 6f * worldHeight)
            {
                wp.windDirection = new Vector2(-1, -1);
            }
            else if (wp.pos.y > 2f / 6f * worldHeight)
            {
                wp.windDirection = new Vector2(-1, 1);
            }
            else if (wp.pos.y > 1f / 6f * worldHeight)
            {
                wp.windDirection = new Vector2(1, -1);
            }
            else
            {
                wp.windDirection = new Vector2(-1, 1);
            }
        }
    }
    public void ColorWorld()
    {
        GameObject WorldRenderHolder = Instantiate(new GameObject());
        WorldRenderHolder.name = "World";
        GameObject WindRenderHolder = Instantiate(new GameObject());
        WindRenderHolder.SetActive(false);
        WindRenderHolder.transform.position = new Vector3(0, 0, -1);
        WindRenderHolder.name = "Wind";
        GameObject OceanRenderHolder = Instantiate(new GameObject());
        OceanRenderHolder.SetActive(false);
        OceanRenderHolder.name = "Ocean";
        OceanRenderHolder.transform.position = new Vector3(0, 0, -2);
        GameObject PrecRenderHolder = Instantiate(new GameObject());
        PrecRenderHolder.SetActive(false);
        PrecRenderHolder.name = "Prec";
        PrecRenderHolder.transform.position = new Vector3(0, 0, -3);
        GameObject ClimateRenderHolder = Instantiate(new GameObject());
        ClimateRenderHolder.SetActive(false);
        ClimateRenderHolder.name = "Climate";
        ClimateRenderHolder.transform.position = new Vector3(0, 0, -4);
        foreach (var pair in Points)
        {
            GameObject go = Instantiate(cubePrefab);
            go.transform.parent = WorldRenderHolder.transform;
            go.transform.position = pair.Key;
            go.GetComponent<MeshRenderer>().material = Resources.Load<Material>("green");
            go.GetComponent<MeshRenderer>().material.color = pair.Value.height >= 7 || pair.Value.height < 5 ? colors[pair.Value.height] : GeographicClimateColors[pair.Value.climate.ToString()];
            EarthLandRenderGameObjects.Add(go);
            if (pair.Value.hotWater || pair.Value.coldWater)
            {
                go = Instantiate(cubePrefab);
                go.transform.parent = OceanRenderHolder.transform;
                go.transform.localPosition = pair.Key;
                go.GetComponent<MeshRenderer>().material = Resources.Load<Material>("green");
                go.GetComponent<MeshRenderer>().material.color = pair.Value.hotWater ? Color.red : Color.blue;
            }
            if (pair.Value.land)
            {
                go = Instantiate(cubePrefab);
                go.transform.parent = PrecRenderHolder.transform;
                go.transform.localPosition = pair.Key;
                go.GetComponent<MeshRenderer>().material = Resources.Load<Material>("green");
                Color c = Color.black;
                if (pair.Value.precipitation == 4)
                    c = Color.blue;
                if (pair.Value.precipitation == 3)
                    c = Color.cyan;
                if (pair.Value.precipitation == 2)
                    c = Color.green;
                if (pair.Value.precipitation == 1)
                    c = Color.yellow;
                if (pair.Value.precipitation == 0)
                    c = Color.red;
                go.GetComponent<MeshRenderer>().material.color = c;
            }
            if (pair.Value.land)
            {
                go = Instantiate(cubePrefab);
                go.transform.parent = ClimateRenderHolder.transform;
                go.transform.localPosition = pair.Key;
                go.GetComponent<MeshRenderer>().material = Resources.Load<Material>("green");
                go.GetComponent<MeshRenderer>().material.color = ClimateColors[pair.Value.climate.ToString()];
                go.GetComponent<ClickMe>().sim = this;
            }
        }
    }
    public void OceanDepth()
    {
        //can expand later for better looking ocean, for now just for showing tectonic plates
        foreach (Plate p in Plates)
        {
            foreach (Plate p2 in p.NeighboringPlates)
            {
                float dist = Vector2.Distance(p2.center, p.center);
                float modDist = Vector2.Distance(p2.center + p2.direction.normalized, p.center + p.direction.normalized);
                float newDist = modDist - dist;
                if (newDist > 0.75)//atlantic drift apart, vector pointing away
                {
                    foreach (Vector2 v in p.Points)
                    {
                        EarthPoint ep = Points[v];
                        if (!ep.land)
                        {
                            List<EarthPoint> neighbors = Neighbors(v, false, 5);
                            /*foreach (EarthPoint point in neighbors)
                            {
                                if (point.land)
                                    continue;
                                if (Mathf.Abs(point.pos.x - v.x) == 5 || Mathf.Abs(point.pos.y - v.y) == 5)
                                    point.height = 0;
                                if (Mathf.Abs(point.pos.x - v.x) == 4 || Mathf.Abs(point.pos.y - v.y) == 4 && point.height == 0)
                                    point.height = 1;
                                if (Mathf.Abs(point.pos.x - v.x) == 3 || Mathf.Abs(point.pos.y - v.y) == 3 && point.height == 0)
                                    point.height = 2;
                                if (Mathf.Abs(point.pos.x - v.x) == 2 || Mathf.Abs(point.pos.y - v.y) == 2 && point.height == 0)
                                    point.height = 3;
                                if (Mathf.Abs(point.pos.x - v.x) == 1 || Mathf.Abs(point.pos.y - v.y) == 1 && point.height == 0)
                                    point.height = 4;
                            }*/
                            if (ep.collisionPlateNumber == p2.plateNumber && !ep.land)
                                ep.height = 3;
                        }
                    }
                }
                else
                    foreach (Vector2 v in p.Points)
                    {
                        EarthPoint ep = Points[v];
                        if (ep.collisionPlateNumber == p2.plateNumber && !ep.land)
                            if (ep.height == 0)
                                ep.height = 4;
                    }
            }
        }
    }
    public void CreateIslands()
    {
        //subduction islands
        foreach (Plate p in Plates)
        {
            foreach (Plate p2 in p.NeighboringPlates)
            {
                float dist = Vector2.Distance(p2.center, p.center);
                float modDist = Vector2.Distance(p2.center + p2.direction.normalized, p.center + p.direction.normalized);
                float newDist = modDist - dist;
                if (newDist < -0.75)//distance with movement vectors is shorter, they are coming together
                {
                    //create islands on this border
                    foreach (Vector2 v in p2.Points)
                    {
                        EarthPoint ep = Points[v];
                        if (ep.collisionPlateNumber == p.plateNumber)
                        {
                            List<EarthPoint> neighbors = Neighbors(v, true);
                            //nicer water
                            foreach (EarthPoint neighbor in neighbors.Where(p => p.plateNumber == p2.plateNumber && !ep.land))
                                if (Random.Range(0, 20) == 0)
                                    neighbor.height = 4;
                            //5% chance to make islands
                            if (Random.Range(0, 15) == 0)
                            {
                                foreach (EarthPoint neighbor in neighbors.Where(p => p.plateNumber == p2.plateNumber && !ep.land))
                                {
                                    if (Random.Range(0, 2) == 0)
                                    {
                                        neighbor.height = Random.Range(5, 8);
                                        neighbor.island = true;
                                    }
                                }
                            }
                        }
                    }
                    //visual subduction on our side
                    foreach (Vector2 v in p.Points)
                    {
                        EarthPoint ep = Points[v];
                        if (ep.collisionPlateNumber == p2.plateNumber && !ep.land)
                            ep.height = 2;
                    }
                }
            }
        }
        //hotspot islands
        for (int i = 0; i < 15; i++)
        {
            List<EarthPoint> oceanPoints = Points.Values.Where(p => !p.land).ToList();
            EarthPoint start = oceanPoints[Random.Range(0, oceanPoints.Count)];
            Vector2 genDirection = new Vector2(Random.Range(-1, 2), Random.Range(-1, 2));
            if (genDirection == Vector2.zero)
                genDirection = Vector2.one;
            Vector2 lastPos = start.pos;
            for (int m = 0; m < Random.Range(1, 4); m++)
            {
                Vector2 newpos = GetWorldMapPos(lastPos + genDirection + new Vector2(Random.Range(-1, 2), Random.Range(-1, 2)));
                if (lastPos == newpos)
                    continue;
                if (Points.ContainsKey(newpos) && !Points[newpos].land)
                {
                    RaiseGround(newpos, false, true);
                }
                lastPos = newpos;
            }
        }
    }
    public void CreateMountains()
    {
        //first check for ocean subduction plates
        foreach (EarthPoint ep in Points.Values.Where(p => p.collision && p.height == 5))
        {
            RaiseGround(ep.pos, true);
        }

        //old mountains
        for (int i = 0; i < 15; i++)
        {
            List<EarthPoint> landPoints = Points.Values.Where(p => p.height == 5).ToList();
            EarthPoint start = landPoints[Random.Range(0, landPoints.Count)];
            Vector2 genDirection = new Vector2(Random.Range(-1, 2), Random.Range(-1, 2));
            if (genDirection == Vector2.zero)
                genDirection = Vector2.one;
            Vector2 lastPos = start.pos;
            for (int m = 0; m < Random.Range(5, 15); m++)
            {
                Vector2 newpos = GetWorldMapPos(lastPos + genDirection + new Vector2(Random.Range(-1, 2), Random.Range(-1, 2)));
                if (lastPos == newpos)
                    continue;
                if (Points.ContainsKey(newpos) && Points[newpos].land)
                {
                    RaiseGround(newpos);
                }
                lastPos = newpos;
            }
        }
        //hotspots
        for (int i = 0; i < 50; i++)
        {
            List<EarthPoint> landPoints = Points.Values.Where(p => p.land).ToList();
            EarthPoint start = landPoints[Random.Range(0, landPoints.Count)];
            Vector2 genDirection = new Vector2(Random.Range(-1, 2), Random.Range(-1, 2));
            if (genDirection == Vector2.zero)
                genDirection = Vector2.one;
            Vector2 lastPos = start.pos;
            for (int m = 0; m < Random.Range(1, 4); m++)
            {
                Vector2 newpos = GetWorldMapPos(lastPos + genDirection + new Vector2(Random.Range(-1, 2), Random.Range(-1, 2)));
                if (lastPos == newpos)
                    continue;
                if (Points.ContainsKey(newpos) && Points[newpos].land)
                {
                    RaiseGround(newpos, true, false, 2);
                }
                lastPos = newpos;
            }
        }
    }
    public void RaiseGround(Vector2 center, bool onlyFresh = false, bool makeIslands = false, int maxHeight = 5)
    {
        List<EarthPoint> neighbors = Neighbors(center, true);
        foreach (EarthPoint neighbor in neighbors)
        {
            if (onlyFresh && neighbor.height > 5)
                continue;
            int addedHeight = Random.Range(0, maxHeight);
            neighbor.height += addedHeight;
            if (makeIslands && neighbor.land)
                neighbor.island = true;
            if (neighbor.height > 9)
                neighbor.height = 9;
        }
    }
    public void DeterminePlateDirection()
    {
        //random!
        foreach (Plate p in Plates)
        {
            Vector2 direction = Vector2.zero;
            float val = Random.Range(0f, 1f);
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
            p.direction = direction;
        }
    }
    public void CreateContinents()
    {
        while (true)
        {
            Vector2 center = new Vector2((int)Random.Range(worldWidth * 0.1f, worldWidth * 0.9f), (int)Random.Range(worldHeight * .15f, worldHeight * .85f));
            int centerTectonic = Points[center].plateNumber;
            if (Plates.First(p => p.plateNumber == centerTectonic).oceanic)
                continue;
            int contSize = Random.Range(5, 8);
            Dictionary<Vector2, bool> newContinentMap = new ZoomLandCreation(contSize, contSize).Create(3);

            bool ContinentalCollision = Random.Range(0, 10) == 0;
            foreach (Vector2 v in newContinentMap.Keys)
            {
                Vector2 worldMapPos = GetWorldMapPos(center + v - new Vector2(13, 13));
                if (!Points.ContainsKey(worldMapPos))
                    continue;//offmap
                if (newContinentMap[v] && (Points[worldMapPos].plateNumber == centerTectonic || ContinentalCollision))
                {
                    Points[worldMapPos].height = 5;
                }
            }
            float land = Points.Values.Count(p => p.height == 5);
            float ocean = Points.Count - land;
            Debug.Log(land / ocean);
            if (land / ocean > 0.29f)
                break;
          
        }
        foreach (Plate plate in Plates)
        {
            List<EarthPoint> platePoints = Points.Values.Where(p => p.plateNumber == plate.plateNumber).ToList();
            if (platePoints.Count(p => p.land) == 0)
                plate.oceanic = true;
        }
        foreach (EarthPoint ep in Points.Values.Where(p => p.land))
        {
            List<EarthPoint> neigh = Neighbors(ep.pos);
            if (neigh.Any(p => !p.land))
                ep.coast = true;
        }
    }
    public Vector2 GetWorldMapPos(Vector2 v)
    {
        Vector2 mod = Vector2.zero;
        if (v.x > worldWidth - 1)
            mod = new Vector2(-worldWidth, 0);
        if (v.x > worldWidth * 2 - 1)
            mod = new Vector2(-worldWidth * 2, 0);
        if (v.x < 0)
            mod = new Vector2(worldWidth, 0);
        if (v.x < -worldWidth)
            mod = new Vector2(worldWidth * 2, 0);
        return v + mod;
    }
    public void CreateTectonics()
    {
        int count = 0;
        for (int i = 0; i < 10; i++)
        {
            Vector2 voronoi = new Vector2(Random.Range(0, worldWidth), Random.Range(0, worldHeight));
            Plates.Add(new Plate(i, voronoi, false));
        }
        foreach (EarthPoint ep in Points.Values)
        {
            float shortestDist = 999999;
            Plate selectedPlate = null;
            float amp = 1;
            float freq = 1;
            float noiseHeight = 0;
            for (int i = 0; i < octaves; i++)
            {
                var per = Mathf.PerlinNoise((ep.pos.x + 60) / 50f / zoom * freq, (ep.pos.y + 60) / 50f / zoom * freq) * 2.5f - 1;
                noiseHeight += per * amp;
                amp *= pers;
                freq *= lac;
            }
            if (noiseHeight > 1)
                noiseHeight = 1;
            if (noiseHeight < 0)
                noiseHeight = 0;
            noiseHeight *= multi + 5;
            foreach (Plate plate in Plates)
            {
                Vector2 voronCenter = plate.center;
                float xDist1 = Mathf.Abs(voronCenter.x - ep.pos.x) + noiseHeight;
                float xDist2 = Mathf.Abs(voronCenter.x + worldWidth - ep.pos.x) + noiseHeight;
                float xDist3 = Mathf.Abs(voronCenter.x - worldWidth - ep.pos.x) + noiseHeight;
                float xDist = Mathf.Min(xDist1, xDist2, xDist3);
                float yDist = voronCenter.y - ep.pos.y;// + noiseHeight;
                float dist = Mathf.Sqrt(Mathf.Pow(xDist, 2) + Mathf.Pow(yDist, 2));
                if (dist < shortestDist)
                {
                    shortestDist = dist;
                    selectedPlate = plate;
                }
            }
            ep.plateNumber = selectedPlate.plateNumber;
            selectedPlate.Points.Add(ep.pos);
        }
        //can add oceanic plates to prevent continents from spawning there
        //Plates.OrderByDescending(p => p.Points.Count).ToList()[0].oceanic = true;
        //Plates.OrderByDescending(p => p.Points.Count).ToList()[1].oceanic = true;
        foreach (var pair in Points)
        {
            int tecNumber = pair.Value.plateNumber;
            Plate pointPlate = Plates.First(p => p.plateNumber == tecNumber);
            List<EarthPoint> neighbors = Neighbors(pair.Key);
            if (neighbors.Any(p => p.plateNumber != tecNumber))
            {
                EarthPoint collisionNeighbor = neighbors.First(p => p.plateNumber != tecNumber);
                Plate collisionPlate = Plates.First(p => p.plateNumber == collisionNeighbor.plateNumber);
                pair.Value.collisionPlateNumber = collisionNeighbor.plateNumber;
                if (!pointPlate.NeighboringPlates.Contains(collisionPlate))
                    pointPlate.NeighboringPlates.Add(collisionPlate);
            }
        }
    }
    public List<EarthPoint> Neighbors(Vector2 center, bool getCenter = false, int dist = 1)
    {
        List<EarthPoint> neighbors = new List<EarthPoint>();
        for (int x = -dist; x < dist + 1; x++)
            for (int y = -dist; y < dist + 1; y++)
            {
                if (x == 0 && y == 0 && !getCenter)
                    continue;
                Vector2 neigh = GetWorldMapPos(center + new Vector2(x, y));
                if (Points.ContainsKey(neigh))
                    neighbors.Add(Points[neigh]);
            }
        return neighbors;
    }
}
public class EarthPoint
{
    public Vector2 pos;
    public int plateNumber;
    public int height = 0;
    public bool hotWater = false;
    public bool coldWater = false;
    public bool island = false;
    public int precipitation { get { return Mathf.Min(4, Mathf.Max(0, 2 + (Equator ? 2 : 0) + (HighPressure ? 1 : 0) + (OnshoreWinds ? 1 : 0) + (WarmCurrent ? 1 : 0) + (LowPressure ? -2 : 0) + (OffshoreWinds ? -1 : 0) + (ColdCurrent ? -1 : 0) + (Interior ? -1 : 0) + (WindwardMountain ? 1 : 0) + (LeewardMountain ? -1 : 0))); } }
    public bool Equator = false;
    public bool LowPressure = false;
    public bool OnshoreWinds = false;
    public bool WarmCurrent = false;
    public bool HighPressure = false;
    public bool OffshoreWinds = false;
    public bool ColdCurrent = false;
    public bool Interior = false;
    public bool WindwardMountain = false;
    public bool LeewardMountain = false;
    public int temperature;
    public Climate climate = Climate.Grassland;
    public bool coast = false;
    public bool land { get { return height >= 5; } }
    public int? collisionPlateNumber;
    public Vector2 windDirection = Vector2.zero;
    public bool collision { get { return collisionPlateNumber.HasValue; } }
    public EarthPoint(Vector2 pos_)
    {
        pos = pos_;
    }
}
public class Plate
{
    public Plate(int number, Vector2 center_, bool ocean)
    {
        plateNumber = number;
        center = center_;
        oceanic = ocean;
    }
    public List<Vector2> Points = new List<Vector2>();
    public Vector2 center;
    public int plateNumber;
    public Vector2 direction;
    public bool oceanic;
    public List<Plate> NeighboringPlates = new List<Plate>();
}
public enum Climate
{
    Grassland,

    TropicalRainforest,
    TropicalSavannah,
    Desert,
    HotSteppe,
    TropicalMonsoon,

    HumidContinental,
    SubarcticContinental,

    Mediterranean,
    HumidSubtropical,
    Oceanic,

    ColdDesert,
    ColdSteppe,

    Tundra,
    IceCap
}
