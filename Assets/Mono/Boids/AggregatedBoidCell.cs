﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class AggregatedBoidCell : ICellAggregation<AggregatedBoidCell, Boid>
{
    public int Count { get; set; }
    public Dictionary<int, CellData> BoidTypeData;


    /// <summary>
    /// 
    /// </summary>
    public AggregatedBoidCell()
    {
        BoidTypeData = new Dictionary<int, CellData>();
        Clear();
    }
    

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public AggregatedBoidCell Add(Boid entity)
    {
        Count++;

        var level = entity.Settings.GetInstanceID();
        if (BoidTypeData.ContainsKey(level))
            BoidTypeData[level] = BoidTypeData[level].Add(entity);
        else
            BoidTypeData.Add(level, new CellData().Add(entity));
        
        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public AggregatedBoidCell Combine(AggregatedBoidCell other)
    {
        Count += other.Count;

        foreach(var d in other.BoidTypeData)
        {
            if (BoidTypeData.ContainsKey(d.Key))
                BoidTypeData[d.Key] = BoidTypeData[d.Key].Combine(d.Value);
            else
                BoidTypeData.Add(d.Key, new CellData().Combine(d.Value));
        }

        return this;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public AggregatedBoidCell Finialize()
    {
        foreach (var k in BoidTypeData.Keys.ToList())
            BoidTypeData[k] = BoidTypeData[k].Finalize();

        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public AggregatedBoidCell Clear()
    {
        Count = 0;
        BoidTypeData.Clear();

        return this;
    }


    /// <summary>
    /// 
    /// </summary>
    public struct CellData
    {
        public int Count { get; set; }
        public Vector3 Position;
        public Vector3 Direction;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public CellData Add(Boid b)
        {
            Count++;

            Position += b.transform.position;
            Direction += b.transform.forward;

            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public CellData Combine(CellData other)
        {
            Count += other.Count;
            Position += other.Position;
            Direction += other.Direction;

            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public CellData Finalize()
        {
            Position = Position / Count;
            Direction.Normalize();

            return this;
        }
    }
}
