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
using FreneticUtilities.FreneticToolkit;
using FGECore.UtilitySystems;

namespace FGECore.CoreSystems
{
    /// <summary>Helper to schedule sync or async tasks.</summary>
    public class Scheduler
    {
        /// <summary>Current set of tasks.</summary>
        public ConcurrentQueue<SyncScheduleItem> Tasks = new ConcurrentQueue<SyncScheduleItem>();

        /// <summary>Removes all pending items from the scheduler.</summary>
        public void Clear()
        {
            while (Tasks.TryDequeue(out _))
            {
                // Do nothing.
            }
        }

        /// <summary>A helper class to help with interactions with <see cref="FreneticEventHelper"/>.</summary>
        public class SchedulerEventHelper : FreneticEventHelper
        {
            /// <summary>The scheduler object.</summary>
            public Scheduler Schedule;

            /// <summary>Start an asynchronous action.</summary>
            /// <param name="act">The relevant action.</param>
            public override void StartAsync(Action act)
            {
                Schedule.StartAsyncTask(act);
            }

            /// <summary>Schedules a synchronous action.</summary>
            /// <param name="act">The relevant action.</param>
            public override void ScheduleSync(Action act)
            {
                Schedule.ScheduleSyncTask(act);
            }

            /// <summary>Schedules a synchronous action.</summary>
            /// <param name="act">The relevant action.</param>
            /// <param name="delay">The delay before starting.</param>
            public override void ScheduleSync(Action act, double delay)
            {
                Schedule.ScheduleSyncTask(act, delay);
            }
        }

        /// <summary>The event helper for this scheduler.</summary>
        public SchedulerEventHelper EventHelper;

        /// <summary>Constructs the scheduler.</summary>
        public Scheduler()
        {
            EventHelper = new SchedulerEventHelper() { Schedule = this };
        }

        /// <summary>Gets a simple string representation of this scheduler.</summary>
        /// <returns>The simple string.</returns>
        public override string ToString()
        {
            return "Scheduler engine...";
        }

        /// <summary>Gets a sync task object, not yet scheduled.</summary>
        /// <param name="act">The action to call.</param>
        /// <param name="delay">The delay value.</param>
        /// <returns>A schedule item.</returns>
        public SyncScheduleItem GetSyncTask(Action act, double delay = 0)
        {
            return new SyncScheduleItem() { MyAction = act, Time = delay, OwningEngine = this };
        }

        /// <summary>Creates and schedules a sync task.</summary>
        /// <param name="act">The action to run.</param>
        /// <param name="delay">How long before the task is called.</param>
        /// <returns>The scheduled item.</returns>
        public SyncScheduleItem ScheduleSyncTask(Action act, double delay = 0)
        {
            SyncScheduleItem item = new SyncScheduleItem() { MyAction = act, Time = delay, OwningEngine = this };
            Tasks.Enqueue(item);
            return item;
        }

        private static readonly SyncScheduleItem END_ITEM = new SyncScheduleItem();

        /// <summary>Ran every frame to cause all sync tasks to be processed.</summary>
        /// <param name="time">The delta time.</param>
        public void RunAllSyncTasks(double time)
        {
            Tasks.Enqueue(END_ITEM);
            while (Tasks.TryDequeue(out SyncScheduleItem item))
            {
                if (item == END_ITEM)
                {
                    return;
                }
                item.Time -= time;
                if (item.Time > 0)
                {
                    Tasks.Enqueue(item);
                    continue;
                }
                try
                {
                    item.MyAction.Invoke();
                }
                catch (Exception ex)
                {
                    if (ex is ThreadAbortException)
                    {
                        throw;
                    }
                    SysConsole.Output("Handling sync task", ex);
                }
            }
        }

        /// <summary>Starts an async task.</summary>
        /// <param name="action">The action to launch async.</param>
        /// <param name="isImportant">Whether this action is considered important.</param>
        /// <returns>The scheduled item.</returns>
        public ASyncScheduleItem StartAsyncTask(Action action, bool isImportant = false)
        {
            ASyncScheduleItem asyncer = new ASyncScheduleItem() { OwningEngine = this, MyAction = action, UnImportant = !isImportant };
            asyncer.RunMe();
            return asyncer;
        }

        /// <summary>Creates but does not start an async task.</summary>
        /// <param name="a">The action to launch async.</param>
        /// <param name="followUp">Optional followup task.</param>
        /// <returns>The created schedule item.</returns>
        public ASyncScheduleItem AddAsyncTask(Action a, ASyncScheduleItem followUp = null)
        {
            ASyncScheduleItem asyncer = new ASyncScheduleItem() { OwningEngine = this, MyAction = a, FollowUp = followUp };
            return asyncer;
        }
    }

    /// <summary>Represents a schedulable item.</summary>
    public abstract class ScheduleItem
    {
        /// <summary>Runs the schedulable item.</summary>
        public abstract void RunMe();

        /// <summary>The relevant scheduler.</summary>
        public Scheduler OwningEngine;
    }

    /// <summary>Represents a synchronous scheduled item.</summary>
    public class SyncScheduleItem : ScheduleItem
    {
        /// <summary>The action to run.</summary>
        public Action MyAction;

        /// <summary>The time left before running.</summary>
        public double Time = 0;

        /// <summary>Causes the action to be run at the next frame.</summary>
        public override void RunMe()
        {
            OwningEngine.ScheduleSyncTask(MyAction);
        }
    }

    /// <summary>Represents an asynchronous running item.</summary>
    public class ASyncScheduleItem : ScheduleItem
    {
        /// <summary>The action to run.</summary>
        public Action MyAction;

        /// <summary>The next thing to run in this sequence.</summary>
        public ASyncScheduleItem FollowUp = null;

        /// <summary>Locker to prevent thread issues.</summary>
        readonly LockObject Locker = new LockObject();

        /// <summary>Whether the item has been started.</summary>
        public bool Started = false;

        /// <summary>Whether the item is complete.</summary>
        bool Done = false;

        /// <summary>Whether this event is unimportant, and thus can be treated as lower priority.</summary>
        public bool UnImportant = true;

        /// <summary>Gets whether the item has started.</summary>
        public bool HasStarted()
        {
            lock (Locker)
            {
                return Started;
            }
        }

        /// <summary>Gets whether the item is complete.</summary>
        public bool IsDone()
        {
            lock (Locker)
            {
                return Done;
            }
        }

        /// <summary>Replaces the schedule item if its not yet started, otherwises follows it with a new item.</summary>
        /// <param name="item">The replacement item.</param>
        /// <returns>The final item.</returns>
        public ASyncScheduleItem ReplaceOrFollowWith(ASyncScheduleItem item)
        {
            lock (Locker)
            {
                if (FollowUp != null)
                {
                    return FollowUp.ReplaceOrFollowWith(item);
                }
                if (Started)
                {
                    if (Done)
                    {
                        item.RunMe();
                        return item;
                    }
                    else
                    {
                        FollowUp = item;
                        return item;
                    }
                }
                else
                {
                    MyAction = item.MyAction;
                    FollowUp = item.FollowUp;
                    return this;
                }
            }
        }

        /// <summary>Tells the item to follow the current item with a new one.</summary>
        /// <param name="item">The follower item.</param>
        public void FollowWith(ASyncScheduleItem item)
        {
            lock (Locker)
            {
                if (Done)
                {
                    item.RunMe();
                }
                else
                {
                    FollowUp = item;
                }
            }
        }

        /// <summary>Runs the item asynchronously immediately.</summary>
        public override void RunMe()
        {
            lock (Locker)
            {
                if (Started && !Done)
                {
                    return;
                }
                Started = true;
                Done = false;
            }
            Created = Task.Factory.StartNew(RunInternal);
        }

        /// <summary>The created task, if any.</summary>
        public Task Created;

        /// <summary>Internal runner for the item.</summary>
        private void RunInternal()
        {
            try
            {
                if (UnImportant)
                {
                    Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
                }
                MyAction.Invoke();
            }
            catch (Exception ex)
            {
                if (ex is ThreadAbortException)
                {
                    throw;
                }
                SysConsole.Output("Running Asynchronous task", ex);
            }
            finally
            {
                Thread.CurrentThread.Priority = ThreadPriority.Normal;
            }
            lock (Locker)
            {
                Done = true;
            }
            if (FollowUp != null)
            {
                FollowUp.RunMe();
            }
        }
    }
}
