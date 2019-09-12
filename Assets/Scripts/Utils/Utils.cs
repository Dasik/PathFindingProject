using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Random = System.Random;

public class Utils
{
    [System.Serializable]
    public class UnityEventVector2 : UnityEvent<Vector2>
    {
    }

    [System.Serializable]
    public class UnityEvent2Vector2 : UnityEvent<Vector2,Vector2>
    {
    }

    private static Random rand = new Random();
    /// <summary>
    /// return true if vectors in same position
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <param name="maxDelta"></param>
    /// <returns></returns>
    public static bool VectorsAreSame(Vector2 v1, Vector2 v2, float maxDelta)
    {
        var delta = v1 - v2;
        //if (Mathf.Abs(delta.x) > maxDelta ||
        //    Mathf.Abs(delta.y) > maxDelta)
        //    return false;
        //return true;
        return !(Mathf.Abs(delta.x) > maxDelta) && !(Mathf.Abs(delta.y) > maxDelta);
    }

    //static string[] ColourValues = new string[] {
    //    "FF0000", "00FF00", "0000FF", "FFFF00", "FF00FF", "00FFFF", "000000",
    //    "800000", "008000", "000080", "808000", "800080", "008080", "808080",
    //    "C00000", "00C000", "0000C0", "C0C000", "C000C0", "00C0C0", "C0C0C0",
    //    "400000", "004000", "000040", "404000", "400040", "004040", "404040",
    //    "200000", "002000", "000020", "202000", "200020", "002020", "202020",
    //    "600000", "006000", "000060", "606000", "600060", "006060", "606060",
    //    "A00000", "00A000", "0000A0", "A0A000", "A000A0", "00A0A0", "A0A0A0",
    //    "E00000", "00E000", "0000E0", "E0E000", "E000E0", "00E0E0", "E0E0E0",
    //};

    public static Color GetRandomPrettyColor()
    {
        //Color clr=new Color();
        //if (ColorUtility.TryParseHtmlString('#'+ColourValues[rand.Next(ColourValues.Length)], out clr))
        //    return clr;
        return new Color((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble());
    }

    public static IEnumerator DelaySeconds(Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action();
    }

}
