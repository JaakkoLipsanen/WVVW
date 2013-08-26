using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace WVVW
{
    public class LevelScore
    {
        private readonly int _hash;
        private readonly int _stageCount;
        private readonly float?[] _stageBestTimes;
        private float? _wholeMapBestTime;

        public int Hash
        {
            get { return _hash; }
        }

        public int StageCount
        {
            get { return _stageCount; }
        }

        public float? MapBestTime
        {
            get { return _wholeMapBestTime; }
            set { _wholeMapBestTime = value; }
        }

        public float?[] StageBestTimes
        {
            get { return _stageBestTimes; }
        }

         public LevelScore(int hash, int stageCount)
             : this(hash, stageCount, null, new float?[stageCount])
         {
         }

        public LevelScore(int hash, int stageCount, float? wholeMapBestTime, float?[] stageBestTimes)
        {
            _hash = hash;
            _stageCount = stageCount;
            _wholeMapBestTime = wholeMapBestTime;
            _stageBestTimes = stageBestTimes;
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(_hash);
            writer.Write(_stageCount);
            if (_wholeMapBestTime.HasValue)
            {
                writer.Write(true);
                writer.Write(_wholeMapBestTime.Value);
            }
            else
            {
                writer.Write(false);
            }

            for (int i = 0; i < _stageBestTimes.Length; i++)
            {
                if (_stageBestTimes[i].HasValue)
                {
                    writer.Write(true);
                    writer.Write(_stageBestTimes[i].Value);
                }
                else
                {
                    writer.Write(false);
                }
            }
        }

        public static LevelScore Load(BinaryReader reader)
        {
            int hash = reader.ReadInt32();
            int stageCount = reader.ReadInt32();

            float? wholeMapBestTime = reader.ReadBoolean() ? reader.ReadSingle() : (float?)null;
            float?[] stageBestTimes = new float?[stageCount];
            for(int i = 0; i < stageBestTimes.Length; i++)
            {
                stageBestTimes[i] = reader.ReadBoolean() ? reader.ReadSingle() : (float?)null;
            }

            return new LevelScore(hash, stageCount, wholeMapBestTime, stageBestTimes);
        }
    }

    public class ScoreFile
    {
        private static readonly string FilePath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "scores.bin");

        private readonly List<LevelScore> _levelScores = new List<LevelScore>();
        private ScoreFile(List<LevelScore> levelScores)
        {
            _levelScores = levelScores;
        }

        public void Save()
        {
            using (var stream = File.Open(ScoreFile.FilePath, FileMode.Create, FileAccess.Write))
            {
                using(BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(_levelScores.Count);
                    foreach (LevelScore score in _levelScores)
                    {
                        score.Save(writer);
                    }
                }
            }
        }

        public LevelScore GetLevelScore(int hash)
        {
            return _levelScores.First(score => score.Hash == hash);
        }

        public void AddLevelScore(LevelScore levelScore)
        {
            if (!_levelScores.Any(score => (score.Hash == levelScore.Hash)))
            {
                _levelScores.Add(levelScore);
            }
            else
            {
                MessageBox.Show("Level already exists!");
                return;
            }
        }

        public bool ContainsHash(int hash)
        {
            return _levelScores.Any(score => score.Hash == hash);
        }

        public static ScoreFile Open()
        {
            if (File.Exists(ScoreFile.FilePath))
            {
                using (var stream = File.Open(ScoreFile.FilePath, FileMode.Open, FileAccess.Read))
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        int count = reader.ReadInt32();
                        List<LevelScore> scores = new List<LevelScore>(count);
                        for(int i = 0; i < count; i++)
                        {
                            scores.Add(LevelScore.Load(reader));
                        }

                        return new ScoreFile(scores);
                    }
                }
            }
            else
            {
                return new ScoreFile(new List<LevelScore>());
            }
        }
    }
}
