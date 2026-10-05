//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using OpenTK.Graphics.OpenGL4;

namespace FGEGraphics.GraphicsHelpers;

/// <summary>Helper class to track GPU graphics timing, tracks time between start/end spots in nanoseconds.</summary>
public class GraphicsTimer
{
    /// <summary>Internal data for a <see cref="GraphicsTimer"/>.</summary>
    public struct InternalData
    {
        /// <summary>How many queries to generate and load the arrays with.</summary>
        public const int GEN_COUNT = 3;

        /// <summary>If true, this timer has a valid ID.</summary>
        public bool HasIDs;

        /// <summary>Array of query heads to read/write.</summary>
        public QueryHead[] Heads;
        
        /// <summary>Index in the ID arrays to write next.</summary>
        public int WriteHead;

        /// <summary>Index in the ID arrays to read next.</summary>
        public int ReadHead;

        /// <summary>Last known data for this timer.</summary>
        public long? LastData;
    }

    /// <summary>Enumeration of possible states a query head can be in.</summary>
    public enum HeadState : int
    {
        /// <summary>The head is waiting, no data.</summary>
        WAITING = 0,
        /// <summary>Begin has been called, end has not.</summary>
        BEGAN = 1,
        /// <summary>End has been called, the head may be readable.</summary>
        ENDED = 2,
        /// <summary>The head has already been read, has data available, ready for next usage.</summary>
        HAS_DATA = 3,
    }

    /// <summary>A single query head, tracking a single timing range.</summary>
    public class QueryHead
    {
        /// <summary>The internal ID of the starter query.</summary>
        public uint StartID;

        /// <summary>The internal ID of the ender query.</summary>
        public uint EndID;

        /// <summary>Current state of this head.</summary>
        public HeadState State;

        /// <summary>Read data, if any. Only when <see cref="State"/> is <see cref="HeadState.HAS_DATA"/>.</summary>
        public long Data;

        /// <summary>Generate IDs for this head.</summary>
        public void Generate()
        {
            GL.GenQueries(1, out StartID);
            GL.GenQueries(1, out EndID);
            State = HeadState.WAITING;
            Data = 0;
        }

        /// <summary>Delete IDs for this head.</summary>
        public void Destroy()
        {
            GL.DeleteQuery(StartID);
            GL.DeleteQuery(EndID);
            StartID = 0;
            EndID = 0;
            State = HeadState.WAITING;
            Data = 0;
        }
    }

    /// <summary>Internal data for this instance of a <see cref="GraphicsTimer"/>.</summary>
    public InternalData Internal;

    /// <summary>Initialize the <see cref="GraphicsTimer"/>, must be called on main GL thread.</summary>
    public void Init()
    {
        if (Internal.HasIDs)
        {
            return;
        }
        Internal.Heads = new QueryHead[InternalData.GEN_COUNT];
        for (int i = 0; i < InternalData.GEN_COUNT; i++)
        {
            Internal.Heads[i] = new();
            Internal.Heads[i].Generate();
        }
        Internal.HasIDs = true;
        Internal.WriteHead = 0;
        Internal.ReadHead = 0;
        Internal.LastData = null;
    }

    /// <summary>Destroy the <see cref="GraphicsTimer"/>, must be called on main GL thread.</summary>
    public void Destroy()
    {
        if (!Internal.HasIDs)
        {
            return;
        }
        for (int i = 0; i < InternalData.GEN_COUNT; i++)
        {
            Internal.Heads[i].Destroy();
        }
        Internal.HasIDs = false;
        Internal.LastData = null;
    }

    /// <summary>Begin measuring GPU time at the current GPU instruction.</summary>
    public void Begin()
    {
        if (!Internal.HasIDs)
        {
            Init();
        }
        QueryHead head = Internal.Heads[Internal.WriteHead];
        if (head.State == HeadState.ENDED)
        {
            // Don't try to fill new data if we haven't even queried this data yet.
            return;
        }
        GL.QueryCounter(head.StartID, QueryCounterTarget.Timestamp);
        head.State = HeadState.BEGAN;
    }

    /// <summary>Stop measuring GPU time at the current GPU instruction.</summary>
    public void End()
    {
        if (!Internal.HasIDs)
        {
            throw new InvalidOperationException("Cannot end a query that has never initialized.");
        }
        QueryHead head = Internal.Heads[Internal.WriteHead];
        if (head.State != HeadState.BEGAN)
        {
            return;
        }
        GL.QueryCounter(head.EndID, QueryCounterTarget.Timestamp);
        head.State = HeadState.ENDED;
        Internal.WriteHead = (Internal.WriteHead + 1) % InternalData.GEN_COUNT;
    }

    /// <summary>Read the currently waiting data on this timer, last-known value if no new data is ready, or null if no data is available.</summary>
    public long? Read()
    {
        if (!Internal.HasIDs)
        {
            return null;
        }
        for (int i = 0; i < InternalData.GEN_COUNT; i++)
        {
            QueryHead head = Internal.Heads[Internal.ReadHead];
            if (head.State != HeadState.ENDED)
            {
                break;
            }
            GL.GetQueryObject(head.StartID, GetQueryObjectParam.QueryResultAvailable, out int readyStart);
            GL.GetQueryObject(head.EndID, GetQueryObjectParam.QueryResultAvailable, out int readyEnd);
            if (readyStart == 0 || readyEnd == 0)
            {
                break;
            }
            GL.GetQueryObject(head.StartID, GetQueryObjectParam.QueryResult, out long startStamp);
            GL.GetQueryObject(head.EndID, GetQueryObjectParam.QueryResult, out long endStamp);
            head.Data = endStamp - startStamp;
            head.State = HeadState.HAS_DATA;
            Internal.LastData = head.Data;
            Internal.ReadHead = (Internal.ReadHead + 1) % InternalData.GEN_COUNT;
        }
        return Internal.LastData;
    }
}
