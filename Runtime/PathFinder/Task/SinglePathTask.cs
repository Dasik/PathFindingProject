using System.Collections.Generic;
using Assets.DasikPathfinding.PathFinder.Task;

namespace Dasik.PathFinder.Task
{
	public class SinglePathTask : AbstractPathTask<IEnumerable<Cell>>
	{
		internal SinglePathTask(long taskId, PathFinding pathFinding) : base(taskId, pathFinding)
		{
		}
	}
}
