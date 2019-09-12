# PathFindingProject
This is my diploma work

## About
- Project uses multithreading. 
- Non-linear grid! You can reduce tiles count without loose accuracy. 
- Simple usage. 
- Work with tiles with different passability.
- Bulk pathfinding. Many start positions, one finish.

### Wiki
* [Examples branch](https://github.com/dasik/PathFindingProject/tree/example) - look at other small projects

### Installing with Unity Package Manager
*(Requires Unity version 2018.3.0b7  or above)*

To install this project as a [Git dependency](https://docs.unity3d.com/Manual/upm-git.html) using the Unity Package Manager,
add the following line to your project's `manifest.json`:

```
"com.dasik.pathfinding": "https://github.com/Dasik/PathFindingProject.git"
```

You will need to have Git installed and available in your system's PATH.

### Installing without Unity Package Manager

Or you can just copy the folder 'Runtime' in your asset scripts folder. 

## How to use

```C#
//scan area
CurrentMap.ScanArea(ScanArea.LeftBottomPoint, ScanArea.RightTopPoint,
  callback:() =>
    {
      //remove some area
      CurrentMap.RemoveArea(RemoveArea.LeftBottomPoint, RemoveArea.RightTopPoint);
    });
    
public class PathManager : MonoBehaviour
{
	public PathFinding PathFinder;
	private BulkPathTask<AgentScript> bulkPathFinderTask;
	private SinglePathTask singlePathFinderTask;
  public AgentScript agent;
	public bool useBulkPathFinding = true;

	public void Update()
	{
// pathfinding can work with bulk operations.    
		if (useBulkPathFinding)
		{
			if (bulkPathFinderTask != null && bulkPathFinderTask.Status == PathTaskStatus.Completed)
			{
				foreach (var path in bulkPathFinderTask.Path)
				{
        //key is some class that can take a path 
					path.Key.ApplyPath(path.Value);
				}
        bulkPathFinderTask.Dispose();
				bulkPathFinderTask = null;
			}
		}
		else
		{
			if (singlePathFinderTask==null)
				return;
			
      if (singlePathFinderTask.Status == PathTaskStatus.Completed)
      {
        agent.ApplyPath(singlePathFinderTask.Path);
        singlePathFinderTask.Dispose();
        singlePathFinderTask = null;
      }	
		}
	}

	public void SetPath(Vector2 targetPoint, double accuracy = 1d)
	{
		if (bulkPathFinderTask != null)
		{
			bulkPathFinderTask.Dispose();
			bulkPathFinderTask = null;
		}

		if (singlePathFinderTask != null)
		{
      singlePathFinderTask.Dispose();
			singlePathFinderTask = null;
		}

		foreach (var item in ObjectGenerator.Instance.Agents)
		{
      //stop moving!
			item.ApplyPath(new List<Cell>());
		}


		if (useBulkPathFinding)
		{
  // generate dictionary (object,position)
			var objectsStartPosition = ObjectGenerator.Instance.Agents.ToDictionary(agent => agent, agent => agent.Position);
  //finding path
			bulkPathFinderTask = PathFinder.GetPathesAsync(objectsStartPosition, targetPoint);
		}
		else
		{
				singlePathFinderTask = PathFinder.GetPathAsync(agent.Position, targetPoint, accuracy);
		}
	}
}
```
