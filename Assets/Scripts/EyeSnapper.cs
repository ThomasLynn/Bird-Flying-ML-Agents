using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeSnapper : MonoBehaviour
{

    public Transform trackedTransform;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = trackedTransform.position;
        Quaternion quat = transform.rotation;
        Vector3 angles = quat.eulerAngles;
        angles.y = (Quaternion.AngleAxis(-90,trackedTransform.right) * trackedTransform.rotation).eulerAngles.y;
        quat.eulerAngles = angles;
        transform.rotation = quat;
    }
}
