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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FGECore.CoreSystems;
using FGECore.ServerSystem.EntitySystem;
using FGECore.StackNoteSystem;
using FGECore.UtilitySystems;

namespace FGECore.ServerSystem;

/// <summary>An instance of a server game.</summary>
public class ServerGameInstance : GameInstance<ServerEntity, ServerEngine>
{
    /// <summary>Constructs the server game instance.</summary>
    /// <param name="shouldDefaultEngine">True to pre-load a default engine, false to skip that step.</param>
    public ServerGameInstance(bool shouldDefaultEngine = true)
    {
        if (shouldDefaultEngine)
        {
            using var _push = StackNoteHelper.UsePush("ServerGameInstance construction, preparation of default engine", this);
            Engines.Add(new ServerEngine()
            {
                OwningInstance = this
            });
            DefaultEngine.LoadBasic();
        }
    }

    /// <summary>Helper class for internal data related to the server instance.</summary>
    public struct InternalData()
    {
        /// <summary>Used as part of accurate tick timing - how much delta has been built up before running the current tick.</summary>
        public double TotalDelta;

        /// <summary>
        /// What delta amount the instance is currently trying to calculate at.
        /// Inverse of this is present target FPS.
        /// </summary>
        public double TargetDelta;

        /// <summary>The value of <see cref="Stopwatch.GetTimestamp"/> when the current tick started.</summary>
        public long TickStartTimestamp = Stopwatch.GetTimestamp();
    }

    /// <summary>Internal data for this server instance.</summary>
    public InternalData Internal = new();

    /// <summary>Target frames per second.</summary>
    public double TargetFPS = 30;

    /// <summary>The current tick rate of the server.</summary>
    public int TPS = 0;

    /// <summary>Classifier term for the instance, for use in debug logs. Defaults to "server".</summary>
    public string InstanceClassification = "server";

    /// <summary>The <see cref="SysConsole"/> output type for "init" messages: <see cref="OutputType.SERVERINIT"/>.</summary>
    public override OutputType InitOutputType => OutputType.SERVERINIT;

    /// <summary>The <see cref="SysConsole"/> output type for "info" messages: <see cref="OutputType.SERVERINFO"/>.</summary>
    public override OutputType InfoOutputType => OutputType.SERVERINFO;

    /// <summary>
    /// Starts and runs the entire server game instance.
    /// Will take over present thread until completion.
    /// Will shut down the instance fully at the end.
    /// </summary>
    public void StartAndRun()
    {
        double targetFps, currentDelta;
        long priorTickStart;
        int targetTime;
        Internal.TotalDelta = 0;
        Internal.TargetDelta = 0;
        using var _push = StackNoteHelper.UsePush("ServerGameInstance main loop - StartAndRun", this);
        try
        {
            while (true)
            {
                priorTickStart = Internal.TickStartTimestamp;
                Internal.TickStartTimestamp = Stopwatch.GetTimestamp();
                // Delta time = Elapsed ticks * (ticks/second)
                currentDelta = (Internal.TickStartTimestamp - priorTickStart) / (double)Stopwatch.Frequency;
                // How much time should pass between each tick ideally
                targetFps = TargetFPS;
                if (targetFps < 1 || targetFps > 600)
                {
                    targetFps = 30;
                }
                Internal.TargetDelta = 1.0 / targetFps;
                // How much delta has been built up
                Internal.TotalDelta += currentDelta;
                double tdelt = Internal.TargetDelta;
                while (Internal.TotalDelta > tdelt * 3)
                {
                    // Lagging - cheat to catch up!
                    tdelt *= 2;
                }
                // As long as there's more delta built up than delta wanted, tick
                while (Internal.TotalDelta > tdelt)
                {
                    if (InstanceShutdownToken.IsCancellationRequested)
                    {
                        return;
                    }
                    lock (TickLock)
                    {
                        PreTick(tdelt);
                        Tick();
                    }
                    Internal.TotalDelta -= tdelt;
                }
                // Only sleep for target milliseconds/tick minus how long the tick took... this is imprecise but that's okay
                long timeStamp = Stopwatch.GetTimestamp();
                double elapsedSeconds = (timeStamp - Internal.TickStartTimestamp) / (double)Stopwatch.Frequency;
                targetTime = (int)Math.Floor((1000d / targetFps) - (elapsedSeconds * 1000));
                CommonUtilities.StableSleep(targetTime, timeStamp);
            }
        }
        catch (ThreadAbortException)
        {
            return;
        }
        catch (Exception ex)
        {
            SysConsole.Output($"{InstanceClassification} [{Name}] crash", ex);
        }
        finally
        {
            InstanceShutdown();
        }
    }

    /// <summary>Returns a simple output string for the server game instance.</summary>
    /// <returns>The server game instance.</returns>
    public override string ToString()
    {
        return "ServerGameInstance, root Engine count=" + Engines.Count;
    }
}
