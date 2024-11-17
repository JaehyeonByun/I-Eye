using UnityEngine;

public class GazeRayCast : MonoBehaviour
{
    private GameObject lastHitObject = null; // 이전에 감지된 오브젝트 저장

    void Update()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            // 현재 감지된 오브젝트가 이전에 감지된 오브젝트와 다를 때만 로그 출력
            if (hit.collider.gameObject != lastHitObject)
            {
                Debug.Log(hit.collider.gameObject.name); // 오브젝트 이름 출력
                lastHitObject = hit.collider.gameObject; // 마지막 감지 오브젝트 업데이트
            }
        }
        else
        {
            lastHitObject = null; // 아무것도 감지되지 않으면 마지막 감지 오브젝트 초기화
        }
    }
}