using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        InvokeRepeating(nameof(InvokePeriodically), 1f, 150); // 2분30초마다 실행
    }

    void InvokePeriodically()
    {
        IncreaseMonsterSpawnRate();
        SpawnNewMonster();
        ExpIndexIncrease();
        RegenerationTimeIncrease();
    }

    public void IncreaseMonsterSpawnRate()
    {

    }

    public void SpawnNewMonster()
    {

    }

    public void ExpIndexIncrease()
    {
        ExperienceManager.Instance.TimeIndex++;
    }

    public void RegenerationTimeIncrease()
    {
        ExperienceManager.Instance.RegenerationTime += 0.5f;
    }
}
