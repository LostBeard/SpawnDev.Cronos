using System;
using Timer = System.Timers.Timer;

namespace SpawnDev.Cronos
{
    /// <summary>
    /// A cron based Timer
    /// </summary>
    public class CronTimer : IDisposable
    {
        /// <summary>
        /// Get or Set the cron interval
        /// </summary>
        public string Interval { get => _Interval; set => SetInterval(value); }
        /// <summary>
        /// Enabled
        /// </summary>
        public bool Enabled { get => _Enabled; set { if (value) Start(); else Stop(); } }
        /// <summary>
        /// Elapsed event
        /// </summary>
        public event EventHandler Elapsed = null;
        private string _Interval { get; set; } = "";
        private bool _Enabled { get; set; } = false;
        private Timer _timer = new Timer();
        /// <summary>
        /// Create a new instance
        /// </summary>
        public CronTimer()
        {
            _timer.Elapsed += _timer_Elapsed;
            _timer.AutoReset = false;
        }
        /// <summary>
        /// Create a new instance
        /// </summary>
        public CronTimer(string interval)
        {
            Interval = interval;
            _timer.Elapsed += _timer_Elapsed;
            _timer.AutoReset = false;
        }
        /// <summary>
        /// Set the cron interval
        /// </summary>
        public void SetInterval(string interval)
        {
            _Interval = interval;
            ResetTimer();
        }
        /// <summary>
        /// Start the Timer
        /// </summary>
        public void Start()
        {
            if (_Enabled) return;
            _Enabled = true;
            ResetTimer();
        }
        /// <summary>
        /// Stop the Timer
        /// </summary>
        public void Stop()
        {
            if (!_Enabled) return;
            _Enabled = false;
            _timer.Enabled = _Enabled;
        }
        /// <inheritdoc/>
        public void Dispose()
        {
            _Enabled = false;
            _timer?.Dispose();
        }
        DateTime? NextOccurrence = null;
        private double? GetTimeUntilNextOccurrenceMS(out bool nextSameAsLast)
        {
            var includeSeconds = Interval.Split(' ').Length == 6;
            var s = CronExpression.Parse(Interval, includeSeconds ? CronFormat.IncludeSeconds : CronFormat.Standard);
            var now = DateTime.UtcNow;
            var nextTime = s.GetNextOccurrence(now);
            nextSameAsLast = nextTime != null && NextOccurrence != null && (nextTime - NextOccurrence) == TimeSpan.Zero;
            NextOccurrence = nextTime;
            if (nextTime == null) return null;
            var timeUntilNext = nextTime - now;
            return timeUntilNext.Value.TotalMilliseconds;
        }
        private bool ResetTimer()
        {
            _timer.Enabled = false;
            var msDelay = GetTimeUntilNextOccurrenceMS(out var nextSameAsLast);
            if (msDelay != null)
            {
                _timer.Interval = msDelay.Value;
            }
            else
            {
                _Enabled = false;
            }
            _timer.Enabled = _Enabled;
            return nextSameAsLast;
        }
        private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var nextSameAsLast = ResetTimer();
            if (nextSameAsLast) return;
            Elapsed?.Invoke(this, null);
        }
    }
}
