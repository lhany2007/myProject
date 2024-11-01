using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ArgumentSender : MonoBehaviour
{
    [SerializeField] List<Button> classButtonList;
    List<string> classNameCheckList = new List<string>() { "Warrior", "Assassin", "Wizard", "Ranger" };

    void Start()
    {
        for (int i = 0; i < classButtonList.Count; i++)
        {
            int index = i;
            classButtonList[i].onClick.AddListener(() => OnButtonClick(index));
        }
    }

    void OnButtonClick(int index)
    {
        if (classButtonList[index].name != classNameCheckList[index])
        {
            Debug.Log("�߸� �Ҵ���");
            return;
        }

        NextScene.Instance.NameNext("Main");
        // ���⿣ Main������ �� ���� ���ڰ��� ����
    }
}
