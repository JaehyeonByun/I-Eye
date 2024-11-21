using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestStatic : MonoBehaviour
{
    private List<int> intList = new List<int>();

    void Start()
    {
        intList.Add(10);
        intList.Add(20);
        intList.Add(30);
        intList.Add(40);
        intList.Add(50);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            PrintList();
        }
    }

    void PrintList()
    {
        foreach (int number in intList)
        {
            Debug.Log(number);
        }
    }
}
