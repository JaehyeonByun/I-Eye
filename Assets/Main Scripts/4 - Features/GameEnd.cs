using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEnd : MonoBehaviour
{
    [SerializeField] private AudioSource _firstPigVoice;
    [SerializeField] private AudioSource _secondPigVoice;
    
    private bool isPlayed = false;
    void Start()
    {
        StartCoroutine(PlayVoice());
        Invoke("ExitGame", 20f);
    }

    IEnumerator PlayVoice()
    {
        yield return new WaitForSeconds(4f); 
        _firstPigVoice.Play();
        
        yield return new WaitForSeconds(5f); 
        _secondPigVoice.Play();
    }
    
    void ExitGame()
    { 
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
