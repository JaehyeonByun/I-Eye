using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class House : MonoBehaviour
{
    public int BricksLeft = 0;
    public int IncorrectTheshhold = 0;
    public int Incorrect = 0;

    [SerializeField] private GameObject _clearUI;
    [SerializeField] private AudioSource _clearSound;
    [SerializeField] private AudioSource _clearVoice;
    [SerializeField] private AudioSource _restartSound;
    [SerializeField] private AudioSource _restartVoice;


    private List<Vector3> occupiedPositions = new List<Vector3>();
    private List<Vector3> incorrectPositions = new List<Vector3>();

    private bool _restart = false;
    private bool _isClear = false;

    void Start()
    {
        _restart = false;
        if (_clearUI.activeSelf)
        {
            _clearUI.SetActive(false);
        }
        BricksLeft = CheckBricks();
        IncorrectTheshhold = BricksLeft / 3;
    }

    public int CheckBricks()
    {
        Transform[] childTransforms = GetComponentsInChildren<Transform>();

        int count = 0;

        foreach (Transform child in childTransforms)
        {
            if (child.name.Contains("Brick"))
            {
                count++;
            }
        }

        return count;
    }

    void Update()
    {
        if (BricksLeft == 0)
        {
            if (!_isClear)
            {
                _isClear = true;
                StartCoroutine(HandleClearSequence());
            }
        }

        if (Incorrect == IncorrectTheshhold)
        {
            if (_restart == false)
            {
                _restart = true;
                Invoke("PlayRestartSound", 1f);

                Invoke("PlayRestartVoice", 6f);

                Invoke("Restart", 10f);
            }
        }
    }
    
    private IEnumerator HandleClearSequence()
    {
        _clearUI.SetActive(true);
        
        yield return new WaitForSeconds(1f);
        _clearSound.Play();

        yield return new WaitForSeconds(3f);
        _clearVoice.Play();
    }

    public bool IsPositionOccupied(Vector3 position)
    {
        foreach (var occupiedPosition in occupiedPositions)
        {
            if (Vector3.Distance(occupiedPosition, position) < 0.1f)
            {
                return true;
            }
        }
        return false;
    }

    public void AddOccupiedPosition(Vector3 position)
    {
        occupiedPositions.Add(position);
        BricksLeft -= 1;
        Debug.Log(BricksLeft);
    }
    
    public bool IsIncorrectPositionRecorded(Vector3 position)
    {
        foreach (var incorrectPosition in incorrectPositions)
        {
            if (Vector3.Distance(incorrectPosition, position) < 0.1f)
            {
                return true;
            }
        }
        return false;
    }

    public void RecordIncorrectPosition(Vector3 position)
    {
        incorrectPositions.Add(position);
    }

    public void Restart()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
    private void PlayRestartSound()
    {
        _restartSound.Play();
    }

    private void PlayRestartVoice()
    {
        _restartVoice.Play();
    }
}