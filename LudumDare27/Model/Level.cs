using System.IO;
using Flai;

namespace WVVW.Model
{
    public class Level
    {
        private readonly World _world;
        private readonly Player _player;
        private readonly GameClock _gameClock;

        public World World
        {
            get { return _world; }
        }

        public Player Player
        {
            get { return _player; }
        }

        public GameClock GameClock
        {
            get { return _gameClock; }
        }

        public Level(World world, int? stage)
        {
            _world = world;
            _player = new Player(_world, stage.HasValue ? _world.GetStageStartIndex(stage.Value) : _world.PlayerSpawnIndex);
            _gameClock = new GameClock();
        }

        public void Initialize()
        {
            foreach (TimeBonus timeBonus in _world.TimeBonuses)
            {
                timeBonus.OnCollected += (time) =>
                {
                    _gameClock.AddTime(time);
                };
            }

            _gameClock.TimeOver += () =>
            {
                _player.Kill();
            };
        }

        public void Update(UpdateContext updateContext)
        {
            _world.Update(updateContext);
            _player.Update(updateContext);

            if (_player.IsAlive)
            {
                _gameClock.Update(updateContext);
            }
        }

        public static Level Load(BinaryReader reader, int? stage)
        {
            return new Level(World.Load(reader), stage);
        }
    }
}
