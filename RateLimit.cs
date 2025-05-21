
public sealed class RateLimit
{
    public int MaxRequests { get; }
    public TimeSpan Window { get; }

    public RateLimit(int maxRequests, TimeSpan window)
    {
        if (maxRequests <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxRequests), "MaxRequests must be positive.");
        if (window <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(window), "Window must be positive.");

        MaxRequests = maxRequests;
        Window = window;
    }
}