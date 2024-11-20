using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LostShovel : MonoBehaviour
{
    [SerializeField] private AudioSource _failSound;
    void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("삽"))
        {
            _failSound.Play();
            StartCoroutine(ReloadSceneAfterDelay(5f));
        }
    }
    private IEnumerator ReloadSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
    }
}
