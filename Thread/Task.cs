using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace LegendaryTools.Systems.Threading
{
    public enum TaskState
    {
        Init,
        Waiting,
        Running,
        Completed,
        Error,
        Aborted
    }

    public enum TaskType
    {
        Action,
        ActionParameterized,
        Function,
        FunctionParameterized
    }

    public class Task
    {
        public delegate void OnTaskCompleteEventHandler(Task task);

        public delegate void OnTaskExceptionEventHandler(Task task);

        public delegate void OnTaskHierarchyCompleteEventHandler(Task task);

        public delegate void OnTaskStartEventHandler(Task task);

        protected static List<Task> AllTasks = new List<Task>();
        protected Action action;

        protected List<Task> childs = new List<Task>();
        protected bool CompleteWasNotified;
        protected bool ExceptionWasNotified;
        protected bool HierarchyCompleteWasNotified;

        protected bool StartWasNotified;
        protected Thread thread;

        protected Task(bool isPooled = true)
        {
            IsPooled = isPooled;

            State = TaskState.Waiting;

            if (!IsPooled)
            {
                thread = new Thread(ThreadWorker);
            }

            AllTasks.Add(this);
        }

        public Task(Action action, bool isPooled = false)
            : this(isPooled)
        {
            this.action = action;
            Type = TaskType.Action;
        }

        public TaskState State { get; protected set; }
        public TaskType Type { get; protected set; }
        public object Arg { get; protected set; }
        public object Result { get; protected set; }
        public Exception Error { get; protected set; }
        public bool IsPooled { get; protected set; }
        public bool IsCompletedHierarchy { get; protected set; }
        public Task Parent { get; protected set; }

        public event OnTaskStartEventHandler OnTaskStart;
        public event OnTaskCompleteEventHandler OnTaskComplete;
        public event OnTaskHierarchyCompleteEventHandler OnTaskHierarchyComplete;
        public event OnTaskExceptionEventHandler OnTaskException;

        /// <summary>
        /// Start this task with a optional param
        /// </summary>
        /// <param name="arg"></param>
        public virtual void Start(object arg = null)
        {
            if (State == TaskState.Waiting)
            {
                Arg = arg;

                if (IsPooled)
                {
                    ThreadPool.QueueUserWorkItem(ThreadWorker, arg);
                }
                else
                {
                    thread.Start(arg);
                }
            }
        }

        /// <summary>
        /// Abort this task
        /// </summary>
        public void Stop()
        {
            State = TaskState.Aborted;

            if (thread != null)
            {
                thread.Abort();
            }
        }

        /// <summary>
        /// Waits for the Task to complete execution.
        /// </summary>
        /// <returns>Return True if successfull, return False if a error or stop </returns>
        public bool Wait()
        {
            while (State != TaskState.Completed)
            {
                if (State == TaskState.Error || State == TaskState.Aborted)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Waits for all childs Tasks to complete execution.
        /// </summary>
        /// <returns>Return True if successfull, return False if a error or stop </returns>
        public bool WaitHierarchy()
        {
            while (!IsCompletedHierarchy)
            {
                if (State == TaskState.Error || State == TaskState.Aborted)
                {
                    return false;
                }
            }

            return true;
        }

        public virtual Task AddChild(Task child)
        {
            if (State == TaskState.Waiting)
            {
                childs.Add(child);
                child.Parent = this;

                return child;
            }

            return child;
        }

        /// <summary>
        /// Reset the entire Task, except WorkFunction (Action, Func). This function make the Task reusable and you can use different param in Start.
        /// </summary>
        /// <param name="recursive">True will reset Task childs also</param>
        public void Reset(bool recursive = true)
        {
            Arg = null;
            Result = null;
            State = TaskState.Waiting;
            StartWasNotified = CompleteWasNotified =
                HierarchyCompleteWasNotified = ExceptionWasNotified = IsCompletedHierarchy = false;
            Error = null;

            if (recursive)
            {
                for (int i = 0; i < childs.Count; i++)
                {
                    childs[i].Reset();
                }
            }
        }

        protected virtual void StartChilds()
        {
            if (childs.Count == 0)
            {
                IsCompletedHierarchy = true;
            }
            else
            {
                for (int i = 0; i < childs.Count; i++)
                {
                    childs[i].Start(Result);
                }
            }

            if (Parent != null)
            {
                Parent.CheckCompletedHierarchy();
            }
        }

        protected virtual void CheckCompletedHierarchy()
        {
            IsCompletedHierarchy = childs.TrueForAll(item => item.IsCompletedHierarchy);
        }

        protected virtual void Check()
        {
            switch (State)
            {
                case TaskState.Completed:

                    if (!CompleteWasNotified)
                    {
                        NotifyCompleted();
                    }

                    if (!HierarchyCompleteWasNotified)
                    {
                        NotifyHierarchyComplete();
                    }

                    break;
                case TaskState.Running:
                    if (!StartWasNotified)
                    {
                        NotifyStart();
                    }

                    break;
                case TaskState.Error:
                    if (!ExceptionWasNotified)
                    {
                        NotifyException();
                    }

                    break;
            }

            for (int i = 0; i < childs.Count; i++)
            {
                childs[i].Check();
            }
        }

        protected virtual void NotifyStart()
        {
            if (State == TaskState.Running)
            {
                if (OnTaskStart != null)
                {
                    OnTaskStart.Invoke(this);
                }

                StartWasNotified = true;
            }
        }

        protected virtual void NotifyCompleted()
        {
            if (State == TaskState.Completed)
            {
                if (OnTaskComplete != null)
                {
                    OnTaskComplete.Invoke(this);
                }

                CompleteWasNotified = true;
            }
        }

        protected virtual void NotifyHierarchyComplete()
        {
            if (IsCompletedHierarchy)
            {
                if (OnTaskHierarchyComplete != null)
                {
                    OnTaskHierarchyComplete.Invoke(this);
                }

                HierarchyCompleteWasNotified = true;
            }
        }

        protected virtual void NotifyException()
        {
            if (State == TaskState.Error)
            {
                if (OnTaskException != null)
                {
                    OnTaskException.Invoke(this);
                }

                ExceptionWasNotified = true;
            }
        }

        protected virtual void ThreadWorker(object param = null)
        {
            Debug.Log("Background ThreadID: " + Thread.CurrentThread.ManagedThreadId);
            thread = Thread.CurrentThread;
            if (State == TaskState.Aborted)
            {
                return;
            }

            State = TaskState.Running;

            try
            {
                WorkerFunction(param);
            }
            catch (Exception ex)
            {
                Exception(ex);
            }

            if (State != TaskState.Error && State != TaskState.Aborted)
            {
                State = TaskState.Completed;
                StartChilds();
            }
        }

        protected virtual void WorkerFunction(object param = null)
        {
            action.Invoke();
        }

        protected virtual void Exception(Exception ex)
        {
            Error = ex;
            State = TaskState.Error;
        }

        /// <summary>
        /// Check all thread and notify when started, completed or error. This function should be called repeatedly in main thread.
        /// </summary>
        public static void Update()
        {
            for (int i = 0; i < AllTasks.Count; i++)
            {
                AllTasks[i].Check();
            }
        }

        /// <summary>
        /// Block current thread until all tasks complete.
        /// </summary>
        /// <param name="tasks">Tasks list</param>
        /// <returns>True if all tasks was completed, faleo if a abort or error happened</returns>
        public static bool WaitAll(List<Task> tasks)
        {
            while (tasks.TrueForAll(item => item.State != TaskState.Completed))
            {
                if (tasks.Any(item => item.State == TaskState.Error || item.State == TaskState.Aborted))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Block current thread until all tasks complete.
        /// </summary>
        /// <param name="tasks">Tasks array</param>
        /// <returns>True if all tasks was completed, false if a abort or error happened</returns>
        public static bool WaitAll(params Task[] tasks)
        {
            while (Array.TrueForAll(tasks, item => item.State != TaskState.Completed))
            {
                if (tasks.Any(item => item.State == TaskState.Error || item.State == TaskState.Aborted))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Block current thread until all tasks complete the entire hierarchy.
        /// </summary>
        /// <param name="tasks">Tasks array</param>
        /// <returns>True if all tasks hierarchy was completed, false if a abort or error happened</returns>
        public static bool WaitHierarchyAll(List<Task> tasks)
        {
            while (tasks.TrueForAll(item => item.IsCompletedHierarchy))
            {
                if (tasks.Any(item => item.State == TaskState.Error || item.State == TaskState.Aborted))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Creates a new Task.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="useThreadPool"></param>
        /// <returns></returns>
        public static Task Create(Action action, bool useThreadPool = true)
        {
            return new Task(action, useThreadPool);
        }

        /// <summary>
        /// Creates a new Task.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="useThreadPool"></param>
        /// <returns></returns>
        public static Task<T> Create<T>(Action<T> action, bool useThreadPool = true)
        {
            return new Task<T>(action, useThreadPool);
        }

        /// <summary>
        /// Creates a new Task.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="useThreadPool"></param>
        /// <returns></returns>
        public static Task<T> Create<T>(Func<T> func, bool useThreadPool = true)
        {
            return new Task<T>(func, useThreadPool);
        }

        /// <summary>
        /// Creates a new Task.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="useThreadPool"></param>
        /// <returns></returns>
        public static Task<TParam, TResult> Create<TParam, TResult>(Func<TParam, TResult> func,
            bool useThreadPool = true)
        {
            return new Task<TParam, TResult>(func, useThreadPool);
        }
    }

    public class Task<T> : Task
    {
        public delegate void OnTask1CompleteEventHandler(T result);

        protected Func<T> func;

        protected Action<T> parameterizedAction;

        public Task(Action<T> parameterizedAction, bool isPooled)
            : base(isPooled)
        {
            this.parameterizedAction = parameterizedAction;
            Type = TaskType.ActionParameterized;
        }

        public Task(Func<T> func, bool isPooled)
            : base(isPooled)
        {
            this.func = func;
            Type = TaskType.Function;
        }

        public event OnTask1CompleteEventHandler OnTaskCompleteResult;

        protected override void NotifyCompleted()
        {
            base.NotifyCompleted();

            if (OnTaskCompleteResult != null && Type == TaskType.Function)
            {
                OnTaskCompleteResult.Invoke((T) Result);
            }
        }

        protected override void WorkerFunction(object param = null)
        {
            switch (Type)
            {
                case TaskType.ActionParameterized:
                    parameterizedAction((T) param);
                    break;
                case TaskType.Function:
                    Result = func.Invoke();
                    break;
            }
        }
    }

    public class Task<TParam, TResult> : Task
    {
        public delegate void OnTask2CompleteEventHandler(TResult result);

        protected Func<TParam, TResult> parameterizedFunc;

        public Task(Func<TParam, TResult> parameterizedFunc, bool isPooled)
            : base(isPooled)
        {
            this.parameterizedFunc = parameterizedFunc;
            Type = TaskType.FunctionParameterized;
        }

        public event OnTask2CompleteEventHandler OnTaskCompleteResult;

        protected override void NotifyCompleted()
        {
            base.NotifyCompleted();

            if (OnTaskCompleteResult != null)
            {
                OnTaskCompleteResult.Invoke((TResult) Result);
            }
        }

        protected override void WorkerFunction(object param = null)
        {
            Result = parameterizedFunc.Invoke((TParam) param);
        }
    }
}