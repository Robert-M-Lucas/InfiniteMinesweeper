using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class TapManager : MonoBehaviour
{
    private float[] _timeTouchBegan;
    private bool[] _touchDidMove;
    private float _tapTimeThreshold = 1f;

    [SerializeField] private GameMenuController gameMenuController;

    [SerializeField] private GameManagerScript gameManager;

    void Start()
    {
        _timeTouchBegan = new float[10];
        _touchDidMove = new bool[10];
    }

    private bool IsPointerOverUIObject()
    {
        PointerEventData event_data_current_position = new PointerEventData(EventSystem.current);
        event_data_current_position.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(event_data_current_position, results);
        return results.Count > 0;
    }

    private void Update()
    {
        // Touches
        foreach (Touch touch in Input.touches)
        {
            int finger_index = touch.fingerId;

            if (touch.phase == TouchPhase.Began)
            {
                _timeTouchBegan[finger_index] = Time.time;
                _touchDidMove[finger_index] = false;

                if (finger_index == 0)
                {
                    gameManager.OnTapDown(touch.position);
                }
            }
            if (touch.phase == TouchPhase.Moved)
            {
                _touchDidMove[finger_index] = true;
            }
            if (touch.phase == TouchPhase.Ended)
            {
                if (finger_index == 0)
                {
                    gameManager.OnTapUp();
                }
                float tapTime = Time.time - _timeTouchBegan[finger_index];
                if (tapTime <= _tapTimeThreshold && _touchDidMove[finger_index] == false && IsPointerOverUIObject() == false && !gameMenuController.paused)
                {
                    gameManager.OnTap(touch.position);
                }
            }
        }
    }
}