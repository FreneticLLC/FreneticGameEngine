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
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FGECore.StackNoteSystem;
using FGECore.CoreSystems;
using FGECore.ServerSystem.EntitySystem;

namespace FGECore.ServerSystem
{
    /// <summary>
    /// An instance of a server game.
    /// </summary>
    public class ServerGameInstance : GameInstance<ServerEntity, ServerEngine>
    {
        /// <summary>
        /// Constructs the server game instance.
        /// </summary>
        public ServerGameInstance()
        {
            try
            {
                StackNoteHelper.Push("ServerGameInstance construction, preparation of default engine", this);
                Engines.Add(new ServerEngine()
                {
                    OwningInstance = this
                });
                DefaultEngine.LoadBasic();
            }
            finally
            {
                StackNoteHelper.Pop();
            }
        }

        /// <summary>
        /// Used to calculate the <see cref="GameInstance{T, T2}.Delta"/> value.
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
        /// The current tick rate of the server.
        /// </summary>
        public int TPS = 0;

        /// <summary>
        /// Starts and runs the entire server game instance.
        /// Will take over present thread until completion.
        /// </summary>
        public void StartAndRun()
        {
            // Tick
            double TARGETFPS;
            Stopwatch Counter = new Stopwatch();
            DeltaCounter = new Stopwatch();
            DeltaCounter.Start();
            TotalDelta = 0;
            double CurrentDelta;
            TargetDelta = 0.0;
            int targettime;
            try
            {
                StackNoteHelper.Push("ServerGameInstance main loop - StartAndRun", this);
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
                        tdelt *= 2;
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
                            PreTick(tdelt);
                            Tick();
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
            finally
            {
                StackNoteHelper.Pop();
            }
        }

        /// <summary>
        /// Returns a simple output string for the server game instance.
        /// </summary>
        /// <returns>The server game instance.</returns>
        public override string ToString()
        {
            return "ServerGameInstance, root Engine count=" + Engines.Count;
        }
    }
}
