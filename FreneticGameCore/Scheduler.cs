using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreneticGameCore
{
    public class Scheduler
    {
        public LockedLinkedList<SyncScheduleItem> Tasks = new LockedLinkedList<SyncScheduleItem>();

        public SyncScheduleItem GetSyncTask(Action act, double delay = 0)
        {
            return new SyncScheduleItem() { MyAction = act, Time = delay, OwningEngine = this };
        }

        public SyncScheduleItem ScheduleSyncTask(Action act, double delay = 0)
        {
            SyncScheduleItem item = new SyncScheduleItem() { MyAction = act, Time = delay, OwningEngine = this };
            Tasks.AddAtEnd(item);
            return item;
        }

        public void RunAllSyncTasks(double time)
        {
            LockedLinkedList<SyncScheduleItem>.Node node = Tasks.First;
            while (node != null)
            {
                node.Data.Time -= time;
                if (node.Data.Time > 0)
                {
                    node = node.Next;
                    continue;
                }
                try
                {
                    node.Data.MyAction.Invoke();
                }
                catch (Exception ex)
                {
                    if (ex is ThreadAbortException)
                    {
                        throw ex;
                    }
                    SysConsole.Output("Handling sync task", ex);
                }
                LockedLinkedList<SyncScheduleItem>.Node torem = node;
                node = node.Next;
                Tasks.Remove(torem);
            }
        }

        public ASyncScheduleItem StartAsyncTask(Action a)
        {
            ASyncScheduleItem asyncer = new ASyncScheduleItem() { OwningEngine = this, MyAction = a };
            asyncer.RunMe();
            return asyncer;
        }

        public ASyncScheduleItem AddAsyncTask(Action a, ASyncScheduleItem followUp = null)
        {
            ASyncScheduleItem asyncer = new ASyncScheduleItem() { OwningEngine = this, MyAction = a, FollowUp = followUp };
            return asyncer;
        }
    }

    public abstract class ScheduleItem
    {
        public abstract void RunMe();

        public Scheduler OwningEngine;
    }

    public class SyncScheduleItem : ScheduleItem
    {
        public Action MyAction;

        public double Time = 0;

        public override void RunMe()
        {
            OwningEngine.ScheduleSyncTask(MyAction);
        }
    }

    public class ASyncScheduleItem : ScheduleItem
    {
        public Action MyAction;

        public ASyncScheduleItem FollowUp = null;

        Object Locker = new Object();

        public bool Started = false;

        bool Done = false;

        public bool HasStarted()
        {
            lock (Locker)
            {
                return Started;
            }
        }

        public bool IsDone()
        {
            lock (Locker)
            {
                return Done;
            }
        }

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
            Task.Factory.StartNew(RunInternal);
        }

        private void RunInternal()
        {
            MyAction.Invoke();
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
