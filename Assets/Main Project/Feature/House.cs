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

    private List<Vector3> occupiedPositions = new List<Vector3>();
    private List<Vector3> incorrectPositions = new List<Vector3>();

    void Start()
    {
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
            _clearUI.SetActive(true);
            _clearSound.Play();
        }

        if (Incorrect == IncorrectTheshhold)
        {
            StartCoroutine(RestartAfterDelay(6f));
        }
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
    private IEnumerator RestartAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Restart();
    }
}