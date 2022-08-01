using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;


public class CameraControlScript : MonoBehaviour
{
    private Vector3 touchStart;
    [SerializeField] private float zoomOutMin = 1;
    [SerializeField] private float zoomOutMax = 8;

    private Vector2Int shownPos = new Vector2Int(0, 0);

    [SerializeField] private InputField xText;
    [SerializeField] private InputField yText;

    [SerializeField] private GameManagerScript gameManager;

    // Update is called once per frame
    void Update()
    {
        if (Vector2Int.FloorToInt((Vector2)Camera.main.transform.position) != shownPos)
        {
            shownPos = Vector2Int.FloorToInt((Vector2)Camera.main.transform.position);
            xText.text = shownPos.x.ToString();
            yText.text = shownPos.y.ToString();
        }

        if (Input.GetMouseButtonDown(0))
        {
            touchStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

            float difference = currentMagnitude - prevMagnitude;

            Zoom(difference * 0.01f);
        }
        else if (Input.GetMouseButton(0))
        {
            Vector3 direction = touchStart - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Camera.main.transform.position += direction;
        }
#if UNITY_EDITOR
        Zoom(Input.GetAxis("Mouse ScrollWheel") * 4);
#endif
    }

    public void Jump()
    {
        try
        {
            Camera.main.transform.position = new Vector3(int.Parse(xText.text), int.Parse(yText.text), Camera.main.transform.position.z);
        }
        catch (FormatException)
        {

        } 
    }

    void Zoom(float increment)
    {
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - increment, zoomOutMin, zoomOutMax);
    }
}
