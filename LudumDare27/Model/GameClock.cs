using System.Globalization;
using Flai;

namespace WVVW.Model
{
    public class GameClock
    {
        public const float InitialTime = 10f; // 10 SECONDS! \o/

        public event GenericEvent TimeOver;

        private float _remainingTime = GameClock.InitialTime;
        public float RemainingTime
        {
            get { return _remainingTime; }
        }

        public string RemainingTimeString
        {
            get { return _remainingTime.ToString("0.00", CultureInfo.InvariantCulture); }
        }

        public bool IsRunning
        {
            get { return _remainingTime > 0; }
        }

        public GameClock()
        {
        }

        public void Update(UpdateContext updateContext)
        {
            if (_remainingTime > 0)
            {
                _remainingTime -= updateContext.DeltaSeconds;
                if (_remainingTime < 0)
                {
                    _remainingTime = 0;
                    this.TimeOver.InvokeIfNotNull();
                }
            }
        }

        public void AddTime(float amount)
        {
            if (_remainingTime > 0)
            {
                _remainingTime += amount;
            }
        }
    }
}
