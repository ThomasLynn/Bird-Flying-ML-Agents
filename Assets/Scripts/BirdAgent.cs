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
                AddReward(-0.1f);
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
        /*print("rotation");
        float x = body.rotation.eulerAngles.x;
        if (x > 180f)
        {
            x -= 360f;
        }
        sensor.AddObservation(RescaleValue(x, 0, 90, true));

        float y = body.rotation.eulerAngles.y;
        if (y > 180f)
        {
            y -= 360f;
        }
        sensor.AddObservation(RescaleValue(y, 0, 360, true));

        float z = body.rotation.eulerAngles.z;
        print("z " + z);
        if (z > 180f)
        {
            z -= 360f;
        }
        sensor.AddObservation(RescaleValue(z, 0, 90, true));*/
        sensor.AddObservation(body.rotation);
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
        AddReward(-0.1f);
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
