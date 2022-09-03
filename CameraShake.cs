using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;
    private Vector3 _startPos;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        _startPos = transform.position;
    }

    IEnumerator ShakeCoroutine()
    {
        _startPos = transform.position;
        for (int i = 0; i < 5; i++)
        {
            transform.position = new Vector3(_startPos.x + Random.Range(-0.15f, 0.15f), _startPos.y + Random.Range(-0.15f, 0.15f), transform.position.z);
            yield return new WaitForSeconds(0.05f);
        }
        transform.position = _startPos;
    }

    public void Shake()
    {
        StartCoroutine(ShakeCoroutine());
    }

}