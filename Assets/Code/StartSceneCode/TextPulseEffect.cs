using UnityEngine;
using System.Collections.Generic;

public class TextPulseEffect : MonoBehaviour
{
    float time = 0f;
    float size = 0.15f;
    float upSizeTime = 2.8f; // 3�� ���� Ŀ��
    float downSizeTime = 2.8f; // 3�� ���� �۾���

    RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        float cycleTime = upSizeTime + downSizeTime; // ��ü ����Ŭ �ð�
        time += Time.deltaTime;

        // �ֱ������� time�� 0���� cycleTime ���̿��� �ݺ��ǵ��� ��
        if (time > cycleTime)
        {
            time -= cycleTime;
        }

        // Ŀ���� �ִϸ��̼�
        if (time <= upSizeTime)
        {
            rectTransform.localScale = Vector3.one * (1 + size * (time / upSizeTime));
        }
        // �۾����� �ִϸ��̼�
        else
        {
            float downTime = time - upSizeTime;
            rectTransform.localScale = Vector3.one * (1 + size * (1 - (downTime / downSizeTime)));
        }
    }
}
