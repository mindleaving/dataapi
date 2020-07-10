using System.Collections.Concurrent;
using System.Collections.Generic;
using Commons.Extensions;

namespace DataProcessing
{
    public class TaskDatabase
    {
        public TaskDatabase()
        {
        }

        public TaskDatabase(IEnumerable<ITask> tasks)
        {
            tasks.ForEach(Add);
        }

        private readonly ConcurrentBag<ITask> tasks = new ConcurrentBag<ITask>();
        public IEnumerable<ITask> Tasks => tasks;

        public void Add(ITask task)
        {
            tasks.Add(task);
        }
    }
}
