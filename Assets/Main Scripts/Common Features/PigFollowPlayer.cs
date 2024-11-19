using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PigFollowPlayer : MonoBehaviour
{
    [SerializeField] private Transform player;

    void Update()
    {
        if (player != null)
        {
            Vector3 direction = player.position - transform.position;
            
            direction.y = 0;
            
            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = targetRotation;
            }
        }
    }
}
