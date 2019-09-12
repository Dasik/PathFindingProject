using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class RectTransformExt
{
    static public Vector2 GetWorldCenter(this RectTransform rt)
    {
        //Vector2 rectCenter = _rectTransform.TransformPoint(0f, 0f, 0f);
        // Convert the rectangle to world corners and grab the top left
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        Vector2 rectCenter = Vector2.zero;
        foreach (var item in corners)
        {
            rectCenter.x += item.x;
            rectCenter.y += item.y;
        }

        rectCenter /= 4f;
        
        // Rescale the size appropriately based on the current Canvas scale
        Vector2 scaledRectCentere = new Vector2(rectCenter.x,rectCenter.y);

        return scaledRectCentere;
    }

    public static Vector2 GetCenterInScreenSpace(this RectTransform transform)
    {
        Vector3[] corners = new Vector3[4];
        transform.GetWorldCorners(corners);
        //Debug.Log("Screen point1: " + new Vector3(transform.rect.xMax, transform.rect.yMin, 0) + transform.position);
        //foreach (Vector3 corner in corners)
        //{
        //    Debug.Log("World point: " + corner);
        //    Debug.Log("Screen point: " + RectTransformUtility.WorldToScreenPoint(null, corner));
        //    Debug.Log("Viewport: " + Camera.main.ScreenToViewportPoint(corner));
        //}
        Vector2 rectCenter = Vector2.zero;
        foreach (var item in corners)
        {
            rectCenter.x += item.x;
            rectCenter.y += item.y;
        }

        return rectCenter/4f;

    }
}
