using UnityEngine;

public class RandomScale : MonoBehaviour
{
    public float MinScale = 1f;
    public float MaxScale = 1f;
    private Transform myTransform;
    private Vector3 OriginalTransformScale;
    private float OriginalPSScale = 1f;

    void Awake()
    {
        myTransform = GetComponent<Transform>();
        OriginalTransformScale = myTransform.localScale;
    }

    void OnEnable()
    {
        float tmpScale = Random.Range(MinScale, MaxScale);
        Vector3 newTransformScale = OriginalTransformScale * tmpScale;
        newTransformScale.z = 1f;
        myTransform.localScale = newTransformScale;
    }
}
