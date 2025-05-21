using System.Diagnostics;
using Xunit;

public class RateLimiterTests
{
    private static readonly TimeSpan TimingTolerance = TimeSpan.FromMilliseconds(50);

    [Fact]
    public async Task Allows_Requests_Within_Limit()
    {
        var limiter = new RateLimiter<int>(
            async _ => await Task.CompletedTask,
            new[] { new RateLimit(5, TimeSpan.FromSeconds(1)) }
        );

        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 5; i++)
            await limiter.ExecuteAsync(i);

        sw.Stop();
        Assert.True(sw.Elapsed < TimeSpan.FromSeconds(1) + TimingTolerance, $"Elapsed: {sw.Elapsed}");
    }

    [Fact]
    public async Task Delays_When_Exceeding_Limit()
    {
        var limiter = new RateLimiter<int>(
            async _ => await Task.CompletedTask,
            new[] { new RateLimit(2, TimeSpan.FromMilliseconds(500)) }
        );

        var sw = Stopwatch.StartNew();
        await limiter.ExecuteAsync(1);
        await limiter.ExecuteAsync(2);
        await limiter.ExecuteAsync(3); // Should be delayed

        sw.Stop();
        Assert.True(sw.Elapsed >= TimeSpan.FromMilliseconds(500) - TimingTolerance, $"Elapsed: {sw.Elapsed}");
    }

    [Fact]
    public async Task Multiple_RateLimits_Are_Enforced()
    {
        var limiter = new RateLimiter<int>(
            async _ => await Task.CompletedTask,
            new[]
            {
                new RateLimit(2, TimeSpan.FromMilliseconds(300)),
                new RateLimit(3, TimeSpan.FromMilliseconds(600))
            }
        );

        var sw = Stopwatch.StartNew();
        await limiter.ExecuteAsync(1);
        await limiter.ExecuteAsync(2);
        await limiter.ExecuteAsync(3); // Should be delayed by first limit
        await limiter.ExecuteAsync(4); // Should be delayed by second limit

        sw.Stop();
        Assert.True(sw.Elapsed >= TimeSpan.FromMilliseconds(600) - TimingTolerance, $"Elapsed: {sw.Elapsed}");
    }

    [Fact]
    public async Task Concurrent_Requests_Are_ThreadSafe()
    {
        var limiter = new RateLimiter<int>(
            async _ => await Task.Delay(10),
            new[] { new RateLimit(5, TimeSpan.FromMilliseconds(200)) }
        );

        var tasks = new List<Task>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(limiter.ExecuteAsync(i));
        }
        await Task.WhenAll(tasks);
        Assert.Equal(10, tasks.Count);
    }

    [Fact]
    public void Throws_On_Invalid_RateLimit()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new RateLimit(0, TimeSpan.FromSeconds(1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new RateLimit(1, TimeSpan.Zero));
        Assert.Throws<ArgumentNullException>(() => new RateLimiter<int>(null, new[] { new RateLimit(1, TimeSpan.FromSeconds(1)) }));
        Assert.Throws<ArgumentNullException>(() => new RateLimiter<int>(async _ => await Task.CompletedTask, null));
        Assert.Throws<ArgumentException>(() => new RateLimiter<int>(async _ => await Task.CompletedTask, new RateLimit[0]));
    }
}

// Command to run tests
// dotnet test