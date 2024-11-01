using UnityEngine;
using System.Collections.Generic;

public class TextPulseEffect : MonoBehaviour
{
    float time = 0f;
    float size = 0.15f;
    float upSizeTime = 2.8f; // 3초 동안 커짐
    float downSizeTime = 2.8f; // 3초 동안 작아짐

    RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        float cycleTime = upSizeTime + downSizeTime; // 전체 사이클 시간
        time += Time.deltaTime;

        // 주기적으로 time을 0에서 cycleTime 사이에서 반복되도록 함
        if (time > cycleTime)
        {
            time -= cycleTime;
        }

        // 커지는 애니매이션
        if (time <= upSizeTime)
        {
            rectTransform.localScale = Vector3.one * (1 + size * (time / upSizeTime));
        }
        // 작아지는 애니매이션
        else
        {
            float downTime = time - upSizeTime;
            rectTransform.localScale = Vector3.one * (1 + size * (1 - (downTime / downSizeTime)));
        }
    }
}
