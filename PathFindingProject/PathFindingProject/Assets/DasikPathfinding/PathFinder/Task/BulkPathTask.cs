using Dasik.PathFinder;
using Dasik.PathFinder.Task;
using System.Collections.Generic;

namespace Assets.DasikPathfinding.PathFinder.Task
{
	public class BulkPathTask<T> : AbstractPathTask<IDictionary<T, IEnumerable<Cell>>>
	{
		internal BulkPathTask(long taskId, PathFinding pathFinding) : base(taskId, pathFinding)
		{
		}
	}
}
