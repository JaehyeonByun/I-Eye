using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintDisappear : MonoBehaviour
{
    private void OnEnable()
    {
        StartCoroutine(DeactivateAfterTime(3f)); // 3초 후 비활성화
    }

    // 지정된 시간 후에 오브젝트 비활성화
    private IEnumerator DeactivateAfterTime(float time)
    {
        yield return new WaitForSeconds(time); // 지정된 시간만큼 대기
        gameObject.SetActive(false); // 오브젝트 비활성화
    }
}
