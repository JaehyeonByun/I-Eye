using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalculateDistance : MonoBehaviour
{
    private Vector3 lastPosition; 
    private float totalDistance;  
    private float startTime;      
    private float elapsedTime;   

    private bool _isGameStart = false;
    private bool _isGameEnd = false;   

    void Start()
    {
        lastPosition = new Vector3(transform.position.x, 0, transform.position.z);
        totalDistance = 0f;
        elapsedTime = 0f;
    }

    void Update()
    {
        if (_isGameStart && !_isGameEnd)
        {
            elapsedTime = Time.time - startTime;
            
            Vector3 currentPosition = new Vector3(transform.position.x, 0, transform.position.z);
            float distance = Vector3.Distance(currentPosition, lastPosition);
            totalDistance += distance;
            lastPosition = currentPosition;
        }
    }

    public void StartGame()
    {
        if (!_isGameStart)
        {
            _isGameStart = true;
            _isGameEnd = false;
            startTime = Time.time;
            totalDistance = 0f;   
            elapsedTime = 0f;      
        }
    }

    public void EndGame()
    {
        if (_isGameStart && !_isGameEnd)
        {
            _isGameEnd = true;
            elapsedTime = Time.time - startTime;
            Debug.Log($"Game Ended. Total Distance: {totalDistance} units, Total Time: {elapsedTime} seconds.");
        }
    }

    public float GetTotalDistance()
    {
        return totalDistance;
    }

    public float GetElapsedTime()
    {
        return elapsedTime;
    }

    public void ResetData()
    {
        totalDistance = 0f;
        elapsedTime = 0f;
        _isGameStart = false;
        _isGameEnd = false;
    }
}
