using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchDispatcher : MonoBehaviour {

    public delegate void TouchBeganHandler(Vector2 position, int fingerId);
    public static event TouchBeganHandler TouchBegan;
    private void CallTouchBegan(Vector2 position, int fingerId)
    {
        if (TouchBegan != null)
            TouchBegan(position, fingerId);
    }
    public delegate void TouchMovedHandler(Vector2 position, int fingerId);
    public static event TouchBeganHandler TouchMoved;
    private void CallTouchMoved(Vector2 position, int fingerId)
    {
        if (TouchMoved != null)
            TouchMoved(position, fingerId);
    }
    public delegate void TouchEndedHandler(Vector2 position, int fingerId);
    public static event TouchBeganHandler TouchEnded;
    private void CallTouchEnded(Vector2 position, int fingerId)
    {
        if (TouchEnded != null)
            TouchEnded(position, fingerId);
    }
    public delegate void TouchCanceledHandler(Vector2 position, int fingerId);
    public static event TouchBeganHandler TouchCanceled;
    private void CallTouchCanceled(Vector2 position, int fingerId)
    {
        if (TouchCanceled != null)
            TouchCanceled(position, fingerId);
    }


    bool mouseReleased;

    void Start()
    {
        mouseReleased = true;
    }
    private void Update()
    {
        //		if(targetedHandlers.Count>0){
        MakeDetectionMouseTouch();
        //		}
        //		ClearDelList();
    }

    protected virtual void MakeDetectionMouseTouch()
    {
        //      #if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN
        //MakeDetectionMouse();
        //#else
        //MakeDetectionTouch();
        //#endif
        if (Input.touchSupported)
            MakeDetectionTouch();
        else
            MakeDetectionMouse();
    }

    private Vector2 LastPosition;
    protected virtual void MakeDetectionMouse()
    {

        if (IsPointerOverUIObject(Input.mousePosition))
            return;
            //left mouse button
            if (Input.GetMouseButtonDown(0))
        {
            //мышь не была отжата
            if (!mouseReleased)
            {
                CallTouchCanceled(Input.mousePosition, 1);
            }
            else
            {
                CallTouchBegan(Input.mousePosition, 1);

                mouseReleased = false;

            }
        }
        //зажатый компонент
        if (Input.GetMouseButton(0))
        {
            //			LastPosition-=new Vector2(Input.mousePosition.x,Input.mousePosition.y);
            CallTouchMoved(new Vector2(Input.mousePosition.x, Input.mousePosition.y), 1);
            //			CallTouchMoved(LastPosition-Input.mousePosition,1);
        }
        //released button
        if (Input.GetMouseButtonUp(0))
        {
            mouseReleased = true;
            CallTouchEnded(Input.mousePosition, 1);
        }
        LastPosition = Input.mousePosition;
    }

    protected virtual void MakeDetectionTouch()
    {
        int count = Input.touchCount;
        Touch touch;
        for (int i = 0; i < count; i++)
        {
            touch = Input.GetTouch(i);
            if (IsPointerOverUIObject(touch.position))
                continue;
            switch (touch.phase)
            {
                case TouchPhase.Began: CallTouchBegan(touch.position, touch.fingerId); break;
                case TouchPhase.Moved: CallTouchMoved(touch.position, touch.fingerId); break;
                case TouchPhase.Ended: CallTouchEnded(touch.position, touch.fingerId); break;
                case TouchPhase.Canceled: CallTouchCanceled(touch.position, touch.fingerId); break;
            }
        }
    }

    private bool IsPointerOverUIObject(Vector2 position)
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = position;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
}