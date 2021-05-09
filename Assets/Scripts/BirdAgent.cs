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
    public AudioSource flappingAudio;
    public AudioSource chirpingAudio;
    public bool printAngles;
    public bool flingAtStart;
    public bool disableWings;
    public bool respawnOnHit;

    private float distance;
    private float bestDistance;
    private bool distanceSet = false;
    private Quaternion startingRot;
    private bool started = false;
    private Transform targetTransform;
    private int targetNumber;

    // Start is called before the first frame update
    void Start()
    {
        startingRot = body.rotation;
        started = true;
        if (Application.isEditor)
        {
            flappingAudio.PlayDelayed(Random.Range(0f, 5f));
            chirpingAudio.PlayDelayed(Random.Range(1f, 20f));
        }
        if (flingAtStart)
        {
            body.GetComponent<Rigidbody>().velocity = new Vector3(0, 10, 20);
        }
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
            if (disableWings)
            {
                liftMultipliers[i].liftMultiplier = 0;
            }
            else
            {
                liftMultipliers[i].liftMultiplier = actionBuffers.DiscreteActions[i];
            }
        }
        float newDistance = GetDistance();
        if (newDistance < 5)
        {
            NextTarget();
        }
        else
        {
            if (distanceSet)
            {
                AddReward((distance - newDistance) * 0.01f); // Scaled to keep the extrinsic value estimate below 1
                if (bestDistance + 1 < newDistance)
                {
                    //AddReward(-0.1f);
                    EndEpisode();
                    GetParentArena().ResetEnv(gameObject);
                }
            }
            else
            {
                distanceSet = true;
                bestDistance = newDistance;
            }
        }
        distance = newDistance;
        if (distance < bestDistance)
        {
            bestDistance = distance;
        }


        /*if (started)
        {
            //print(Quaternion.Angle(body.rotation, startingRot));
            if (Quaternion.Angle(body.rotation, startingRot) > 90)
            {
                AddReward(-0.1f);
                EndEpisode();
                GetParentArena().ResetEnv(gameObject);
            }
        }*/

        if (body.position.y < 0 || body.position.y > 50 || Mathf.Abs(body.position.x) > 100f || Mathf.Abs(body.position.z) > 100f)
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
        //sensor.AddObservation(body.rotation);
        sensor.AddObservation(Quaternion.LookRotation(targetTransform.position-transform.position));
        sensor.AddObservation(RescaleValue(body.position.y, 0, 50, true));
        sensor.AddObservation(RescaleValue(body.position.y, 0, 5, true));

        //print("speed");
        Vector3 speed = body.InverseTransformDirection(body.GetComponent<Rigidbody>().velocity) / 10f;
        sensor.AddObservation(RescaleValue(speed.x, 0, 1, true));
        sensor.AddObservation(RescaleValue(speed.y, 0, 1, true));
        sensor.AddObservation(RescaleValue(speed.z, 0, 1, true));

        //print("angularSpeed");
        Vector3 angularSpeed = body.GetComponent<Rigidbody>().angularVelocity;
        sensor.AddObservation(RescaleValue(angularSpeed.x,0,5,true));
        sensor.AddObservation(RescaleValue(angularSpeed.y,0,5,true));
        sensor.AddObservation(RescaleValue(angularSpeed.z,0,5,true));
    }

    // minValue can be the middle value if you want to rescale from -1 to 1
    private float RescaleValue(float value, float minValue, float maxValue, bool useAtan)
    {
        float val = (value - minValue) / (maxValue - minValue);
        //print(val);
        if (useAtan)
        {
            return Mathf.Atan(val) / (Mathf.PI/2f);
        }
        return val;
    }

    public void childCollision(Collision collision)
    {
        if (respawnOnHit)
        {
            AddReward(-0.1f);
            EndEpisode();
            GetParentArena().ResetEnv(gameObject);
        }
    }

    public float GetDistance()
    {
        //return body.position.z;
        //print((targetTransform.position - body.position).magnitude);
        return (targetTransform.position - body.position).magnitude;
    }

    public ArenaController GetParentArena()
    {
        return transform.parent.GetComponent<ArenaController>();
    }

    public void SetTarget(int localTargetNumber)
    {
        localTargetNumber = (localTargetNumber + 1) % GetParentArena().spawnPoints.Count;
        targetNumber = localTargetNumber;
        targetTransform = GetParentArena().spawnPoints[localTargetNumber];
    }

    public void NextTarget()
    {
        SetTarget(targetNumber + 1);
    }
}
