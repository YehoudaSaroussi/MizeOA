internal sealed class SlidingWindowRateLimiter
{
    private readonly RateLimit _rateLimit;
    private readonly Queue<DateTime> _timestamps = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public SlidingWindowRateLimiter(RateLimit rateLimit)
    {
        _rateLimit = rateLimit ?? throw new ArgumentNullException(nameof(rateLimit));
    }

    public async Task<TimeSpan> GetRequiredDelayAsync(DateTime nowUtc)
    {
        await _semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            CleanupOldTimestamps(nowUtc);

            if (_timestamps.Count < _rateLimit.MaxRequests)
                return TimeSpan.Zero;

            var earliest = _timestamps.Peek();
            var windowEnd = earliest + _rateLimit.Window;
            var delay = windowEnd - nowUtc;
            return delay > TimeSpan.Zero ? delay : TimeSpan.Zero;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task RecordRequestAsync(DateTime nowUtc)
    {
        await _semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            CleanupOldTimestamps(nowUtc);
            _timestamps.Enqueue(nowUtc);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private void CleanupOldTimestamps(DateTime nowUtc)
    {
        while (_timestamps.Count > 0 && nowUtc - _timestamps.Peek() >= _rateLimit.Window)
        {
            _timestamps.Dequeue();
        }
    }
}
