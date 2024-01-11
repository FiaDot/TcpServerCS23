using ServerCore;

namespace Server.Game
{
	struct JobTimerElem : IComparable<JobTimerElem>
	{
		public int execTick; // 실행 시간
		public IJob action;

		public int CompareTo(JobTimerElem other)
		{
			// 작은값 먼저 
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
			// 현재 시간에서 + 일정시간(tickAfter) 이후 값이 실행 시간
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

					// 아직 실행 시간 안됐음
					if (job.execTick > now)
						break;

					_pq.Dequeue();
				}

				// TODO : job 없는 경우 있지 않나?
				job.action.Execute();
			}
		}
	}
}
