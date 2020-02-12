using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Aircraft
{
    public class AircraftArea : MonoBehaviour
    {
        [Tooltip("The path the take will take")]
        public CinemachineSmoothPath racePath;

        [Tooltip("The prefab to use for checkpoints")]
        public GameObject checkPointPrefab;

        [Tooltip("The prefab to use for the start/end checkpoint")]
        public GameObject finishCheckPointPrefab;

        [Tooltip("If true enable training mode")]
        public bool trainingMode;

        public List<AircraftAgent> AircraftAgents { get; private set; }
        public List<GameObject> Checkpoints { get; private set; }
        public AircraftAcademy AircraftAcademy { get; private set; }

        /// <summary>
        /// Actions to perform when the script wakes up
        /// </summary>
        private void Awake()
        {
            // Find all AircraftAgents in the area  

            AircraftAgents = transform.GetComponentsInChildren<AircraftAgent>().ToList();
            Debug.Assert(AircraftAgents.Count > 0, "No AircraftAgents found");

            AircraftAcademy = FindObjectOfType<AircraftAcademy>();
        }

        /// <summary>
        /// Setup the area
        /// </summary>
        private void Start()
        {
            // Create checkpoints along the race path
            Debug.Assert(racePath != null, "Race path was not set");
            Checkpoints = new List<GameObject>();
            int numCheckpoints = (int)racePath.MaxUnit(CinemachinePathBase.PositionUnits.PathUnits);

            for(int i = 0; i < numCheckpoints; i++)
            {
                // Instantiate either checkpoint or finish line checkpoint
                GameObject checkPoint;
                if(i == numCheckpoints - 1)
                {
                    checkPoint = Instantiate<GameObject>(finishCheckPointPrefab);
                }
                else
                {
                    checkPoint = Instantiate<GameObject>(checkPointPrefab);
                }

                // Set the parent, position and rotation
                checkPoint.transform.SetParent(racePath.transform);
                checkPoint.transform.localPosition = racePath.m_Waypoints[i].position;
                checkPoint.transform.rotation = racePath.EvaluateOrientationAtUnit(i, CinemachinePathBase.PositionUnits.PathUnits);

                // Add checkpoint to list
                Checkpoints.Add(checkPoint);
            }
        }

        /// <summary>
        /// Resets the position of an agent using it's current current NextCheckPointIndex
        /// unless randomize is true, then will pick a new random checkpoint
        /// </summary>
        /// <param name="agent">The agent to reset</param>
        /// <param name="randomize">If true, will choose a NextCheckPointIndex before reset</param>
        public void ResetAgentPosition(AircraftAgent agent, bool randomize = false)
        {
            if(randomize)
            {
                // Pick a new NextCheckPointIndex at random
                agent.NextCheckpointIndex = Random.Range(0, Checkpoints.Count);
            }

            // set start position to the previous checkpoint
            int previousCheckPointIndex = agent.NextCheckpointIndex - 1;
            if(previousCheckPointIndex == -1)
            {
                previousCheckPointIndex = Checkpoints.Count - 1;
            }

            float startPosition = racePath.FromPathNativeUnits(previousCheckPointIndex, CinemachinePathBase.PositionUnits.PathUnits);

            // Convert the position on the race path to a position in 3d space
            Vector3 basePosition = racePath.EvaluatePosition(startPosition);

            // Get the orientation at that position on the race path
            Quaternion orientation = racePath.EvaluateOrientation(startPosition);

            // Calculate horizontal offset so that agents are spread out
            Vector3 positionOffset = Vector3.right * (AircraftAgents.IndexOf(agent) - AircraftAgents.Count / 2f)
                * UnityEngine.Random.Range(8f, 10f);

            // set the aircraft position and rotation
            agent.transform.position = basePosition + orientation * positionOffset;
            agent.transform.rotation = orientation;
        }
    }
}


