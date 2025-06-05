# MizeTest - Sliding Window Rate Limiter

This project demonstrates a thread-safe, sliding window rate limiter in C# using .NET 7 and xUnit for testing.

## Project Structure

- [`Program.cs`](Program.cs): Example usage of the rate limiter, firing 200 requests in parallel and printing timestamps.
- [`RateLimit.cs`](RateLimit.cs): Defines the `RateLimit` class, representing a maximum number of requests per time window.
- [`SlidingWindowRateLimiter.cs`](SlidingWindowRateLimiter.cs): Implements the sliding window logic for a single rate limit.
- [`RateLimiter.cs`](RateLimiter.cs): Coordinates multiple `SlidingWindowRateLimiter` instances to enforce multiple rate limits at once.
- [`RateLimiterTests.cs`](RateLimiterTests.cs): Contains xUnit tests verifying correct rate limiting, thread safety, and error handling.

## Flow

1. **Define Rate Limits:**  
   Create one or more [`RateLimit`](RateLimit.cs) objects specifying how many requests are allowed per time window.

2. **Create RateLimiter:**  
   Instantiate a [`RateLimiter<T>`](RateLimiter.cs) with an async action and the desired rate limits.

3. **Execute Requests:**  
   Call `ExecuteAsync` for each request. The limiter checks all rate limits and delays execution if any would be exceeded.

4. **Thread Safety:**  
   The implementation uses `SemaphoreSlim` to ensure thread safety for concurrent requests.

5. **Testing:**  
   [`RateLimiterTests`](RateLimiterTests.cs) verifies the limiter's correctness, including timing, concurrency, and error cases.

## Running

- Build and run the project to see the rate limiter in action.
- Run the tests with your preferred .NET test runner (e.g., `dotnet test`).

## Running the Tests

To run the xUnit tests in this project, follow these steps:

1. **Ensure the .NET SDK is installed**  
   Check by running:

   ```
   dotnet --version
   ```

   If not installed, download it from [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download).

2. **Open a terminal in the project directory**

3. **Restore dependencies**

   ```
   dotnet restore
   ```

4. **Run the tests**
   ```
   dotnet test
   ```

Test results will be displayed in the terminal.  
If you use Visual Studio Code with the C# extension, you can also run and view tests using the built-in Test Explorer.

---
