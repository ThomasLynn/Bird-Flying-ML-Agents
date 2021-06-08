using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMoverScript : MonoBehaviour
{

    public List<Vector3> positions;
    public List<Quaternion> rotations;
    public bool follow;

    [HideInInspector]
    public int birdTarget = -1;

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private float moveSpeed = 0;
    private float rotSpeed = 0;

    // Start is called before the first frame update
    void Start()
    {
        targetPosition = transform.position;
        targetRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("a"))
        {
            positions.Add(transform.position);
            rotations.Add(transform.rotation);
        }
        if (Input.GetKeyDown("1"))
        {
            targetPosition = positions[0];
            targetRotation = rotations[0];
            moveSpeed = (transform.position - targetPosition).magnitude;
            rotSpeed = Quaternion.Angle(transform.rotation, targetRotation);
            birdTarget = -1;
        }
        if (Input.GetKeyDown("2"))
        {
            targetPosition = positions[1];
            targetRotation = rotations[1];
            moveSpeed = (transform.position - targetPosition).magnitude;
            rotSpeed = Quaternion.Angle(transform.rotation, targetRotation);
            birdTarget = 1;
        }
        if (Input.GetKeyDown("3"))
        {
            targetPosition = positions[2];
            targetRotation = rotations[2];
            moveSpeed = (transform.position - targetPosition).magnitude;
            rotSpeed = Quaternion.Angle(transform.rotation, targetRotation);
            birdTarget = 1;
        }
        if (Input.GetKeyDown("4"))
        {
            targetPosition = positions[3];
            targetRotation = rotations[3];
            moveSpeed = (transform.position - targetPosition).magnitude;
            rotSpeed = Quaternion.Angle(transform.rotation, targetRotation);
            birdTarget = 0;
        }
        if (Input.GetKeyDown("5"))
        {
            targetPosition = positions[4];
            targetRotation = rotations[4];
            moveSpeed = (transform.position - targetPosition).magnitude;
            rotSpeed = Quaternion.Angle(transform.rotation, targetRotation);
            birdTarget = 2;
        }
        if (follow) {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * 2 * Time.deltaTime);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotSpeed * 2 * Time.deltaTime);
        }
    }
}
