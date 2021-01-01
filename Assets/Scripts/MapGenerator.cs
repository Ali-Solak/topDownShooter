using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEditor.Playables;
using UnityEngine;
using UnityEngine.UIElements;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector2f = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class MapGenerator : MonoBehaviour
{

    public Map[] maps;
    public Transform tilePrefab;

    public Transform navMeshMaskFloor;

    public Transform NavMeshFloor;
    public Vector2f maxMapSize;

    public Transform obstaclePrefab;
    

    [Range(0, 1)] public float outlinePercent;

    private List<Coord> allTileCoords;
    private Queue<Coord> shuffledTileCoords;
    private Queue<Coord> shuffledOpenTileCoords;


    public float tilesize;

    private  Map currentMap;
    public int mapIndex;
    private Transform[,] tilemap;
    

    private void Start()
    {
        FindObjectOfType<Spawner>().onNewWave += onNewWave;
    }

    void onNewWave(int waveNumber)
    {
        mapIndex = waveNumber - 1;
        generateMap();
    }

    public void generateMap()
    {
        currentMap = maps[mapIndex];
        tilemap = new Transform[(int)currentMap.mapSize.x,(int)currentMap.mapSize.y];
        System.Random prng = new System.Random(currentMap.seed);
        GetComponent<BoxCollider>().size = new Vector3(currentMap.mapSize.x * tilesize, .05f, currentMap.mapSize.y * tilesize);
   

        // Generating Coords
        allTileCoords = new List<Coord>();
        for (int x = 0; x < currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                allTileCoords.Add(new Coord(x, y));
            }
        }
        shuffledTileCoords = new Queue<Coord>(Utility.shuffleArray(allTileCoords.ToArray(), currentMap.seed));

        
        //Create Map Holder Object
        string holderName = "Generated Map";
        if (transform.Find(holderName))
        {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        
        //Spawning Tiles
        for (int x = 0; x < currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                Vector3 tilePos = CoordToPosition(x, y);
                Transform newTile = (Transform) Instantiate(tilePrefab, tilePos, Quaternion.Euler(Vector3.right * 90));
                newTile.localScale = Vector3.one * (1 - outlinePercent) * tilesize;

                newTile.parent = mapHolder;

                tilemap[x, y] = newTile;
            }
        }

        //Spawning Obstacles
        bool[,] obstacleMap = new bool[(int) currentMap.mapSize.x, (int) currentMap.mapSize.y];

        int obstacleCount = (int) (currentMap.mapSize.x * currentMap.mapSize.y * currentMap.obstaclePercent);
        int currentObstacleCount = 0;
        List<Coord> allOpenCoords = new List<Coord>(allTileCoords);
        
        for (int i = 0; i < obstacleCount; i++)
        {
            Coord randomCoord = getRandomCoord();
            obstacleMap[randomCoord.x, randomCoord.y] = true;
            currentObstacleCount++;

            if (randomCoord != currentMap.mapCenter && mapIsFullyAccessable(obstacleMap, currentObstacleCount))
            {
                float obstacleHeight = Mathf.Lerp(currentMap.minObstacleHeight, currentMap.maxObstacleHeight,
                    (float) prng.NextDouble());
                
                Vector3 obstaclePos = CoordToPosition(randomCoord.x, randomCoord.y);

                Transform newObstacle = (Transform) Instantiate(obstaclePrefab, obstaclePos + Vector3.up * obstacleHeight/2f,
                    Quaternion.identity);
                newObstacle.parent = mapHolder;
                newObstacle.localScale = new Vector3( (1 - outlinePercent) * tilesize, obstacleHeight,(1 - outlinePercent) * tilesize);

                Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
                Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);
                float colorPercent = randomCoord.y / (float)currentMap.mapSize.y;
                obstacleMaterial.color = Color.Lerp(currentMap.foregroundColor, currentMap.backgroundColor, colorPercent);
                obstacleRenderer.sharedMaterial = obstacleMaterial;

                allOpenCoords.Remove(randomCoord);
            }
            else
            {
                obstacleMap[randomCoord.x, randomCoord.y] = false;
                currentObstacleCount--;
            }
        }
        
        shuffledOpenTileCoords = new Queue<Coord>(Utility.shuffleArray(allOpenCoords.ToArray(), currentMap.seed));

        //Creating NavMeshMasks
        
        NavMeshFloor.localScale = new Vector3(maxMapSize.x, maxMapSize.y) * tilesize;

        Transform maskLeft = (Transform) Instantiate(navMeshMaskFloor,
            Vector3.left * (currentMap.mapSize.x + maxMapSize.x) / 4f * tilesize, Quaternion.identity);
        maskLeft.parent = mapHolder;
        maskLeft.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tilesize;
        
        Transform maskRight = (Transform) Instantiate(navMeshMaskFloor,
            Vector3.right * (currentMap.mapSize.x + maxMapSize.x) / 4f * tilesize, Quaternion.identity);
        maskRight.parent = mapHolder;
        maskRight.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tilesize;
        
        Transform maskTop = (Transform) Instantiate(navMeshMaskFloor,
            Vector3.forward * (currentMap.mapSize.y + maxMapSize.y) / 4f * tilesize, Quaternion.identity);
        maskTop.parent = mapHolder;
        maskTop.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y)/2f) * tilesize;
        
        Transform maskBottom = (Transform) Instantiate(navMeshMaskFloor,
            Vector3.back * (currentMap.mapSize.y + maxMapSize.y) / 4f * tilesize, Quaternion.identity);
        maskBottom.parent = mapHolder;
        maskBottom.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y)/2f) * tilesize;
    }

    bool mapIsFullyAccessable(bool[,] obstacleMap, int currentObstacleCount)
    {
        bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];
        Queue<Coord> queue = new Queue<Coord>();

        queue.Enqueue(currentMap.mapCenter);
        mapFlags[currentMap.mapCenter.x, currentMap.mapCenter.y] = true;

        int accesibleTileCount = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    int neighborX = tile.x + x;
                    int neighborY = tile.y + y;

                    if (x == 0 || y == 0)
                    {
                        if (neighborX >= 0 && neighborX < obstacleMap.GetLength(0) && neighborY >= 0 &&
                            neighborY < obstacleMap.GetLength(1))
                        {
                            if (!mapFlags[neighborX, neighborY] && !obstacleMap[neighborX, neighborY])
                            {
                                mapFlags[neighborX, neighborY] = true;
                                queue.Enqueue(new Coord(neighborX, neighborY));
                                accesibleTileCount++;
                            }
                        }
                    }
                }
            }
        }

        int targetAccebilTileCount = (int) (currentMap.mapSize.x * currentMap.mapSize.y - currentObstacleCount);
        return targetAccebilTileCount == accesibleTileCount;
    }

    Vector3 CoordToPosition(int x, int y)
    {
        return new Vector3(-currentMap.mapSize.x / 2f + 0.5f + x, 0, -currentMap.mapSize.y / 2f + 0.5f + y) * tilesize;
    }

    public Transform getOpenRandomCoord()
    {
        Coord randomCoord = shuffledOpenTileCoords.Dequeue();
        shuffledOpenTileCoords.Enqueue(randomCoord);
        return tilemap[randomCoord.x, randomCoord.y];
    }

    public Coord getRandomCoord()
    {
        Coord randomCoord = shuffledTileCoords.Dequeue();
        shuffledTileCoords.Enqueue(randomCoord);
        return randomCoord;
    }

    public Transform getTileFromPos(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x / tilesize + (currentMap.mapSize.x - 1) / 2f);
        int y = Mathf.RoundToInt(position.z / tilesize + (currentMap.mapSize.y - 1) / 2f);

        
        x = Mathf.Clamp(x, 0, tilemap.GetLength(0) -1);
        y = Mathf.Clamp(y, 0, tilemap.GetLength(1) -1);
        return tilemap[x, y];
    }

    public struct Coord
    {
        public int x;
        public int y;

        public Coord(int _x, int _y)
        {
            x = _x;
            y = _y;
        }

        public static bool operator ==(Coord c1, Coord c2f)
        {
            return c1.x == c2f.x && c1.y == c2f.y;
        }

        public static bool operator !=(Coord c1, Coord c2f)
        {
            return !(c1 == c2f);
        }
    }

    [System.Serializable]
    public class Map
    {
        public Vector2 mapSize;
        [Range(0,1)]
        public float obstaclePercent;
        public int seed;
        public float minObstacleHeight;
        public float maxObstacleHeight;
        public Color foregroundColor;
        public Color backgroundColor;

        public Coord mapCenter
        {
            get
            {
                return new Coord((int)mapSize.x / 2, (int)mapSize.y / 2);
            }
        }
    }
}