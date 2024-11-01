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
            Debug.Log("잘못 할당함");
            return;
        }

        NextScene.Instance.NameNext("Main");
        // 여기엔 Main씬에서 쓸 직업 인자값을 전달
    }
}
