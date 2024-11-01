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
        // �����Ǵ� �ִ� ��ǥ
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
                Debug.Log("�ε��� ���� �ذ��ϼ�");
                return;
            }
            // ȸ��x, ��ũ��Ʈ�� �Ҵ�Ǿ� �ִ� ������Ʈ�� �ڽ����� ����
            Instantiate(ExpOrbList[TimeIndex], SpawnLocation, Quaternion.identity, transform);
        }
    }

    public void SpawnDropExpOrb(GameObject deadMonster, int monsterDifficulty)
    {
        Instantiate(ExpOrbList[monsterDifficulty], deadMonster.transform.position, Quaternion.identity, transform);
    }
}
