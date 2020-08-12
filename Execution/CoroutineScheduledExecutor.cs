﻿using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using Framework.Asynchronous;
using UnityEngine;
using IAsyncResult = Framework.Asynchronous.IAsyncResult;

namespace Framework.Execution
{
    public class CoroutineScheduledExecutor : AbstractExecutor, IScheduledExecutor
    {

        private readonly ComparerImpl<IDelayTask> _comparer = new ComparerImpl<IDelayTask>();
        private readonly List<IDelayTask> _queue = new List<IDelayTask>();
        private bool _running;

        public CoroutineScheduledExecutor()
        {
        }

        private void Add(IDelayTask task)
        {
            _queue.Add(task);
            _queue.Sort(_comparer);
        }

        private bool Remove(IDelayTask task)
        {
            if (_queue.Remove(task))
            {
                _queue.Sort(_comparer);
                return true;
            }
            return false;
        }

        public void Start()
        {
            if (this._running)
                return;

            this._running = true;

            InterceptableEnumerator ie = new InterceptableEnumerator(DoStart());
            ie.RegisterCatchBlock(e => { this._running = false; });
            Executors.RunOnCoroutineNoReturn(ie);
        }

        protected virtual IEnumerator DoStart()
        {
            while (_running)
            {
                while (_running && (_queue.Count <= 0 || _queue[0].Delay.Ticks > 0))
                {
                    yield return null;
                }

                if (!_running)
                    yield break;

                IDelayTask task = _queue[0];
                _queue.RemoveAt(0);
                task.Run();
            }
        }

        public void Stop()
        {
            if (!this._running)
                return;

            this._running = false;
            List<IDelayTask> list = new List<IDelayTask>(_queue);
            foreach (IDelayTask task in list)
            {
                if (task != null && !task.IsDone)
                    task.Cancel();
            }
            this._queue.Clear();
        }

        protected virtual void Check()
        {
            if (!this._running)
                throw new Exception("The ScheduledExecutor isn't started.");
        }

        public virtual Asynchronous.IAsyncResult Schedule(Action command, long delay)
        {
            return this.Schedule(command, GetTimeSpan(delay).Value);
        }

        public virtual Asynchronous.IAsyncResult Schedule(Action command, TimeSpan delay)
        {
            this.Check();
            return new OneTimeDelayTask(this, command, delay);
        }

        public virtual IAsyncResult<TResult> Schedule<TResult>(Func<TResult> command, long delay)
        {
            return this.Schedule(command, GetTimeSpan(delay).Value);
        }

        public virtual IAsyncResult<TResult> Schedule<TResult>(Func<TResult> command, TimeSpan delay)
        {
            this.Check();
            return new OneTimeDelayTask<TResult>(this, command, delay);
        }

        public virtual Asynchronous.IAsyncResult ScheduleAtFixedRate(Action command, long initialDelay, long period)
        {
            return this.ScheduleAtFixedRate(command, GetTimeSpan(initialDelay).Value, GetTimeSpan(period).Value);
        }

        public virtual Asynchronous.IAsyncResult ScheduleAtFixedRate(Action command, TimeSpan initialDelay, TimeSpan period)
        {
            this.Check();
            return new FixedRateDelayTask(this, command, initialDelay, period);
        }

        public IAsyncResult ScheduleAtFixedRate(Action<long> fixedUpdateCommand , Action endCommand, long? initialDelay, long? period, long? duration)
        {
            return this.ScheduleAtFixedRate(fixedUpdateCommand, endCommand, GetTimeSpan(initialDelay),
                GetTimeSpan(period), GetTimeSpan(duration));
        }

        public IAsyncResult ScheduleAtFixedRate(Action<long> fixedUpdateCommand, Action endCommand, TimeSpan? initialDelay, TimeSpan? period, TimeSpan? duration)
        {
            Check();
            return new DurationFixedRateTask(this, fixedUpdateCommand, endCommand, initialDelay, period, duration);
        }


        public virtual void Dispose()
        {
            this.Stop();
        }

        interface IDelayTask : Asynchronous.IAsyncResult
        {
            TimeSpan Delay { get; }

            void Run();
        }

        private TimeSpan? GetTimeSpan(long? milliseconds)
        {
            if (!milliseconds.HasValue) return null;
            return new TimeSpan(milliseconds.Value * TimeSpan.TicksPerMillisecond);
        }

        class OneTimeDelayTask : AsyncResult, IDelayTask
        {
            private readonly long _startTime;
            private TimeSpan _delay;
            private readonly Action _command;
            private readonly CoroutineScheduledExecutor _executor;

            public OneTimeDelayTask(CoroutineScheduledExecutor executor, Action command, TimeSpan delay)
            {
                this._startTime = (long)(Time.fixedTime * TimeSpan.TicksPerSecond);
                this._delay = delay;
                this._executor = executor;
                this._command = command;
                this._executor.Add(this);
            }

            public virtual TimeSpan Delay => new TimeSpan(_startTime + _delay.Ticks - (long)(Time.fixedTime * TimeSpan.TicksPerSecond));

            public override bool Cancel()
            {
                if (this.IsDone)
                    return false;

                if (!this._executor.Remove(this))
                    return false;

                this.CancellationRequested = true;
                this.SetCancelled();
                return true;
            }

            public virtual void Run()
            {
                try
                {
                    if (this.IsDone)
                        return;

                    if (this.IsCancellationRequested)
                    {
                        this.SetCancelled();
                    }
                    else {
                        _command();
                        this.SetResult();
                    }
                }
                catch (Exception e)
                {
                    this.SetException(e);
                    Log.Warning(e);
                }
            }
        }

        class OneTimeDelayTask<TResult> : AsyncResult<TResult>, IDelayTask
        {
            private readonly long _startTime;
            private TimeSpan _delay;
            private readonly Func<TResult> _command;
            private readonly CoroutineScheduledExecutor _executor;

            public OneTimeDelayTask(CoroutineScheduledExecutor executor, Func<TResult> command, TimeSpan delay)
            {
                this._startTime = (long)(Time.fixedTime * TimeSpan.TicksPerSecond);
                this._delay = delay;
                this._executor = executor;
                this._command = command;
                this._executor.Add(this);
            }

            public virtual TimeSpan Delay => new TimeSpan(_startTime + _delay.Ticks - (long)(Time.fixedTime * TimeSpan.TicksPerSecond));

            public override bool Cancel()
            {
                if (this.IsDone)
                    return false;

                if (!this._executor.Remove(this))
                    return false;

                this.CancellationRequested = true;
                this.SetCancelled();
                return true;
            }

            public virtual void Run()
            {
                try
                {
                    if (this.IsDone)
                        return;

                    if (this.IsCancellationRequested)
                    {
                        this.SetCancelled();
                    }
                    else {
                        this.SetResult(_command());
                    }
                }
                catch (Exception e)
                {
                    this.SetException(e);
                    Log.Warning(e);
                }
            }
        }

        class FixedRateDelayTask : AsyncResult, IDelayTask
        {
            private readonly long _startTime;
            private TimeSpan _initialDelay;
            private TimeSpan _period;
            private readonly CoroutineScheduledExecutor _executor;
            private readonly Action _command;
            private int _count;

            public FixedRateDelayTask(CoroutineScheduledExecutor executor, Action command, TimeSpan initialDelay, TimeSpan period) : base()
            {
                this._startTime = (long)(Time.fixedTime * TimeSpan.TicksPerSecond);
                this._initialDelay = initialDelay;
                this._period = period;
                this._executor = executor;
                this._command = command;
                this._executor.Add(this);
            }

            public virtual TimeSpan Delay => new TimeSpan(_startTime + _initialDelay.Ticks + _period.Ticks * _count - (long)(Time.fixedTime * TimeSpan.TicksPerSecond));

            public override bool Cancel()
            {
                if (this.IsDone)
                    return false;

                this._executor.Remove(this);
                this.CancellationRequested = true;
                this.SetCancelled();
                return true;
            }

            public virtual void Run()
            {
                try
                {
                    if (this.IsDone)
                        return;

                    if (this.IsCancellationRequested)
                    {
                        this.SetCancelled();
                    }
                    else {
                        Interlocked.Increment(ref _count);
                        Debug.Log($"{_count}  {_period.TotalMilliseconds}");
                        this._executor.Add(this);
                        _command();
                    }
                }
                catch (Exception e)
                {
                    Log.Warning(e);
                }
            }
        }

        class DurationFixedRateTask : AsyncResult, IDelayTask
        {
            private readonly long _startTime;
            private TimeSpan _initialDelay;
            private TimeSpan _period;
            private TimeSpan? _duration;
            private TimeSpan _timer;
            private readonly CoroutineScheduledExecutor _executor;
            private readonly Action<long> _fixedUpdateCommand;
            private readonly Action _endCommand;
            private int _count;

            public DurationFixedRateTask(CoroutineScheduledExecutor executor, Action<long> fixedUpdateCommand, Action endCommand, TimeSpan? initialDelay, TimeSpan? period, TimeSpan? duration) : base()
            {
                this._startTime = (long)(Time.fixedTime * TimeSpan.TicksPerSecond);
                this._initialDelay = initialDelay ?? TimeSpan.Zero;
                this._period = period ?? TimeSpan.FromMilliseconds(10);
                this._duration = duration;
                this._executor = executor;
                this._fixedUpdateCommand = fixedUpdateCommand;
                this._endCommand = endCommand;
                this._executor.Add(this);
            }

            public virtual TimeSpan Delay => new TimeSpan(_startTime + _initialDelay.Ticks + _period.Ticks * _count - (long)(Time.fixedTime * TimeSpan.TicksPerSecond));

            public override bool Cancel()
            {
                if (this.IsDone)
                    return false;

                this._executor.Remove(this);
                this.CancellationRequested = true;
                this.SetCancelled();
                return true;
            }

            public virtual void Run()
            {
                try
                {
                    if (this.IsDone)
                        return;

                    if (this.IsCancellationRequested)
                    {
                        this.SetCancelled();
                    }
                    else
                    {
                        if (_count >= 1)
                            _timer += _period;
                        if (_duration.HasValue && _timer >= _duration)
                        {
                            _endCommand();
                            Cancel();
                        }

                        _fixedUpdateCommand((long) _timer.TotalMilliseconds);
                        this._executor.Add(this);
                        Interlocked.Increment(ref _count);
                    }
                }
                catch (Exception e)
                {
                    Log.Warning(e);
                }
            }
        }

        class ComparerImpl<T> : IComparer<T> where T : IDelayTask
        {
            public int Compare(T x, T y)
            {
                if (x.Delay.Ticks == y.Delay.Ticks)
                    return 0;

                return x.Delay.Ticks > y.Delay.Ticks ? 1 : -1;
            }
        }
    }
}
