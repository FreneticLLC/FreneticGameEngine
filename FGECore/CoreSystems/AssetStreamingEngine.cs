//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FreneticUtilities.FreneticExtensions;
using FGECore.FileSystems;

namespace FGECore.CoreSystems;

/// <summary>A special helper class to assist with live multithreaded asset streaming.</summary>
/// <param name="_files">The backing file engine.</param>
/// <param name="_schedule">The backing scheduler.</param>
public class AssetStreamingEngine(FileEngine _files, Scheduler _schedule)
{
    /// <summary>The dedicated file reading thread (splitting across multiple threads is unlikely to benefit anything, and may even get in the way).</summary>
    public Thread FilesThread;

    /// <summary>The backing file engine.</summary>
    public FileEngine Files = _files;

    /// <summary>The backing scheduler.</summary>
    public Scheduler Schedule = _schedule;

    /// <summary>
    /// All currently waiting asset streaming goals, mapped by priority level.
    /// Controlled mainly by <see cref="AddGoal"/>.
    /// </summary>
    public ConcurrentQueue<StreamGoal>[] Goals = [new(), new(), new(), new(), new()];

    /// <summary>Enumeration of priority levels for asset streaming goals.</summary>
    public enum GoalPriority : int
    {
        /// <summary>Need this data immediately! Supercede all others!</summary>
        FASTEST = 0,
        /// <summary>This data should be grabbed quickly. Ideal for thumbnails, proxies, etc. to keep user experience clean.</summary>
        FAST = 1,
        /// <summary>Standard data read, when priority choices don't apply.</summary>
        NORMAL = 2,
        /// <summary>Slower than normal - ideal for a large asset that can be separately proxied with a FAST read.</summary>
        SLOW = 3,
        /// <summary>I do not care! Read it tomorrow!</summary>
        SLOWEST = 4
    }

    /// <summary>
    /// Reset event for when the files thread has more goals to process.
    /// Set mainly by <see cref="AddGoal"/>.
    /// </summary>
    public AutoResetEvent GoalWaitingReset = new(false);

    /// <summary>The cancel token for <see cref="FilesThread"/>.</summary>
    public CancellationTokenSource FileThreadCancelToken = new();

    /// <summary>Starts the asset streaming engine.</summary>
    public void Init()
    {
        FilesThread = new Thread(new ThreadStart(FilesMainLoop)) { Name = "assetstreamingfiles" };
        FilesThread.Start();
    }

    /// <summary>Shuts down the asset streaming engine.</summary>
    public void Shutdown()
    {
        FileThreadCancelToken.Cancel();
        GoalWaitingReset.Set();
    }

    /// <summary>The asset streaming goal to be executed.</summary>
    public class StreamGoal
    {
        /// <summary>The name of the file to load. MUST be set.</summary>
        public string FileName;

        /// <summary>Optional, alternate file extensions that will be accepted.</summary>
        public string[] AltExtensions;

        /// <summary>
        /// Action to call in the case of an error. The first parameter is the error message.
        /// If unset, errors go to the <see cref="SysConsole"/>.
        /// </summary>
        public Action<string> OnError = null;

        /// <summary>Action to call if the file is not present. If unset, will become an error message.</summary>
        public Action OnFileMissing = null;

        /// <summary>Called to process the file data once loaded. MUST be set.</summary>
        public Action<byte[]> ProcessData;

        /// <summary>The priority level of this goal.</summary>
        public GoalPriority Priority;

        /// <summary>
        /// Whether to sync the process result call to the main thread (if not, runs async).
        /// Defaults to false.
        /// </summary>
        public bool ShouldSyncToMainThread = false;

        /// <summary>Handles a file-missing situation.</summary>
        public void HandleFileMissing()
        {
            try
            {
                if (OnFileMissing is not null)
                {
                    OnFileMissing();
                }
                else
                {
                    HandleError($"File '{FileName}' not found.");
                }
            }
            catch (Exception ex2)
            {
                Logs.Error($"Exception in asset streaming error handler: {ex2}\nCaused by: File not found.");
            }
        }

        /// <summary>Handles an error message.</summary>
        /// <param name="message">The error message.</param>
        public void HandleError(string message)
        {
            try
            {
                if (OnError is not null)
                {
                    OnError(message);
                }
                else
                {
                    Logs.Error($"Asset streaming engine encountered error: {message}");
                }
            }
            catch (Exception ex2)
            {
                Logs.Error($"Exception in asset streaming error handler: {ex2}\nCaused by:\n{message}");
            }
        }
    }

    /// <summary>Entry point of the Files thread, internal usage.</summary>
    public void FilesMainLoop()
    {
        while (true)
        {
            GoalWaitingReset.WaitOne();
            if (FileThreadCancelToken.IsCancellationRequested)
            {
                return;
            }
        backToZero:
            for (int i = 0; i < Goals.Length; i++)
            {
                if (Goals[i].TryDequeue(out StreamGoal goal))
                {
                    if (FileThreadCancelToken.IsCancellationRequested)
                    {
                        return;
                    }
                    ProcessGoal(goal);
                    goto backToZero;
                }
            }
        }
    }

    /// <summary>Process a single asset streaming goal, internal usage.</summary>
    public void ProcessGoal(StreamGoal goal)
    {
        try
        {
            if (!Files.TryReadFileData(goal.FileName, out byte[] data))
            {
                bool found = false;
                if (goal.AltExtensions is not null)
                {
                    string noExt = goal.FileName.BeforeLast('.');
                    foreach (string ext in goal.AltExtensions)
                    {
                        if (Files.TryReadFileData($"{noExt}.{ext}", out data))
                        {
                            found = true;
                            break;
                        }
                    }
                }
                if (!found)
                {
                    goal.HandleFileMissing();
                    return;
                }
            }
            void CallProcData()
            {
                if (FileThreadCancelToken.IsCancellationRequested)
                {
                    return;
                }
                goal.ProcessData(data);
            }
            if (goal.ShouldSyncToMainThread)
            {
                Schedule.ScheduleSyncTask(CallProcData);
            }
            else
            {
                Schedule.StartAsyncTask(CallProcData);
            }
        }
        catch (Exception ex)
        {
            goal.HandleError(ex.ToString());
        }
    }

    /// <summary>Adds a new goal to the system.</summary>
    /// <param name="fileName">The name of the file to load.</param>
    /// <param name="processOnMainThread">Whether to sync the process result call to the main thread (if not, runs async).</param>
    /// <param name="processAction">Called to process the file data once loaded.</param>
    /// <param name="onFileMissing">(Optional) called to handle a file-missing situation.</param>
    /// <param name="onError">(Optional) called to handle an error message. If unset, errors go to the <see cref="SysConsole"/>.</param>
    /// <param name="altExtensions">Alternate file extensions that are also acceptable.</param>
    /// <param name="priority">The priority level for this goal.</param>
    /// <returns>The created <see cref="StreamGoal"/>.</returns>
    public StreamGoal AddGoal(string fileName, bool processOnMainThread, Action<byte[]> processAction, Action onFileMissing = null, Action<string> onError = null, string[] altExtensions = null, GoalPriority priority = GoalPriority.NORMAL)
    {
        StreamGoal goal = new()
        {
            FileName = fileName,
            ShouldSyncToMainThread = processOnMainThread,
            ProcessData = processAction,
            OnFileMissing = onFileMissing,
            OnError = onError,
            AltExtensions = altExtensions
        };
        Goals[(int)priority].Enqueue(goal);
        GoalWaitingReset.Set();
        return goal;
    }
}
