using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public Wave[] waves;
    public Enemy enemy;

    private LivingEntity playerEntity;
    private Transform playerT;

    private Wave currentWave;
    private int currentWaveNumber;
    private int EnemiesRemainingAlive;

    private int enemiesRemainingToSpawn;
    private float nextSpawnTime;

    private MapGenerator map;

    private float timeBetweenCampingChecks = 2;
    private float campThresholdDistance = 1.5f;
    private float nextCampCheckTime;
    private Vector3 campPosOld;
    private bool isCamping;

    private bool isDisabled;

    public event System.Action<int> onNewWave;
    

    private void Start()
    {

        playerEntity = FindObjectOfType<Player>();
        playerT = playerEntity.transform;

        nextCampCheckTime = timeBetweenCampingChecks + Time.time;
        campPosOld = playerT.position;
        playerEntity.OnDeath += onPlayerDeath;
        
        map = FindObjectOfType<MapGenerator>();
        nextWave();
    }

    private void Update()
    {
        if (!isDisabled)
        {

            if (Time.time > nextCampCheckTime)
            {
                nextCampCheckTime = Time.time + timeBetweenCampingChecks;

                isCamping = (Vector3.Distance(playerT.position, campPosOld) < campThresholdDistance);

                campPosOld = playerT.position;
            }

            if (enemiesRemainingToSpawn > 0 && Time.time > nextSpawnTime)
            {
                enemiesRemainingToSpawn--;
                nextSpawnTime = Time.time + currentWave.timeBetweenSpawn;

                StartCoroutine(spawnEnemy());

            }
        }
    }

    IEnumerator spawnEnemy()
    {
        float spawnDelay = 1;
        float tileFlashSpeed = 4;

        Transform randomTile = map.getOpenRandomCoord();
        
        if (isCamping)
        {
            randomTile = map.getTileFromPos(playerT.position);
        }
        
        Material tileMat = randomTile.GetComponent<Renderer>().material;
        Color initialColor = tileMat.color;
        Color flashcolor = Color.red;

        float spawnTimer = 0f;

        while (spawnTimer < spawnDelay)
        {
            tileMat.color = Color.Lerp(initialColor, flashcolor, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1));
            
            spawnTimer += Time.deltaTime;
            yield return null;
        }

        Enemy spawnedEnemy = (Enemy) Instantiate(enemy, randomTile.position + Vector3.up, Quaternion.identity);
        spawnedEnemy.OnDeath += onEnemyDeath;
    }

    void onEnemyDeath()
    {
        EnemiesRemainingAlive--;

        if (EnemiesRemainingAlive <= 0)
        {
            nextWave();
        }
    }
    
    void onPlayerDeath()
    {
        isDisabled = true;
    }

    private void nextWave()
    {
        if (currentWaveNumber - 1 < waves.Length - 1)
        {
            currentWaveNumber++;
            currentWave = waves[currentWaveNumber - 1];

            enemiesRemainingToSpawn = currentWave.enemyCount;

            EnemiesRemainingAlive = enemiesRemainingToSpawn;

            if (onNewWave != null)
            {
                onNewWave(currentWaveNumber);
            }
            resetPlayerPos();
        }
    }

    void resetPlayerPos()
    {
        playerT.position = map.getTileFromPos(Vector3.zero).position + Vector3.up * 3;
    }

    [System.Serializable]
    public class Wave
    {
        public int enemyCount;
        public float timeBetweenSpawn;
    }
}