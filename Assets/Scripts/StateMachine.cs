using UnityEngine;

namespace Dasik.PathFinder.Example
{
	public class StateMachine : MonoBehaviour
	{
		public PathManager PathManager;
		public Map Map;

		private States currentState = States.PathFinding;

		// Use this for initialization
		void Start()
		{
			TouchDispatcher.TouchEnded += TouchDispatcherOnTouchEnded;
		}

		private void TouchDispatcherOnTouchEnded(Vector2 position, int fingerId)
		{
			var worldPoint = Camera.main.ScreenToWorldPoint(position);
			switch (currentState)
			{
				case States.PathFinding:
					PathManager.SetPath(worldPoint);
					break;
				case States.AgentsAddition:
					ObjectGenerator.Instance.InitAgent(new Vector2(worldPoint.x, worldPoint.y));
					break;
				case States.ObstaclesAddition:
					ObjectGenerator.Instance.InitPlayerObstacle(new Vector2(worldPoint.x, worldPoint.y));
					Map.ScanArea(worldPoint - Vector3.one * 5, worldPoint + Vector3.one * 5);
					break;
			}

		}

		public void SetState(string state)
		{

			switch (state)
			{
				case "PathFinding":
					currentState = States.PathFinding;
					break;
				case "AgentsAddition":
					currentState = States.AgentsAddition;
					break;
				case "ObstaclesAddition":
					currentState = States.ObstaclesAddition;
					break;
			}
		}


		public enum States
		{
			PathFinding,
			AgentsAddition,
			ObstaclesAddition
		}

	}
}