using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExperienceManager : MonoBehaviour
{
    public static ExperienceManager Instance;

    public GameObject Player;
    public List<GameObject> ExpOrbList;

    public int ExpSpawnCount = 3;
    public float RegenerationTime = 2f;
    public float MaximumSpawnDistance = 5;

    public int TimeIndex = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        StartCoroutine(GenerateRandomSpawnLocations());
    }

    IEnumerator GenerateRandomSpawnLocations()
    {
        while (true)
        {
            SpawnExpOrbsOverTime();
            yield return new WaitForSeconds(RegenerationTime);
        }
    }

    void SpawnExpOrbsOverTime()
    {
        // 스폰되는 최대 좌표
        float left = Player.transform.position.x - MaximumSpawnDistance;
        float right = Player.transform.position.x + MaximumSpawnDistance;
        float down = Player.transform.position.y - MaximumSpawnDistance;
        float up = Player.transform.position.y + MaximumSpawnDistance;

        for (int i = 0; i < ExpSpawnCount; i++)
        {
            float randomX = Random.Range(left, right);
            float randomY = Random.Range(down, up);

            Vector2 SpawnLocation = new Vector2(randomX, randomY);

            if (ExpOrbList.Count - 1 == TimeIndex)
            {
                Debug.Log("인덱스 문제 해결하셈");
                return;
            }
            // 회전x, 스크립트가 할당되어 있는 오브젝트의 자식으로 생성
            Instantiate(ExpOrbList[TimeIndex], SpawnLocation, Quaternion.identity, transform);
        }
    }

    public void SpawnDropExpOrb(GameObject deadMonster, int monsterDifficulty)
    {
        Instantiate(ExpOrbList[monsterDifficulty], deadMonster.transform.position, Quaternion.identity, transform);
    }
}
