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
using System.Text;
using System.Threading;
using BepuUtilities;
using BepuUtilities.Memory;

namespace FGECore.PhysicsSystem
{
    /// <summary>
    /// Implements <see cref="IThreadDispatcher"/>.
    /// Source is mostly just copied from the bepu demos and corrected to C# standards.
    /// Some documentation and comments originate from BEPU, some is added for FGE to enforce documentation standard.
    /// </summary>
    public class BepuThreadDispatcher : IThreadDispatcher, IDisposable
    {
        /// <summary>
        /// Gets the number of workers available in the thread dispatcher.
        /// </summary>
        /// <remarks>
        /// Note that some systems (like the solver) expect the ThreadCount to be backed by truly independent threads capable of progression even when one is blocked.
        /// If the ThreadCount doesn't represent independent threads, deadlocks will occur.
        /// </remarks>
        public int ThreadCount { get; set; }

        struct Worker
        {
            public Thread Thread;
            public AutoResetEvent Signal;
        }

        readonly Worker[] Workers;
        readonly AutoResetEvent Finished;
        readonly BufferPool[] BufferPools;

        /// <summary>Constructs the <see cref="BepuThreadDispatcher"/>.</summary>
        public BepuThreadDispatcher(int _threadCount)
        {
            ThreadCount = _threadCount;
            Workers = new Worker[ThreadCount - 1];
            for (int i = 0; i < Workers.Length; i++)
            {
                Workers[i] = new Worker { Thread = new Thread(WorkerLoop), Signal = new AutoResetEvent(false) };
                Workers[i].Thread.IsBackground = true;
                Workers[i].Thread.Start(Workers[i].Signal);
            }
            Finished = new AutoResetEvent(false);
            BufferPools = new BufferPool[ThreadCount];
            for (int i = 0; i < BufferPools.Length; i++)
            {
                BufferPools[i] = new BufferPool();
            }
        }

        void DispatchThread(int workerIndex)
        {
            WorkerBody(workerIndex);
            if (Interlocked.Increment(ref CompletedWorkerCounter) == ThreadCount)
            {
                Finished.Set();
            }
        }

        volatile Action<int> WorkerBody;
        int WorkerIndex;
        int CompletedWorkerCounter;

        void WorkerLoop(object untypedSignal)
        {
            AutoResetEvent signal = untypedSignal as AutoResetEvent;
            while (true)
            {
                signal.WaitOne();
                if (Disposed)
                {
                    return;
                }
                DispatchThread(Interlocked.Increment(ref WorkerIndex) - 1);
            }
        }

        void SignalThreads()
        {
            for (int i = 0; i < Workers.Length; i++)
            {
                Workers[i].Signal.Set();
            }
        }

        /// <summary>
        /// Dispatches all the available workers.
        /// </summary>
        /// <param name="workerBody">Delegate to be invoked on for every worker.</param>
        public void DispatchWorkers(Action<int> workerBody)
        {
            WorkerIndex = 1; //Just make the inline thread worker 0. While the other threads might start executing first, the user should never rely on the dispatch order.
            CompletedWorkerCounter = 0;
            this.WorkerBody = workerBody;
            SignalThreads();
            //Calling thread does work. No reason to spin up another worker and block this one!
            DispatchThread(0);
            Finished.WaitOne();
            this.WorkerBody = null;
        }

        volatile bool Disposed;

        /// <summary>Disposes the <see cref="BepuThreadDispatcher"/>.</summary>
        public void Dispose()
        {
            if (!Disposed)
            {
                GC.SuppressFinalize(this);
                Disposed = true;
                SignalThreads();
                for (int i = 0; i < BufferPools.Length; ++i)
                {
                    BufferPools[i].Clear();
                }
                foreach (var worker in Workers)
                {
                    worker.Thread.Join();
                    worker.Signal.Dispose();
                }
            }
        }

        /// <summary>
        /// Gets the memory pool associated with a given worker index. It is guaranteed that no other workers will share the same pool for the duration of the worker's execution.
        /// </summary>
        /// <remarks>
        /// All usages of the memory pool within the simulation are guaranteed to return thread pool memory before the function returns. In other words,
        /// thread memory pools are used for strictly ephemeral memory, and it will never be held by the simulation outside the scope of a function that 
        /// takes the IThreadDispatcher as input.
        /// </remarks>
        /// <param name="workerIndex">Index of the worker to grab the pool for.</param>
        /// <returns>The memory pool for the specified worker index.</returns>
        public BufferPool GetThreadMemoryPool(int workerIndex)
        {
            return BufferPools[workerIndex];
        }
    }
}
