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
using System.Threading.Tasks;

namespace FreneticGameCore.CoreSystems
{
    /// <summary>
    /// Represents an event within the Frenetic Game Engine.
    /// </summary>
    public class FreneticEvent<T> where T : EventArgs
    {
        /// <summary>
        /// All event handlers for this event.
        /// </summary>
        public List<HandlerSet> Handlers = new List<HandlerSet>();

        /// <summary>
        /// A map of sources to what they handle.
        /// </summary>
        public Dictionary<Object, List<HandlerIndex>> HandlersBySource = new Dictionary<Object, List<HandlerIndex>>();

        /// <summary>
        /// Represents the index of a handler in the event handler list.
        /// </summary>
        public class HandlerIndex
        {
            /// <summary>
            /// The handler set object itself.
            /// </summary>
            public HandlerSet SetObject;

            /// <summary>
            /// The index within the handler set.
            /// </summary>
            public int SetIndex;
        }

        /// <summary>
        /// Represents a set of handlers with the same priority.
        /// </summary>
        public class HandlerSet
        {
            /// <summary>
            /// The script priority.
            /// </summary>
            public int Priority;

            /// <summary>
            /// The index within the main handler list.
            /// </summary>
            public int Index;

            /// <summary>
            /// The event handlers contained in the set.
            /// </summary>
            public List<KeyValuePair<HandlerIndex, FreneticEventFirer<T>>> Handlers = new List<KeyValuePair<HandlerIndex, FreneticEventFirer<T>>>();
        }

        /// <summary>
        /// Helper for various usages, primarily scheduling.
        /// </summary>
        public FreneticEventHelper Helper;

        /// <summary>
        /// Constructs the <see cref="FreneticEvent{T}"/>.
        /// </summary>
        /// <param name="_helper">The relevant helper object.</param>
        public FreneticEvent(FreneticEventHelper _helper)
        {
            Helper = _helper;
        }

        /// <summary>
        /// Returns whether the <see cref="FreneticEvent{T}"/> has any handlers. If this returns false, firing the event will do nothing.
        /// </summary>
        /// <returns>Whether the event is handled.</returns>
        public bool IsHandled()
        {
            return Handlers.Count > 0;
        }

        private HandlerIndex ProcessingPatch;

        private HandlerIndex CurrentlyProcessing;

        /// <summary>
        /// Fire the event with the given arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="waiters">A list of waiters to output into, if any.</param>
        public void Fire(T args, List<FreneticEventWaiter> waiters = null)
        {
            try
            {
                for (int i = 0; i < Handlers.Count; i++)
                {
                    HandlerSet set = Handlers[i];
                    for (int x = 0; x < set.Handlers.Count; x++)
                    {
                        CurrentlyProcessing = set.Handlers[x].Key;
                        FreneticEventArgs<T> fargs = new FreneticEventArgs<T>()
                        {
                            PriorityPosition = set.Priority,
                            Context = args,
                            Helper = Helper
                        };
                        FreneticEventWaiter few = set.Handlers[x].Value.Fire(fargs);
                        if (waiters != null && few != null)
                        {
                            waiters.Add(few);
                        }
                        if (ProcessingPatch != null)
                        {
                            x = ProcessingPatch.SetIndex;
                            ProcessingPatch = null;
                        }
                    }
                    i = Handlers[i].Index;
                }
            }
            finally
            {
                ProcessingPatch = null;
                CurrentlyProcessing = null;
            }
        }

        /// <summary>
        /// Fire the event with the given arguments, with a callback action to indicate delayed completion.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="complete">Action to fire when completed.</param>
        public void Fire(T args, Action complete)
        {
            List<FreneticEventWaiter> fews = new List<FreneticEventWaiter>();
            Fire(args, fews);
            if (fews.Count == 0)
            {
                complete();
                return;
            }
            Helper.StartAsync(() =>
            {
                foreach (FreneticEventWaiter few in fews)
                {
                    few.MREFinalComplete.WaitOne();
                }
                Helper.ScheduleSync(complete);
            });
        }

        /// <summary>
        /// Removes all event handlers from a given source.
        /// </summary>
        /// <param name="sourceTracker">The source.</param>
        /// <returns>Whether anything was removed.</returns>
        public bool RemoveBySource(Object sourceTracker)
        {
            if (!HandlersBySource.TryGetValue(sourceTracker, out List<HandlerIndex> indices))
            {
                return false;
            }
            HandlersBySource.Remove(sourceTracker);
            foreach (HandlerIndex index in indices)
            {
                RemoveByIndex(index);
            }
            return true;
        }

        /// <summary>
        /// Removes the specific handler at a given index.
        /// </summary>
        /// <param name="index">The index.</param>
        public void RemoveByIndex(HandlerIndex index)
        {
            List<KeyValuePair<HandlerIndex, FreneticEventFirer<T>>> handlersInSet = index.SetObject.Handlers;
            handlersInSet.RemoveAt(index.SetIndex);
            if (handlersInSet.Count == 0)
            {
                Handlers.RemoveAt(index.SetObject.Index);
                if (ProcessingPatch != null && index.SetObject.Index <= ProcessingPatch.SetObject.Index)
                {
                    ProcessingPatch.SetObject.Index--;
                }
                for (int i = index.SetObject.Index; i < Handlers.Count; i++)
                {
                    Handlers[i].Index--;
                }
            }
            else
            {
                if (ProcessingPatch != null && index.SetObject.Index == ProcessingPatch.SetObject.Index && ProcessingPatch.SetIndex >= index.SetIndex)
                {
                    ProcessingPatch.SetIndex--;
                }
                for (int i = index.SetIndex; i < handlersInSet.Count; i++)
                {
                    handlersInSet[i].Key.SetIndex--;
                }
            }
            if (index == CurrentlyProcessing)
            {
                ProcessingPatch = CurrentlyProcessing;
            }
        }

        /// <summary>
        /// Adds an event firer to the event.
        /// </summary>
        /// <param name="firer">The firer.</param>
        /// <param name="sourceTracker">The object sourcing this event (for example, a plugin object).</param>
        /// <param name="priority">The priority of the handler.</param>
        /// <returns>The index of the added handler.</returns>
        public HandlerIndex AddEvent(FreneticEventFirer<T> firer, Object sourceTracker, double priority)
        {
            HandlerSet set;
            int index;
            for (index = 0; index < Handlers.Count; index++)
            {
                if (Handlers[index].Priority == priority)
                {
                    set = Handlers[index];
                    goto buildset;
                }
                else if (Handlers[index].Priority > priority)
                {
                    set = new HandlerSet();
                    Handlers.Insert(index, set);
                    goto buildset;
                }
            }
            set = new HandlerSet();
            Handlers.Add(set);
            buildset:
            HandlerIndex indexObject = new HandlerIndex() { SetObject = set, SetIndex = set.Handlers.Count };
            if (!HandlersBySource.TryGetValue(sourceTracker, out List<HandlerIndex> trackerIndices))
            {
                trackerIndices = new List<HandlerIndex>();
                HandlersBySource.Add(sourceTracker, trackerIndices);
            }
            trackerIndices.Add(indexObject);
            set.Handlers.Add(new KeyValuePair<HandlerIndex, FreneticEventFirer<T>>(indexObject, firer));
            for (int i = index + 1; i < Handlers.Count; i++)
            {
                Handlers[i].Index++;
            }
            return indexObject;
        }

        /// <summary>
        /// Add a non-waitable event handler to this event.
        /// </summary>
        /// <param name="act">The handler.</param>
        /// <param name="sourceTracker">The object sourcing this event (for example, a plugin object).</param>
        /// <param name="priority">The priority of the handler.</param>
        /// <returns>The index of the added handler.</returns>
        public HandlerIndex AddEvent(Action<FreneticEventArgs<T>> act, Object sourceTracker, double priority)
        {
            return AddEvent(new FreneticEventFirer<T>(act), sourceTracker, priority);
        }

        /// <summary>
        /// Add a waitable event handler to this event.
        /// </summary>
        /// <param name="act">The handler.</param>
        /// <param name="sourceTracker">The object sourcing this event (for example, a plugin object).</param>
        /// <param name="priority">The priority of the handler.</param>
        /// <returns>The index of the added handler.</returns>
        public HandlerIndex AddEvent(Action<FreneticEventArgs<T>, FreneticEventWaiter> act, Object sourceTracker, double priority)
        {
            return AddEvent(new FreneticEventFirer<T>(act), sourceTracker, priority);
        }
    }

    /// <summary>
    /// Represents a cancellable event.
    /// </summary>
    public interface ICancellableEvent
    {
        /// <summary>
        /// Gets or sets whether the event is cancelled.
        /// </summary>
        bool Cancelled { get; set; }
    }

    /// <summary>
    /// A helper class for events.
    /// </summary>
    public abstract class FreneticEventHelper
    {

        /// <summary>
        /// Start an asynchronous action.
        /// </summary>
        /// <param name="act">The relevant action.</param>
        public abstract void StartAsync(Action act);

        /// <summary>
        /// Schedules a synchronous action.
        /// </summary>
        /// <param name="act">The relevant action.</param>
        public abstract void ScheduleSync(Action act);

        /// <summary>
        /// Schedules a synchronous action.
        /// </summary>
        /// <param name="act">The relevant action.</param>
        /// <param name="delay">The delay before starting.</param>
        public abstract void ScheduleSync(Action act, double delay);
    }
    
    /// <summary>
    /// Represents the arguments to an event.
    /// </summary>
    public class FreneticEventArgs<T> where T: EventArgs
    {
        /// <summary>
        /// The priority value of this firing. Not valid after a wait instruction.
        /// </summary>
        public double PriorityPosition;

        /// <summary>
        /// Helper for various usages, primarily scheduling.
        /// </summary>
        public FreneticEventHelper Helper;

        /// <summary>
        /// The contextual arguments to this event.
        /// </summary>
        public T Context;
    }

    /// <summary>
    /// Helper to wait inside events.
    /// </summary>
    public class FreneticEventWaiter : IDisposable
    {
        /// <summary>
        /// Helper for various usages, primarily scheduling.
        /// </summary>
        public FreneticEventHelper Helper;

        /// <summary>
        /// Whether this waiter has been used to create a wait.
        /// </summary>
        public bool Used = false;

        /// <summary>
        /// The marker for completetion of the waiter.
        /// </summary>
        public ManualResetEvent MRECompletion = new ManualResetEvent(false);

        /// <summary>
        /// Locked until the first pass of the wait run is complete.
        /// </summary>
        public ManualResetEvent MREFirst = new ManualResetEvent(false);

        /// <summary>
        /// Locked until the wait is entirely complete.
        /// </summary>
        public ManualResetEvent MREFinalComplete = new ManualResetEvent(false);

        /// <summary>
        /// Waits for a delay in seconds.
        /// </summary>
        /// <param name="delay">The delay, in seconds.</param>
        public void Wait(double delay)
        {
            Used = true;
            ManualResetEvent mre = new ManualResetEvent(false);
            MREFirst.Set();
            MRECompletion.Set();
            Helper.ScheduleSync(() =>
            {
                MRECompletion.Reset();
                mre.Set();
                MRECompletion.WaitOne();
            }, delay);
            mre.WaitOne();
        }

        /// <summary>
        /// Waits for an MRE to be set - the runs at the next frame tick.
        /// Waits minimum one frame.
        /// </summary>
        /// <param name="mre"></param>
        public void WaitFor(ManualResetEvent mre)
        {
            Used = true;
            MREFirst.Set();
            MRECompletion.Set();
            mre.WaitOne();
            Wait(0);
        }

        /// <summary>
        /// Whether to automatically complete this waiter at the end of the event system.
        /// If turned to false, must call <see cref="Complete"/> to end the waiter when the event is done.
        /// </summary>
        public bool AutoComplete = true;

        /// <summary>
        /// Marks the Waiter complete. MUST be run if waiting is ever used! Called automatically by the event system.
        /// </summary>
        public void Complete()
        {
            MREFirst.Set();
            MRECompletion.Set();
            MREFinalComplete.Set();
        }

        /// <summary>
        /// Screw microsoft.
        /// </summary>
        /// <param name="disposing">If you input false, screw you.</param>
        protected virtual void Dispose(bool disposing)
        {
            // Useless if check
            if (!disposing)
            {
                return;
            }
            // Actual dispose
            MREFirst.Dispose();
            MRECompletion.Dispose();
            MREFinalComplete.Dispose();
        }

        /// <summary>
        /// Dispose and destroy the event.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
    }

    /// <summary>
    /// Helper to fire an event.
    /// </summary>
    public class FreneticEventFirer<T> where T: EventArgs
    {
        /// <summary>
        /// Used to fire the event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        public FreneticEventWaiter Fire(FreneticEventArgs<T> args)
        {
            if (FireAction != null)
            {
                return FireNoWait(args);
            }
            return FireWait(args);
        }
        
        /// <summary>
        /// The action used to fire the FreneticEventArgs without a water.
        /// </summary>
        public Action<FreneticEventArgs<T>> FireAction;

        /// <summary>
        /// The action used to fire the FreneticEventArgs with a waiter.
        /// </summary>
        public Action<FreneticEventArgs<T>, FreneticEventWaiter> FireWaiter;
        
        /// <summary>
        /// Constructs the firer without a waitable option.
        /// </summary>
        /// <param name="noWaitEvent">The standard event.</param>
        public FreneticEventFirer(Action<FreneticEventArgs<T>> noWaitEvent)
        {
            FireAction = noWaitEvent;
        }

        /// <summary>
        /// Constructs the firer with a waitable option.
        /// </summary>
        /// <param name="waitedEvent">The waiter.</param>
        public FreneticEventFirer(Action<FreneticEventArgs<T>, FreneticEventWaiter> waitedEvent)
        {
            FireWaiter = waitedEvent;
        }

        /// <summary>
        /// Fires the event, not returning a waiter.
        /// </summary>
        /// <param name="fea">The arguments to the event.</param>
        /// <returns>No waiter.</returns>
        public FreneticEventWaiter FireNoWait(FreneticEventArgs<T> fea)
        {
            FireAction(fea);
            return null;
        }

        /// <summary>
        /// Fires the event, potentially returning a waiter.
        /// </summary>
        /// <param name="fea">The arguments to the event.</param>
        /// <returns>A waiter if needed.</returns>
        public FreneticEventWaiter FireWait(FreneticEventArgs<T> fea)
        {
            FreneticEventWaiter few = new FreneticEventWaiter() { Helper = fea.Helper };
            fea.Helper.StartAsync(() =>
            {
                FireWaiter(fea, few);
                few.MREFirst.Set();
                if (few.Used && few.AutoComplete)
                {
                    few.Complete();
                }
            });
            few.MREFirst.WaitOne();
            if (few.Used)
            {
                return few;
            }
            return null;
        }
    }
}
