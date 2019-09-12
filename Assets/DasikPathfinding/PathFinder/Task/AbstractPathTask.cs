using System;

namespace Dasik.PathFinder.Task
{
	public abstract class AbstractPathTask<T> : IDisposable
	{
		public T Path { get; protected set; }

		public PathTaskStatus Status
		{
			get
			{
				lock (statusLock)
				{
					return _status;
				}
			}
			protected set
			{
				lock (statusLock)
				{
					_status = value;
				}
			}
		}
		private PathTaskStatus _status;
		private object statusLock = new object();

		private bool isDisposed = false;
		public bool IsEnded
		{
			get
			{
				return Status == PathTaskStatus.Canceled
					   || Status == PathTaskStatus.Faulted
					   || Status == PathTaskStatus.Completed;
			}
		}

		private long _taskId;
		private PathFinding _pathFinding;

		internal AbstractPathTask(long taskId, PathFinding pathFinding)
		{
			Status = PathTaskStatus.WaitingToRun;
			_taskId = taskId;
			_pathFinding = pathFinding;
		}

		internal virtual void Complete(T path)
		{
			Path = path;
			Status = PathTaskStatus.Completed;
		}

		public virtual T WaitForResult()
		{
			while (!IsEnded) { }

			return Path;
		}

		internal virtual void Run()
		{
			Status = PathTaskStatus.Running;
		}

		public virtual void Stop()
		{
			if (Status == PathTaskStatus.Completed)
				return;
			Status = PathTaskStatus.Canceled;
			CloseThread();
		}

		internal virtual void Fail()
		{
			Status = PathTaskStatus.Faulted;
			CloseThread();
		}

		protected virtual void CloseThread()
		{
			_pathFinding.ClosePathFindingThread(_taskId);
		}



		public virtual void Dispose()
		{
			if (isDisposed)
				return;
			isDisposed = true;
			Stop();
		}
	}

	public enum PathTaskStatus
	{
		WaitingToRun,
		Running,
		Canceled,
		Faulted,
		Completed,
	}
}
