//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FGECore.UtilitySystems
{
    /// <summary>Helper for live-sorted queues of data.</summary>
    /// <typeparam name="T">The data type at hand.</typeparam>
    public class PriorityQueue<T>
    {
        /// <summary>Represents a single node in a queue.</summary>
        private struct Node
        {
            /// <summary>The data at hand.</summary>
            public T Data;

            /// <summary>The priority of the data.</summary>
            public double Priority;
        }

        /// <summary>Where the queue starts.</summary>
        private int start;

        /// <summary>How many nodes are actually in the queue.</summary>
        private int numNodes;

        /// <summary>All current nodes.</summary>
        private Node[] nodes; // TODO: Array possibly isn't the most efficient way to store a priority queue, even when working with structs? Experiment!

        /// <summary>Constructs the priority queue.</summary>
        /// <param name="capacity">The capacity to prepare.</param>
        public PriorityQueue(int capacity = 512)
        {
            start = 0;
            numNodes = 0;
            nodes = new Node[capacity];
        }

        /// <summary>Gets the number of items in the queue.</summary>
        public int Count
        {
            get
            {
                return numNodes;
            }
        }

        /// <summary>Gets the present capacity already prepared.</summary>
        public int Capacity
        {
            get
            {
                return nodes.Length;
            }
        }

        /// <summary>Clears the queue quickly.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            numNodes = 0;
            start = 0;
        }

        /// <summary>Enqueues a new item.</summary>
        /// <param name="nodeData">The data.</param>
        /// <param name="priority">The priority.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(ref T nodeData, double priority)
        {
            if (numNodes + start + 1 >= nodes.Length)
            {
                Resize();
            }
            int first = start;
            int last = start + numNodes;
            int middle = start;
            while (first <= last)
            {
                middle = (first + last) / 2;
                if (priority > nodes[middle].Priority)
                {
                    first = middle + 1;
                }
                if (priority < nodes[middle].Priority)
                {
                    last = middle - 1;
                }
                else
                {
                    break;
                }
            }
            int len = numNodes - (middle - start);
            if (len != 0)
            {
                Array.Copy(nodes, middle, nodes, middle + 1, len);
            }
            nodes[middle].Data = nodeData;
            nodes[middle].Priority = priority;
            numNodes++;
        }

        /// <summary>
        /// Dequeues the highest priority item.
        /// DO NOT CALL IF COUNT IS ZERO!
        /// </summary>
        /// <returns>The item dequeued.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Dequeue()
        {
#if DEBUG
            if (numNodes < 0)
            {
                throw new InvalidOperationException("Cannot dequeue: the queue is empty.");
            }
#endif
            T returnMe = nodes[start].Data;
            numNodes--;
            start++;
            return returnMe;
        }

        /// <summary>Internal method to resize the queue or move it sideways to fit new data.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize()
        {
            if (numNodes * 2 > nodes.Length)
            {
                Node[] newArray = new Node[nodes.Length * 2];
                Array.Copy(nodes, start, newArray, 0, numNodes);
                nodes = newArray;
            }
            else
            {
                // TODO: Circularity to reduce need for this?
                Array.Copy(nodes, start, nodes, 0, numNodes);
            }
            start = 0;
        }

        /// <summary>Gets the first item.</summary>
        public T First
        {
            get
            {
#if DEBUG
                if (numNodes < 0)
                {
                    throw new InvalidOperationException("Cannot get first: the queue is empty.");
                }
#endif
                return nodes[start].Data;
            }
        }
    }
}
