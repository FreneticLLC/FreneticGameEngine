//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using FGECore.EntitySystem;
using FGECore.StackNoteSystem;
using FreneticUtilities.FreneticToolkit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FGECore.CoreSystems
{
    /// <summary>
    /// Watchdog thread manager for <see cref="GameInstance"/>.
    /// </summary>
    public class InstanceWatchdog
    {
        /// <summary>
        /// The watchdog SysConsole output type.
        /// </summary>
        public static OutputType OUT_TYPE = new OutputType() { Name = "WATCHDOG", BaseColor = "^r^3" };

        /// <summary>
        /// The relevant <see cref="GameInstance"/> being watched.
        /// </summary>
        public GameInstance Instance;

        /// <summary>
        /// The primary <see cref="GameInstance"/> tick thread.
        /// </summary>
        public Thread WatchedThread;

        /// <summary>
        /// The <see cref="StackNoteSet"/> for the <see cref="WatchedThread"/>.
        /// </summary>
        public StackNoteSet NotesForWatchedThread;

        /// <summary>
        /// Internal data, do not touch.
        /// </summary>
        public struct InternalData
        {
            /// <summary>
            /// The async lock object for starts/stops.
            /// </summary>
            public LockObject Lock;

            /// <summary>
            /// Counter, incremented via <see cref="InstanceWatchdog.IsAlive"/>.
            /// </summary>
            public ulong Counter;

            /// <summary>
            /// The cancel token, to stop the watchdog.
            /// </summary>
            public CancellationTokenSource CancelToken;
        }

        /// <summary>
        /// Internal data, do not touch.
        /// </summary>
        public InternalData Internal;

        /// <summary>
        /// Constructs and readies the watchdog.
        /// Call <see cref="Start"/> to begin watching.
        /// </summary>
        public InstanceWatchdog(GameInstance _instance)
        {
            Instance = _instance;
            Internal.Lock = new LockObject();
        }

        /// <summary>
        /// Starts the watchdog.
        /// Call <see cref="Stop"/> when done.
        /// </summary>
        public void Start()
        {
            lock (Internal.Lock)
            {
                if (WatchedThread != null)
                {
                    throw new InvalidOperationException("Watchdog already started.");
                }
                Internal.CancelToken = new CancellationTokenSource();
                WatchedThread = Thread.CurrentThread;
                NotesForWatchedThread = StackNoteHelper.Notes;
                new Thread(MainWatchdogLoop)
                {
                    Name = "fge_instance_watchdog"
                }.Start();
            }
        }

        /// <summary>
        /// The main watch dog loop.
        /// Call <see cref="Start"/> to start this.
        /// </summary>
        public async void MainWatchdogLoop()
        {
            Internal.Counter = 0;
            ulong lastId = 0;
            int ticksDead = 0;
            try
            {
                while (true)
                {
                    await Task.Delay(1000, Internal.CancelToken.Token);
                    if (Internal.CancelToken.IsCancellationRequested)
                    {
                        return;
                    }
                    ulong newId = Interlocked.Read(ref Internal.Counter);
                    if (newId != lastId)
                    {
                        lastId = newId;
                        ticksDead = 0;
                        continue;
                    }
                    ticksDead++;
                    if (ticksDead % 10 == 0)
                    {
                        if (Internal.CancelToken.IsCancellationRequested)
                        {
                            return;
                        }
                        string instanceData = $"Game instance (type={Instance.GetType().Name} threadID={WatchedThread.ManagedThreadId}, threadName={WatchedThread.Name})";
                        string type = WatchedThread.IsAlive ? "unresponsive" : "DEAD";
                        SysConsole.Output(OUT_TYPE, $"{instanceData} {type} for {ticksDead} seconds... stack notes:\n{NotesForWatchedThread}");
                    }
                }
            }
            catch (TaskCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                SysConsole.Output("Watchdog crash", ex);
            }
        }

        /// <summary>
        /// Signal to the watchdog that the original thread is still alive.
        /// </summary>
        public void IsAlive()
        {
            Interlocked.Increment(ref Internal.Counter);
        }

        /// <summary>
        /// Stops the watchdog.
        /// </summary>
        public void Stop()
        {
            lock (Internal.Lock)
            {
                Internal.CancelToken.Cancel();
                WatchedThread = null;
                NotesForWatchedThread = null;
            }
        }
    }
}
