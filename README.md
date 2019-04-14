# PathFindingProject
This is my diploma work

## About
- Project uses multithreading. 
- Non-linear grid! You can reduce tiles count without loose accuracy. 
- Simple usage. 
- Work with tiles with different passability.

## How to use

First, copy the folder 'DasikPathfinding' in your asset scripts folder. Once you have it use pathfinding like this:

```C#
//scan area and save this to memory
CurrentMap.ScanArea(ScanArea.LeftBottomPoint, ScanArea.RightTopPoint,
  callback:() =>
    {
      //remove some area
      CurrentMap.RemoveArea(RemoveArea.LeftBottomPoint, RemoveArea.RightTopPoint);
    });
    
// pathfinding can work with bulk operations. Or try to work:)    
if (useBulkPathFinding)
{
  // generate dictionary (object,position)
  var objectsStartPosition = ObjectGenerator.Instance.Agents.ToDictionary(agent => agent, agent => agent.Position);
  //finding path
  var pathFinderId = PathFinder.GetPathesAsync(objectsStartPosition, targetPoint, 
    (param, pathes) =>
    {
      foreach (var path in pathes)
      {
        //key is some class that can take a path 
        path.Key.ApplyPath(path.Value);
      }
    });
  //saving taskId
  pathFinderIds.Add(pathFinderId);
}
else
{
  foreach (var item in ObjectGenerator.Instance.Agents)
  {
    var pathFinderId = PathFinder.GetPathAsync(item.Position, targetPoint, (o, pathes) =>
    {
      o.ApplyPath(pathes ?? new List<Cell>());
    }, accuracy, item);//item is some class that can take a path 
    pathFinderIds.Add(pathFinderId);
  }
}
```
