using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageTwoUI : MonoBehaviour
{
    public GameObject UI;
    [SerializeField] private AudioSource PigVoice;
    void Start()
    {
        PigVoice.Play();
        if (!gameObject.activeSelf)
        {
            UI.SetActive(true);
        }
    }
    public void OnButtonClick()
    {
        UI.SetActive(false);
    }
}
