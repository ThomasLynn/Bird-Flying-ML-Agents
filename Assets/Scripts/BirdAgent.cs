using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class BirdAgent : Unity.MLAgents.Agent
{

    public Transform body;
    public List<ConfigurableJoint> joints;
    public List<SimpleWing> liftMultipliers;
    public bool printAngles;

    private ArenaController parentArena;
    private float distance;
    private bool distanceSet = false;
    private Quaternion startingRot;
    private bool started = false;

    // Start is called before the first frame update
    void Start()
    {
        parentArena = transform.parent.GetComponent<ArenaController>();
        startingRot = body.rotation;
        started = true;
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        for (int i = 0; i < joints.Count; i++)
        {

            //JointSpring hingeSpring = joints[i].spring;
            //hingeSpring.targetPosition = actionBuffers.ContinuousActions[i] * 45;
            //joints[i].spring = hingeSpring;
            Quaternion targetAngle = Quaternion.Euler(Mathf.Clamp(actionBuffers.ContinuousActions[i * 3],-1,1) * 45, Mathf.Clamp(actionBuffers.ContinuousActions[i * 3 + 1], -1, 1) * 45, Mathf.Clamp(actionBuffers.ContinuousActions[i * 3 +2], -1, 1) * 45);
            joints[i].targetRotation = targetAngle;
        }
        for(int i=0;i < liftMultipliers.Count; i++)
        {
            liftMultipliers[i].liftMultiplier = actionBuffers.DiscreteActions[i];
        }
        float newDistance = GetDistance();
        if (distanceSet)
        {
            AddReward((newDistance - distance) * 0.01f); // Scaled to keep the extrinsic value estimate below 1
        }
        else
        {
            distanceSet = true;
        }
        
        distance = newDistance;

        if (started)
        {
            //print(Quaternion.Angle(body.rotation, startingRot));
            if (Quaternion.Angle(body.rotation, startingRot) > 90)
            {
                AddReward(-0.01f);
                EndEpisode();
                GetParentArena().ResetEnv(gameObject);
            }
        }

        if (body.position.y < 0 || Mathf.Abs(body.position.x) > 100f || Mathf.Abs(body.position.z) > 100f)
        {
            EndEpisode();
            GetParentArena().ResetEnv(gameObject);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        if (Input.GetKey(KeyCode.W))
        {
            for (int i = 0; i < actionsOut.ContinuousActions.Array.Length; i++)
            {
                //actionsOut.DiscreteActions.Array[i] = 1;
                actionsOut.ContinuousActions.Array[i] = 1.0f;
            }
        }
        else if (Input.GetKey(KeyCode.S))
        {
            for (int i = 0; i < actionsOut.ContinuousActions.Array.Length; i++)
            {
                //actionsOut.DiscreteActions.Array[i] = -1;
                actionsOut.ContinuousActions.Array[i] = -1f;
            }
        }
        else
        {
            for (int i = 0; i < actionsOut.ContinuousActions.Array.Length; i++)
            {
                //actionsOut.DiscreteActions.Array[i] = 0;
                actionsOut.ContinuousActions.Array[i] = 0f;
            }
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        float x = body.rotation.eulerAngles.x;
        if (x > 180f)
        {
            x -= 360f;
        }
        x = Mathf.Atan(x / 90f);
        //Debug.Log("x " + x + " " + body.rotation.eulerAngles.x);
        sensor.AddObservation(x);

        float y = body.rotation.eulerAngles.y;
        if (y > 180f)
        {
            y -= 360f;
        }
        y = Mathf.Atan(y / 360f);
        //Debug.Log("y " + y + " " + body.rotation.eulerAngles.y);
        sensor.AddObservation(y);

        float z = body.rotation.eulerAngles.z;
        if (z > 180f)
        {
            z -= 360f;
        }
        z = Mathf.Atan(z / 90f);
        //Debug.Log("z " + z + " " + body.rotation.eulerAngles.z);
        sensor.AddObservation(z);

        Vector3 speed = body.InverseTransformDirection(body.GetComponent<Rigidbody>().velocity) / 10f;
        speed.x = Mathf.Atan(speed.x);
        speed.y = Mathf.Atan(speed.y);
        speed.z = Mathf.Atan(speed.z);
        //print(speed);
        sensor.AddObservation(speed);
        Vector3 angularSpeed = body.InverseTransformDirection(body.GetComponent<Rigidbody>().angularVelocity);
        angularSpeed.x = Mathf.Atan(angularSpeed.x);
        angularSpeed.y = Mathf.Atan(angularSpeed.y);
        angularSpeed.z = Mathf.Atan(angularSpeed.z);
        //print(angularSpeed);
        sensor.AddObservation(angularSpeed);
    }

    public void childCollision(Collision collision)
    {
        AddReward(-0.01f);
        EndEpisode();
        GetParentArena().ResetEnv(gameObject);
    }

    public float GetDistance()
    {
        return body.position.z;
    }

    public ArenaController GetParentArena()
    {
        return parentArena;
    }
}
