using UnityEngine;
using System.Collections;
using RoadArchitect;
using System.Collections.Generic;
using System.Linq;

namespace RVP
{
    [RequireComponent(typeof(VehicleParent))]
    [DisallowMultipleComponent]
    [AddComponentMenu("RVP/AI/Road AI", 0)]

    // Class for following AI
    public class RoadAI : MonoBehaviour
    {
        Transform tr;
        public FollowAI ai;
        public Transform startpoint;
        HashSet<Road> roads;
        Road curRoad;
        RoadIntersection atIntersection;
        bool otherCarDetected;

        float stopTimer;
        public float minStopTime = 10f;
        public int turnChance = 100;

        SplineN firstNode;
        Road nextRoad;
        Road prevRoad;
        Road lastEvaluated;
        bool doingTurnProtocol = false;
        bool decided = false;

        void Start()
        {
            tr = transform;
            tr.position = startpoint.position;
            tr.rotation = startpoint.rotation;
            curRoad = startpoint.parent.parent.gameObject.GetComponent<Road>();
            lastEvaluated = curRoad;
            prevRoad = curRoad;
            tr.Translate((curRoad.laneAmount / 2 - 1) * curRoad.laneWidth + curRoad.laneWidth / 2, 0, 0);
        }

        //void FixedUpdate()
        //{
        //    roads = new();
        //    roadChanged = false;
        //    atIntersection = null;
        //    if (changeTimer > 0)
        //        changeTimer -= Time.deltaTime;

        //    Collider[] hitColliders = Physics.OverlapSphere(tr.position, 8f);
        //    foreach (var hitCollider in hitColliders)
        //    {
        //        Transform parent = hitCollider.gameObject.transform.parent;
        //        if (parent.name.StartsWith("Road"))
        //        {
        //            Transform roadtransform = parent.parent.parent;
        //            roads.Add(roadtransform.gameObject.GetComponent<Road>());
        //        }
        //        if (parent.name.StartsWith("Inter"))
        //        {
        //            atIntersection = parent.gameObject.GetComponent<RoadIntersection>();
        //        }
        //        if (hitCollider.gameObject.name == "body" && Vector2.Angle(hitCollider.transform.position - tr.position, tr.forward) < 90)
        //        {
        //            otherCarDetected = true;
        //        }
        //    }

        //    bool shouldStop = false;

        //    if (roads.Count > 1)
        //    {
        //        if (Vector2.Distance(tr.position, curRoad.spline.nodes[0].transform.position) < 5f ||
        //            Vector2.Distance(tr.position, curRoad.spline.nodes[curRoad.spline.nodes.Count - 1].transform.position) < 5f)
        //        {
        //            turnChance = 100;
        //        }
        //        foreach (Road road in roads)
        //        {
        //            if (road == curRoad) { continue; }

        //            float distanceToRoad = Vector2.Distance(tr.position, closestN(road).transform.position);
        //            if (road != prevRoad && changeTimer <= 0 && distanceToRoad < road.laneAmount * (road.laneWidth - 1) / 2)
        //            {
        //                int random = Random.Range(0, 100);
        //                Debug.Log(random);
        //                changeTimer = changeCooldown;
        //                if (random <= turnChance)
        //                {
        //                    prevRoad = curRoad;
        //                    Debug.Log("Prev Road: " + prevRoad);
        //                    curRoad = road;
        //                    roadChanged = true;
        //                    changeTimer = changeCooldown;
        //                    Debug.Log("Decided to change road to " + road.name + "With roll " + random);
        //                    break;
        //                }
        //            }
        //            else if (distanceToRoad < (road.laneWidth) * (1 + road.laneAmount / 2))
        //            {
        //                shouldStop = true;
        //            }
        //        }


        //        if (shouldStop || atIntersection != null && Vector2.Distance(atIntersection.transform.position, tr.position) < (curRoad.laneWidth) * (1 + curRoad.laneAmount / 2) || stopTimer != 0)
        //        {
        //            Debug.Log("Trying To stop");
        //            if (stopTimer < minStopTime || otherCarDetected)
        //            {
        //                Debug.Log("Stopping");
        //                ai.hasToStop(true);
        //            }
        //            else
        //            {
        //                ai.hasToStop(false);
        //            }
        //            stopTimer += Time.deltaTime;
        //        }
        //    }
        //    else { stopTimer = 0; }


        //    Transform nodetransf = nextNode(curRoad);

        //    Transform transf;
        //    if (nodetransf.GetComponentInChildren<VehicleWaypoint>() == null)
        //    {
        //        transf = CreateWaypoint(nodetransf);
        //    }
        //    else
        //    {
        //        transf = nodetransf.GetChild(0);
        //    }

        //    if (ai.targetPrev != null && ai.targetPrev != transf)
        //    {  
        //        Destroy(ai.targetPrev.gameObject);
        //    }

        //    ai.target = transf;
        //}

        void FixedUpdate()
        {
            otherCarDetected = false;
            atIntersection = null;
            roads = new();

            Collider[] hitColliders = Physics.OverlapSphere(tr.position, 20f);
            foreach (var hitCollider in hitColliders)
            {
                Transform parent = hitCollider.gameObject.transform.parent;
                if (parent.name.StartsWith("Road") && !parent.name.StartsWith("RoadSign"))
                {
                    Transform roadtransform = parent.parent.parent;
                    roads.Add(roadtransform.gameObject.GetComponent<Road>());
                }
                if (parent.name.StartsWith("Inter"))
                {
                    atIntersection = parent.gameObject.GetComponent<RoadIntersection>();
                }
                if (hitCollider.gameObject.name == "body" && Vector2.Angle(hitCollider.transform.position - tr.position, tr.forward) < 90)
                {
                    otherCarDetected = true;
                }
            }

            if (roads.Count > 1 && !doingTurnProtocol)
            {
                float scalingfactor = Mathf.Clamp(ai.speed * 2 / 3, 0.5f, 2);
                if (atIntersection == null)
                {
                    float mindist = Mathf.Infinity;
                    foreach (Road road in roads)
                    {
                        if (road == curRoad) { continue; }

                        SplineN n = closestNodeInFront(road);
                        if (n == null) { continue; }

                        float dist = Vector2.Distance(n.pos, tr.position);
                        if (road != lastEvaluated && road != prevRoad && dist < mindist && dist < road.laneWidth * scalingfactor * road.laneAmount )
                        {
                            doingTurnProtocol = true;
                            mindist = dist;
                            nextRoad = road;
                            firstNode = n;
                        }
                    }
                }
                else
                {
                    float dist = Vector2.Distance(atIntersection.node1.pos, tr.position);
                    Road road = atIntersection.node1.spline.road;
                    if (road == curRoad) { road = atIntersection.node2.spline.road; }

                    if (road != lastEvaluated && road != prevRoad && dist < road.laneWidth * scalingfactor * road.laneAmount )
                    {
                        doingTurnProtocol = true;
                        nextRoad = road;
                        firstNode = closestNodeInFront(road);
                    }
                }
            }

            Transform nodetransf;
            if (doingTurnProtocol)
            {
                int random = 100;
                nodetransf = ai.targetPrev;

                if (!decided) 
                {
                    Vector2 trpos = new Vector2(tr.position.x, tr.position.z);
                    Vector2 firstnodepos = new Vector2(curRoad.spline.nodes[0].transform.position.x, curRoad.spline.nodes[0].transform.position.z);
                    Vector2 lastnodepos = new Vector2(curRoad.spline.nodes[curRoad.spline.nodes.Count - 1].transform.position.x, curRoad.spline.nodes[curRoad.spline.nodes.Count - 1].transform.position.z);


                    if (Vector2.Distance(trpos, firstnodepos) < curRoad.laneWidth * curRoad.laneAmount * 1.5f ||
                        Vector2.Distance(trpos, lastnodepos) < curRoad.laneWidth * curRoad.laneAmount * 1.5f)
                    {
                        turnChance = 100;
                    }

                    random = Random.Range(0, 100); 
                    decided = true; 

                    if (random <= turnChance)
                    {
                        prevRoad = curRoad;
                    }


                    if (stopTimer < minStopTime || otherCarDetected)
                    {
                    }
                }

                if (stopTimer < minStopTime || otherCarDetected)
                {
                    ai.hasToStop(true);
                    stopTimer += Time.deltaTime;
                }
                else
                {
                    ai.hasToStop(false);

                    if (random <= turnChance)
                    {
                        curRoad = nextRoad;
                        nodetransf = determineTurnNode(firstNode).transform;
                    }
                    lastEvaluated = nextRoad;

                    Transform waypoint = null;
                    if (nodetransf.GetComponentInChildren<VehicleWaypoint>() != null)
                        waypoint = nodetransf.GetComponentInChildren<VehicleWaypoint>().transform;
                    
                    if (waypoint != null)
                    {
                        float dist = Vector2.Distance(new Vector2(tr.position.x, tr.position.z), new Vector2(waypoint.position.x, waypoint.position.z));
                        if (dist < 1f)
                        {
                            decided = false;
                            stopTimer = 0;
                            doingTurnProtocol = false;
                        }
                    }
                }
            }
            else
            {
                nodetransf = nextNode(curRoad);
                decided = false;
                stopTimer = 0;
            }



            Transform transf;
            if (nodetransf.GetComponentInChildren<VehicleWaypoint>() == null)
            {
                transf = CreateWaypoint(nodetransf);
            }
            else
            {
                transf = nodetransf.GetComponentInChildren<VehicleWaypoint>().gameObject.transform;
            }

            if (ai.targetPrev != null && ai.targetPrev != transf)
            {
                Destroy(ai.targetPrev.gameObject);
            }

            ai.target = transf;
        }


        private Transform CreateWaypoint(Transform nodetransf)
        {
            GameObject target = new GameObject();
            target.name = "Target";
            target.AddComponent<VehicleWaypoint>();
            target.transform.parent = nodetransf;

            VehicleWaypoint waypoint = target.GetComponent<VehicleWaypoint>();
            waypoint.speed = (float)curRoad.roadSpeedLimit / 100;
            waypoint.radius = 8f;

            if (Vector2.Angle(new Vector2(nodetransf.forward.x, nodetransf.forward.z), new Vector2((nodetransf.position - tr.position).x, (nodetransf.position - tr.position).z)) < 90) { target.transform.localPosition = new Vector3((curRoad.laneAmount / 2 - 1) * curRoad.laneWidth + curRoad.laneWidth / 2, 0, 0); }
            else { target.transform.localPosition = new Vector3(-((curRoad.laneAmount / 2 - 1) * curRoad.laneWidth + curRoad.laneWidth / 2), 0, 0); }
            
            return target.transform;
        }


        private bool satisfiesTurnNodeCondition(SplineN node)
        {
            Vector2 nodePos = new Vector2(node.pos.x, node.pos.z);
            Vector2 trPos = new Vector2(tr.position.x, tr.position.z);

            float angle = Vector2.Angle(nodePos - trPos, new Vector2(tr.forward.x, tr.forward.z));

            if (Vector2.Distance(nodePos, trPos) > 5f && 90 > angle && angle > 30)
            {
                return true;
            }
            return false;
        }

        private SplineN determineTurnNode(SplineN closestNode)
        {
            if (closestNode == closestNode.spline.nodes[0])
            {
                for (int idx = 1; idx < closestNode.spline.nodes.Count; idx++)
                {
                    if (satisfiesTurnNodeCondition(closestNode.spline.nodes[idx]))
                        return closestNode.spline.nodes[idx];
                }
                return closestNode.spline.nodes[1];
            }

            else if (closestNode == closestNode.spline.nodes[closestNode.spline.nodes.Count - 1])
            {
                for (int idx = 1; idx < closestNode.spline.nodes.Count; idx++)
                {
                    if (satisfiesTurnNodeCondition(closestNode.spline.nodes[closestNode.spline.nodes.Count - 1 - idx]))
                        return closestNode.spline.nodes[closestNode.spline.nodes.Count - 1 - idx];
                }
                return closestNode.spline.nodes[closestNode.spline.nodes.Count - 1];
            }

            else
            {
                int idx = closestNode.spline.nodes.IndexOf(closestNode);
                int exp = Random.Range(0, 1);
                for (int i = 0; i < Mathf.Min(idx, closestNode.spline.nodes.Count - idx - 1); i++)
                {
                    if (satisfiesTurnNodeCondition(closestNode.spline.nodes[idx + (int)Mathf.Pow(-i, exp)]))
                    {
                        return closestNode.spline.nodes[idx + (int)Mathf.Pow(-i, exp)];
                    }
                }
                return closestNode.spline.nodes[idx + (int)Mathf.Pow(-1, exp)];
            }
        }


        private SplineN closestN(Road road)
        {
            if (road == null) { return null; }
            SplineN closestNode = null;
            float mindist = Mathf.Infinity;
            float dist;

            foreach (SplineN node in road.spline.nodes)
            {
                dist = Vector3.Distance(node.transform.position, tr.position);
                if (dist < mindist)
                {
                    closestNode = node;
                    mindist = dist;
                }
            }

            return closestNode;
        }

        private SplineN closestNodeInFront(Road road)
        {
            if (road == null) { return null; }
            SplineN closestNode = null;
            float mindist = Mathf.Infinity;
            float dist;

            foreach (SplineN node in road.spline.nodes)
            {
                float angle = Vector2.Angle(new Vector2((node.transform.position - tr.position).x, (node.transform.position - tr.position).z), new Vector2( tr.forward.x, tr.forward.z ));
                if (angle > 90) 
                {
                    continue; 
                }
                dist = Vector2.Distance(new Vector2(node.transform.position.x, node.transform.position.z), new Vector2(tr.position.x, tr.position.z));
                if (dist < mindist)
                {
                    closestNode = node;
                    mindist = dist;
                }
            }

            return closestNode;
        }

        private Transform nextNode(Road road)
        {
            if (road == null) { return null; }

            SplineN closestNode = closestN(road);
            int idx = road.spline.nodes.IndexOf(closestNode);

            SplineN prevNode = closestNode;
            if ( closestNode != road.spline.nodes[0] )
            {
                prevNode = road.spline.nodes[idx - 1];
            }
            SplineN nextNode = closestNode;
            if ( closestNode != road.spline.nodes[road.spline.nodes.Count - 1] ) 
            {
                nextNode = road.spline.nodes[idx + 1];
            }

            Vector2 directionToNext = new Vector2(nextNode.transform.position.x - tr.position.x, nextNode.transform.position.z - tr.transform.position.z);
            Vector2 directionToClosest = new Vector2(closestNode.transform.position.x - tr.position.x, closestNode.transform.position.z - tr.position.z);
            float angle = Vector2.Angle(new Vector2(tr.forward.x, tr.forward.z), directionToNext);
            float closestangle = Vector2.Angle(new Vector2(tr.forward.x, tr.forward.z), directionToClosest);

            Debug.DrawRay(tr.position, tr.forward, Color.blue);
            Debug.DrawRay(tr.position, directionToNext.normalized, Color.green);
            Debug.DrawRay(tr.position, directionToClosest.normalized, Color.red);

            if (Vector2.Distance(closestNode.pos, tr.position) > 1f && Mathf.Abs(closestangle) < 90)
                return closestNode.transform;
            else if ( Mathf.Abs(angle) < 90 )
                return nextNode.transform;
            else
                return prevNode.transform;
        }
    }
}
