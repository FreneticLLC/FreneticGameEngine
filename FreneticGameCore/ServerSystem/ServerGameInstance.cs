//
// This file is created by Frenetic LLC.
// This code is Copyright (C) 2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace FreneticGameCore.ServerSystem
{
    /// <summary>
    /// An instance of a server game.
    /// </summary>
    public class ServerGameInstance
    {
        /// <summary>
        /// Any and all engines running in this instance on the main level.
        /// </summary>
        public List<ServerEngine> Engines = new List<ServerEngine>();

        /// <summary>
        /// Gets the "default" engine: the first in the <see cref="Engines"/> list!
        /// </summary>
        public ServerEngine DefaultEngine
        {
            get
            {
                return Engines[0];
            }
        }

        /// <summary>
        /// Constructs the server game instance.
        /// </summary>
        public ServerGameInstance()
        {
            Engines.Add(new ServerEngine());
            DefaultEngine.LoadBasic();
        }

        /// <summary>
        /// How much time has passed since the instance first loaded.
        /// </summary>
        public double GlobalTickTime = 1;

        /// <summary>
        /// Used to calculate the <see cref="Delta"/> value.
        /// </summary>
        private Stopwatch DeltaCounter;

        /// <summary>
        /// Used as part of accurate tick timing.
        /// </summary>
        private double TotalDelta;

        /// <summary>
        /// What delta amount the instance is currently trying to calculate at.
        /// Inverse of this is present target FPS.
        /// </summary>
        public double TargetDelta;

        /// <summary>
        /// Target frames per second.
        /// </summary>
        public double Target_FPS = 30;

        /// <summary>
        /// The current delta timing for the instance tick.
        /// Represents the amount of time passed since the last tick.
        /// </summary>
        public double Delta = 0;

        /// <summary>
        /// Whether the instance is marked for shutdown as soon as possible.
        /// </summary>
        bool NeedShutdown = false;

        /// <summary>
        /// Lock this object to prevent collision with the instance tick.
        /// </summary>
        public Object TickLock = new Object();

        /// <summary>
        /// The current tick rate of the server.
        /// </summary>
        public int TPS = 0;

        /// <summary>
        /// The scheduling system for this server.
        /// </summary>
        public Scheduler Schedule = new Scheduler();

        /// <summary>
        /// The source object for this instance. Set to any tag style constant reference you find most helpful to keep!
        /// </summary>
        public Object Source;

        /// <summary>
        /// Starts and runs the entire server game instance.
        /// Will take over present thread until completion.
        /// </summary>
        public void StartAndRun()
        {
            // Tick
            double TARGETFPS = 30.0;
            Stopwatch Counter = new Stopwatch();
            DeltaCounter = new Stopwatch();
            DeltaCounter.Start();
            TotalDelta = 0;
            double CurrentDelta = 0.0;
            TargetDelta = 0.0;
            int targettime = 0;
            try
            {
                while (true)
                {
                    // Update the tick time usage counter
                    Counter.Reset();
                    Counter.Start();
                    // Update the tick delta counter
                    DeltaCounter.Stop();
                    // Delta time = Elapsed ticks * (ticks/second)
                    CurrentDelta = ((double)DeltaCounter.ElapsedTicks) / ((double)Stopwatch.Frequency);
                    // Begin the delta counter to find out how much time is /really/ slept+ticked for
                    DeltaCounter.Reset();
                    DeltaCounter.Start();
                    // How much time should pass between each tick ideally
                    TARGETFPS = Target_FPS;
                    if (TARGETFPS < 1 || TARGETFPS > 600)
                    {
                        TARGETFPS = 30;
                    }
                    TargetDelta = (1.0d / TARGETFPS);
                    // How much delta has been built up
                    TotalDelta += CurrentDelta;
                    double tdelt = TargetDelta;
                    while (TotalDelta > tdelt * 3)
                    {
                        // Lagging - cheat to catch up!
                        tdelt *= 2; // TODO: Handle even harder tick loss
                    }
                    // As long as there's more delta built up than delta wanted, tick
                    while (TotalDelta > tdelt)
                    {
                        if (NeedShutdown)
                        {
                            return;
                        }
                        lock (TickLock)
                        {
                            Tick(tdelt);
                        }
                        TotalDelta -= tdelt;
                    }
                    // The tick is done, stop measuring it
                    Counter.Stop();
                    // Only sleep for target milliseconds/tick minus how long the tick took... this is imprecise but that's okay
                    targettime = (int)((1000d / TARGETFPS) - Counter.ElapsedMilliseconds);
                    // Only sleep at all if we're not lagging
                    if (targettime > 0)
                    {
                        // Try to sleep for the target time - very imprecise, thus we deal with precision inside the tick code
                        Thread.Sleep(targettime);
                    }
                }
            }
            catch (ThreadAbortException)
            {
                return;
            }
            catch (Exception ex)
            {
                SysConsole.Output("Server crash", ex);
            }
        }

        /// <summary>
        /// TPS Counter.
        /// </summary>
        private int tpsc = 0;

        /// <summary>
        /// Ticks the server and all engines.
        /// Called automatically by the standard run thread.
        /// </summary>
        /// <param name="delta">How much time has passed since the last tick.</param>
        public void Tick(double delta)
        {
            tpsc++;
            Delta = delta;
            GlobalTickTime += delta;
            Schedule.RunAllSyncTasks(delta);
            foreach (ServerEngine engine in Engines)
            {
                engine.Tick();
            }
        }
    }
}
