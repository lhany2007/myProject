using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerExperience : MonoBehaviour
{
    public static PlayerExperience Instance;

    public Slider ExpSlider;

    public float ExpMaxValue = 30f;

    public int CurrentLevel = 1;

    const float START_EXP = 0f;
    const int EXP_MAX_VALUE_INCREASE = 2;
    const string EXP_TAG = "Exp";

    int lastCheckLevel = 0;

    List<float> ExpGrowthRate = new List<float> { 3, 6, 9, 12, 15, 18, 21 };

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        ExpSlider.maxValue = ExpMaxValue;
        ExpSlider.value = START_EXP;
    }

    public void UpdateExpSlider(int currentExpIndex)
    {
        if (ExpSlider.value + ExpGrowthRate[currentExpIndex] >= ExpMaxValue)
        {
            CurrentLevel += 1;
            
            ExpSlider.value = (ExpSlider.value + ExpGrowthRate[currentExpIndex]) - ExpMaxValue;
        }
        else
        {
            ExpSlider.value += ExpGrowthRate[currentExpIndex];
        }

        // 3레벨 마다 최대 경험치 증가
        if (CurrentLevel % 3 == 0 && CurrentLevel != lastCheckLevel)
        {
            ExpMaxValue += EXP_MAX_VALUE_INCREASE;
            ExpSlider.maxValue = ExpMaxValue;
            lastCheckLevel = CurrentLevel;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(EXP_TAG))
        {
            Destroy(other.gameObject);
            UpdateExpSlider(int.Parse(other.gameObject.name.Split("(")[0]) - 1); // 이름의 숫자부분 가져옴
        }
    }
}
