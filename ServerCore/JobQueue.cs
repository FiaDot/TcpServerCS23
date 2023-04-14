using System;
namespace ServerCore
{
	public interface IJobQueue
	{
		void Push(Action job);
	}

	public class JobQueue : IJobQueue
	{
		Queue<Action> _jobQueue = new Queue<Action>();
		object _lock = new object();

        public void Push(Action job)
        {			
			lock(_lock)
			{
				_jobQueue.Enqueue(job);
			}
        }

		Action? Pop()
		{
			lock(_lock)
			{
				if ( _jobQueue.Count == 0 )
				{
					return null;
				}

				return _jobQueue.Dequeue();
			}
		}
    }
}

