// This code demonstrates a simple rate limiter that allows a specified number of requests
class Program
{
    public static async Task Main()
    {
        var rateLimits = new[]
        {
            new RateLimit(10, TimeSpan.FromSeconds(3)),    // 10 per 3 seconds
            new RateLimit(100, TimeSpan.FromMinutes(1)),   // 100 per minute
            new RateLimit(1000, TimeSpan.FromDays(1)),     // 1000 per day
        };

        var limiter = new RateLimiter<string>(
            async message =>
            {
                Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff}: {message}");
                await Task.Delay(10); // Simulate work
            },
            rateLimits
        );

        // Fire 200 requests in parallel
        var tasks = new Task[200];
        for (int i = 0; i < tasks.Length; i++)
        {
            int requestNumber = i + 1;
            tasks[i] = limiter.ExecuteAsync($"Request {requestNumber}");
        }

        await Task.WhenAll(tasks);
    }
}
