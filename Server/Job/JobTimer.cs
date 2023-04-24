using ServerCore;

namespace Server.Game
{
	struct JobTimerElem : IComparable<JobTimerElem>
	{
		public int execTick; // 실행 시간
		public IJob action;

		public int CompareTo(JobTimerElem other)
		{
			return other.execTick - execTick;
		}
	}

	class JobTimer
	{
		PriorityQueue<JobTimerElem, int> _pq = new PriorityQueue<JobTimerElem, int>();
		object _lock = new object();

		public static JobTimer Instance { get; } = new JobTimer();

		public void Push(IJob action, int tickAfter = 0)
		{
			JobTimerElem job;
			job.execTick = System.Environment.TickCount + tickAfter;
			job.action = action;

			lock (_lock)
			{
				_pq.Enqueue(job, job.execTick);
			}
		}

		public void Flush()
		{
			while (true)
			{
				int now = System.Environment.TickCount;

				JobTimerElem job;

				lock (_lock)
				{
					if (_pq.Count == 0)
						break;

					job = _pq.Peek();
					if (job.execTick > now)
						break;

					_pq.Dequeue();
				}

				job.action.Execute();
			}
		}
	}
}
