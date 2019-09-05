﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Boid : MonoBehaviour, ICellEntity
{
    public BoidSettings Settings;

    private ICellManager<Boid, AggregatedBoidData> CellManager;
    private IEnumerable<Boid>[] NeighborBoidsLists;
    private AggregatedBoidData AggregatedNeighbors;
    private Boid NearestNeighbor;
    
    

    void Update()
    {
        var cellPosition = ProvideCellPosition();

        UnityEngine.Profiling.Profiler.BeginSample("Neighbor boids");
        NeighborBoidsLists = CellManager.GetNeighbourEntities(cellPosition);
        UnityEngine.Profiling.Profiler.EndSample();


        UnityEngine.Profiling.Profiler.BeginSample("Neighbor aggregations");
        AggregatedNeighbors = CellManager.GetNeighborAggregation(cellPosition);
        UnityEngine.Profiling.Profiler.EndSample();


        UnityEngine.Profiling.Profiler.BeginSample("Nearest neighbor");
        NearestNeighbor = EvaluateNearestNeighbor(NeighborBoidsLists);
        UnityEngine.Profiling.Profiler.EndSample();


        UnityEngine.Profiling.Profiler.BeginSample("Processing data");
        ProcessDirection();
        ProcessSpeed();
        UnityEngine.Profiling.Profiler.EndSample();
    }
    


    public Boid EvaluateNearestNeighbor(IEnumerable<IEnumerable<Boid>> neighborsLists)
    {
        var count = 1;
        Boid nearestBoid = null;
        var nearestDistance = 1000000f;

        foreach(var list in neighborsLists)
        {
            foreach (var neighbor in list)
            {
                if (count > Settings.NearestNeighborLimit)
                    break;
                if (neighbor.Equals(this))
                    continue;

                var distance = (neighbor.transform.position - transform.position).sqrMagnitude;
                if (distance < nearestDistance)
                {
                    nearestBoid = neighbor;
                    nearestDistance = distance;
                }

                count++;
            }

            if (count > Settings.NearestNeighborLimit)
                break;
        }


        return nearestBoid;
    }

    public void ProcessDirection()
    {
        var newDirection = transform.forward;
        
        // Target
        newDirection += (Settings.Target - transform.position).normalized * Settings.TargetWeight;

        if (NearestNeighbor != null)
        {
            var nearestNeighborDiff = (transform.position - NearestNeighbor.transform.position);
            var centerDiff = AggregatedNeighbors.Position - transform.position;
            var centerDiffNormalized = centerDiff.normalized;
            
            
            // Seperation
            newDirection += Settings.SeperationWeight * nearestNeighborDiff.normalized / Mathf.Max(0.001f, nearestNeighborDiff.magnitude);

            // Alignment
            newDirection += Settings.AlignmentWeight * AggregatedNeighbors.Direction / Mathf.Max(0.001f, centerDiff.magnitude);

            // Cohesion
            newDirection += Settings.CohesionWeight * centerDiffNormalized;
        }
        
        var foo = Vector3.Lerp(transform.forward, newDirection.normalized, Settings.DirectionReactionSpeed);
        transform.forward = foo;
    }
    
    public void ProcessSpeed()
    {
        transform.position += Settings.Speed * Time.deltaTime * transform.forward;
    }
    
    public void SetCellManager<TCell, TAggregation>(ICellManager<TCell, TAggregation> manager) where TCell : ICellEntity
    {
        CellManager = (ICellManager<Boid, AggregatedBoidData>)manager;
    }
    
    public System.Numerics.Vector3 ProvideCellPosition()
    {
        var p = transform.position;
        return new System.Numerics.Vector3(p.x, p.y, p.z);
    }

    

    private void OnDrawGizmosSelected()
    {
        var neighborLists = CellManager.GetNeighbourEntities(ProvideCellPosition());
        foreach(var list in neighborLists)
        {
            foreach(var neighbor in list)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, neighbor.transform.position);
            }
        }

        var agg = CellManager.GetNeighborAggregation(ProvideCellPosition());
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, agg.Position);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + agg.Direction);
    }
}
