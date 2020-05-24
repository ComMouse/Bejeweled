using Bejeweled.Utility;

namespace Bejeweled
{
    /// <summary>
    /// Global game manager to enable access to game objects and controllers.
    /// </summary>
    public class GameManager : SingletonBehaviour<GameManager>
    {
        public LevelController Level { get; set; }

        public GemController Gem { get; set; }

        public GemMap GemMap { get; set; }

        public GemActionDirector GemDirector { get; set; }

        public DataBlackboard Blackboard => DataBlackboardContainer.Default?.Blackboard;
    }
}
