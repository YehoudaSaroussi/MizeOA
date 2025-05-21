public sealed class RateLimiter<TArg>
{
    private readonly Func<TArg, Task> _action;
    private readonly SlidingWindowRateLimiter[] _limiters;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public RateLimiter(Func<TArg, Task> action, IEnumerable<RateLimit> rateLimits)
    {
        _action = action ?? throw new ArgumentNullException(nameof(action));
        if (rateLimits == null)
            throw new ArgumentNullException(nameof(rateLimits));

        var limits = rateLimits.ToArray();
        if (limits.Length == 0)
            throw new ArgumentException("At least one rate limit must be specified.", nameof(rateLimits));

        _limiters = limits.Select(l => new SlidingWindowRateLimiter(l)).ToArray();
    }

    public async Task ExecuteAsync(TArg arg)
    {
        await _semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            while (true)
            {
                var nowUtc = DateTime.UtcNow;

                var delays = await Task.WhenAll(_limiters.Select(l => l.GetRequiredDelayAsync(nowUtc)));
                var maxDelay = delays.Max();

                if (maxDelay == TimeSpan.Zero)
                {
                    foreach (var limiter in _limiters)
                        await limiter.RecordRequestAsync(nowUtc).ConfigureAwait(false);

                    await _action(arg).ConfigureAwait(false);
                    return;
                }

                await Task.Delay(maxDelay).ConfigureAwait(false);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
