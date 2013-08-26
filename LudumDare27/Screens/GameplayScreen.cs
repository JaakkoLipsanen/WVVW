using System.IO;
using Flai;
using Flai.Graphics;
using Flai.ScreenManagement;
using Flai.ScreenManagement.Screens;
using Microsoft.Xna.Framework.Input;
using WVVW.Controller;
using WVVW.Model;
using WVVW.View;

namespace WVVW.Screens
{
    public enum GameplayMode
    {
        Stage,
        FullRun,
        Editor
    }

    public class GameplayScreen : GameScreen
    {
        private readonly GameplayMode _gameplayMode;
        private readonly string _levelPath;
        private int _stage;

        private Level _level;
        private LevelRenderer _levelRenderer;
        private LevelController _levelController;
        private float _timeSincePlayerDeath = 0f;
        private float _stageStartTime ;

        public GameplayScreen(string levelPath, int stage)
            : this(levelPath, GameplayMode.Stage, stage)
        {
        }

        public GameplayScreen(string levelPath, GameplayMode gameplayMode)
            : this(levelPath, gameplayMode, 0)
        {
        }

        private GameplayScreen(string levelPath, GameplayMode gameplayMode, int stage)
        {
            _gameplayMode = gameplayMode;
            _levelPath = levelPath;
            _stage = stage;

            this.LoadLevel();
        }

        protected override void LoadContent()
        {
            base.Game.ChangeResolution(WvvwGlobals.ScreenWidth, WvvwGlobals.ScreenHeight);
            _levelRenderer.LoadContent();
            base.ScreenManager.AddScreen(new StartScreen());
        }

        protected override void UnloadContent()
        {
            _levelRenderer.Unload();
        }

        protected override void Update(UpdateContext updateContext, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            if (_gameplayMode == GameplayMode.Editor)
            {
                // If the game is in Editor mode and the user presses Escape,
                // then return to the EditorScreen
                if (updateContext.InputState.IsNewKeyPress(Keys.Escape))
                {
                    LoadingScreen.Load(base.ScreenManager, false, new EditorScreen(_levelPath));
                    return;
                }
            }

            if (this.IsActive)
            {
                if(_gameplayMode == GameplayMode.Stage || _gameplayMode == GameplayMode.FullRun)
                {
                    if (updateContext.InputState.IsNewKeyPress(Keys.Escape))
                    {
                        base.ScreenManager.AddScreen(new PauseScreen(_levelPath));
                    }
                }

                _level.Update(updateContext);
                _levelController.Control(updateContext);
                _levelRenderer.Update(updateContext);

                if (!_level.Player.IsAlive)
                {
                    _timeSincePlayerDeath += updateContext.DeltaSeconds;

                    const float WaitUntilReset = 1f;
                    if (_timeSincePlayerDeath > WaitUntilReset)
                    {
                        if (_gameplayMode == GameplayMode.FullRun)
                        {
                            base.ScreenManager.AddScreen(new LevelSelectScreen(_levelPath));
                            return;
                        }

                        _timeSincePlayerDeath = 0f;
                        this.LoadLevel();
                        base.ScreenManager.AddScreen(new StartScreen());
                    }
                }

                // Shouldn't ever be null but whatever
                if (_level.World.Portal != null && _level.Player.IsAlive)
                {
                    if (_level.World.Portal.Area.Intersects(_level.Player.Area))
                    {
                        ScoreFile scoreFile = ScoreFile.Open();

                        LevelScore levelScore;
                        if (scoreFile.ContainsHash(_level.World.Map.Hash))
                        {
                            levelScore = scoreFile.GetLevelScore(_level.World.Map.Hash);
                        }
                        else
                        {
                            levelScore = new LevelScore(_level.World.Map.Hash, _level.World.Map.StageCount);
                        }

                        float stageTime = _level.GameClock.RemainingTime - _stageStartTime + 10; // 10 == base time
                        if (_gameplayMode != GameplayMode.Editor)
                        {
                            if (levelScore.StageBestTimes[_stage].HasValue)
                            {
                                if (stageTime > levelScore.StageBestTimes[_stage].Value)
                                {
                                    levelScore.StageBestTimes[_stage] = stageTime;
                                }
                            }
                            else
                            {
                                levelScore.StageBestTimes[_stage] = stageTime;
                            }
                        }

                        if (_gameplayMode == GameplayMode.FullRun)
                        {
                            if (_stage == _level.World.Map.StageCount - 1)
                            {
                                if (levelScore.MapBestTime.HasValue)
                                {
                                    if (_level.GameClock.RemainingTime > levelScore.MapBestTime.Value)
                                    {
                                        levelScore.MapBestTime = _level.GameClock.RemainingTime;
                                    }
                                }
                                else
                                {
                                    levelScore.MapBestTime = _level.GameClock.RemainingTime;
                                }

                                base.ScreenManager.AddScreen(new StageCompletedScreen(_levelPath, (_gameplayMode != GameplayMode.Stage)));
                            }
                            else
                            {
                                _level.GameClock.AddTime(10);
                                _stageStartTime = _level.GameClock.RemainingTime;

                                _stage++;
                                _level.World.CreatePortal(_level.World.GetStageStartIndex(_stage + 1));
                            }
                        }
                        else
                        {
                            base.ScreenManager.AddScreen(new StageCompletedScreen(_levelPath, (_gameplayMode != GameplayMode.Stage)));
                        }

                        scoreFile.Save();
                    }
                }
            }
        }

        protected override void Draw(GraphicsContext graphicsContext)
        {
            _levelRenderer.Draw(graphicsContext);
        }

        private void LoadLevel()
        {
            if (_levelRenderer != null && base.IsLoaded)
            {
                _levelRenderer.Unload();
            }

            using (BinaryReader reader = new BinaryReader(File.OpenRead(_levelPath)))
            {
                _level = Level.Load(reader, _stage);
            }

            _level.World.CreatePortal(_level.World.GetStageStartIndex(_stage + 1));

            _level.Initialize();
            _levelRenderer = new LevelRenderer(_level);
            _levelController = new LevelController(_level);

            if (base.IsLoaded)
            {
                _levelRenderer.LoadContent();
            }

            _stageStartTime = _level.GameClock.RemainingTime;
        }
    }
}
