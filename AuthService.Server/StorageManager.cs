using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AuthService.Server
{
    /// <summary>
    /// Database operating manager (execution flow controller only; without db specific). 
    /// Queue of DB requests (delegates with TDbContainer argument) 
    /// with a single asynchronous execution thread.
    /// Asynchronous Update and synchronized (with wait) Get methods are implemented.
    /// Thread Safe.
    /// </summary>
    /// <typeparam name="TDbContainer">Type of DbContext</typeparam>
    public class StorageManager<TDbContainer> where TDbContainer : IDisposable, new()
    {
        /// <summary>
        /// Internal operation tag: Process Queue Routine
        /// </summary>
        public const string TAG_PROC_QUEUE = "Process Queue Routine";
        /// <summary>
        /// Maximun number of update operations in single transaction
        /// </summary>
        readonly int UpdBlockSize;
        readonly object UpdQueue_Lock = new object();
        readonly Queue<OpItem> UpdQueue = new Queue<OpItem>();
        long _updQueueProcessCount = 0;
        readonly object ProcessQueue_Lock = new object();
        readonly ManualResetEventSlim ResultEvent = new ManualResetEventSlim(false);
        readonly Action<TDbContainer> ActSave;
        readonly Action<Exception, object> ActProcError;

        /// <summary>
        /// Operation item
        /// </summary>
        class OpItem
        {
            /// <summary>
            /// Operation Tag (optional)
            /// </summary>
            public readonly object Tag;
            /// <summary>
            /// Operating flag: executor should waiting for processing of the result
            /// </summary>
            public readonly bool WaitingForProcessResult;
            /// <summary>
            /// DB Update action
            /// </summary>
            public readonly Action<TDbContainer> UpdateMethod;
            /// <summary>
            ///  DB Get function
            /// </summary>
            public readonly Func<TDbContainer, object> GetMethod;
            /// <summary>
            /// Error of execution
            /// </summary>
            public volatile Exception Error = null;
            /// <summary>
            /// Result of <see cref="GetMethod"/> execution
            /// </summary>
            public volatile object Result = null;
            /// <summary>
            /// State flag: request was processed
            /// </summary>
            public volatile bool IsProcessed = false;
            /// <summary>
            /// State flag: result was processed
            /// </summary>
            public volatile bool IsResultProcessed = false;

            OpItem(object tag)
            {
                Tag = tag;
            }

            public OpItem(Action<TDbContainer> update_method, object tag) : this(tag)
            {
                UpdateMethod = update_method;
                WaitingForProcessResult = false;
            }

            public OpItem(Func<TDbContainer, object> get_method, object tag) : this(tag)
            {
                GetMethod = get_method;
                WaitingForProcessResult = true;
            }

            public void SetupResult(object result, Exception error)
            {
                Result = result;
                Error = error;
                IsProcessed = true;
            }
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="actSave">perform save handler: OnSave(DbContext) </param>
        /// <param name="actProcError">errors handler: OnError(Exception, OperationTag)</param>
        /// <param name="updBlockSize"><see cref="UpdBlockSize">update block size</see> value</param>
        public StorageManager(Action<TDbContainer> actSave, Action<Exception, object> actProcError, int updBlockSize = 25)
        {
            ActSave = actSave;
            ActProcError = actProcError;
            UpdBlockSize = updBlockSize;
        }

        /// <summary>
        /// Puts Get request in queue and wait for result of execution
        /// </summary>
        /// <typeparam name="TRes"></typeparam>
        /// <param name="method">get request</param>
        /// <param name="tag">operation tag</param>
        /// <returns></returns>
        public TRes Get<TRes>(Func<TDbContainer, TRes> method, object tag = null)
        {
            var op_item = new OpItem((dbx) => method(dbx), tag);
            try
            {
                lock (UpdQueue_Lock)
                {
                    UpdQueue.Enqueue(op_item);
                }
                LaunchProcessQueue();
                while (!op_item.IsProcessed)
                {
                    ResultEvent.Wait();
                    if (!op_item.IsProcessed)
                        Thread.Sleep(0);
                }
                if (op_item.Error != null) throw op_item.Error;
                return (TRes)op_item.Result;
            }
            finally
            {
                op_item.IsResultProcessed = true;
            }
        }

        /// <summary>
        /// Puts Update request in queue and returns control flow
        /// </summary>
        /// <param name="method"></param>
        /// <param name="tag"></param>
        public void UpdateAsync(Action<TDbContainer> method, object tag = null)
        {
            var proc_count = Interlocked.Read(ref _updQueueProcessCount);
            if (proc_count < 0)
                throw new InvalidOperationException("Negative value of queue process count");
            //
            lock (UpdQueue_Lock)
            {
                UpdQueue.Enqueue(new OpItem(method, tag));
            }
            LaunchProcessQueue();
        }

        /// <summary>
        /// Processing thread launcher
        /// </summary>
        void LaunchProcessQueue()
        {
            // if update process flag is not set then pull process and set flag 
            if (Interlocked.CompareExchange(ref _updQueueProcessCount, 1, 0) == 0)
            {
                Task.Run(() => ProcessQueue());
            }
        }

        /// <summary>
        /// Processing the requests queue
        /// </summary>
        void ProcessQueue()
        {
            try
            {
                lock (ProcessQueue_Lock) // update execution flow lock 
                {
                    // allow to pool yet another process 
                    Interlocked.Decrement(ref _updQueueProcessCount);
                    bool is_empty;
                    do
                    {
                        lock (UpdQueue_Lock)
                            is_empty = (UpdQueue.Count == 0);
                        if (!is_empty)
                        {
                            // update block processing
                            using (var dbx = new TDbContainer())
                            {
                                int upd_cnt = 0;
                                for (int upd_no = 0; upd_no < UpdBlockSize; upd_no++)
                                {
                                    OpItem cur_item = null;
                                    lock (UpdQueue_Lock)
                                        cur_item = (UpdQueue.Count > 0 ? UpdQueue.Dequeue() : null);
                                    if (cur_item == null)
                                        break;
                                    try
                                    {
                                        var m_upd = cur_item.UpdateMethod;
                                        var m_get = cur_item.GetMethod;
                                        if (m_upd != null)
                                        {
                                            m_upd(dbx);
                                            upd_cnt++;
                                        }
                                        else if (m_get != null)
                                        {
                                            if (upd_cnt > 0) ActSave(dbx);
                                            upd_cnt = 0;
                                            cur_item.SetupResult(result: m_get(dbx), error: null);
                                        }
                                        else throw new InvalidOperationException("Wrong Opreration Item");
                                    }
                                    catch (Exception ex)
                                    {
                                        cur_item.SetupResult(result: null, error: ex);
                                        ActProcError(ex, cur_item.Tag);
                                    }
                                    if (cur_item.WaitingForProcessResult)
                                    {
                                        ResultEvent.Set();
                                        while (!cur_item.IsResultProcessed)
                                            Thread.Sleep(0);
                                        ResultEvent.Reset();
                                    }
                                }
                                if (upd_cnt > 0) ActSave(dbx);
                            }
                        }
                    } while (!is_empty);
                }


            }
            catch (Exception ex)
            {
                ActProcError(ex, TAG_PROC_QUEUE);
            }
        }

    }
}

