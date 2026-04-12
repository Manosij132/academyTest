using Academy.Core.Shared;

namespace Academy.Core.Utilities
{
    public static class RetryHelper
    {
       
        public static async Task<T> RetryAsync<T>(Func<Task<T>> action, int maxRetries = 3, int delayMilliseconds = 1000, bool allowCustomExeption =false)
        {
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    return await action(); // return result on success
                }
                catch (CustomException ex)
                {
                    // 🚫 Custom exception — stop retrying immediately
                    throw ex;
                }

                catch (Exception ex)
                {
                    Console.WriteLine($"Attempt {attempt} failed: {ex.Message}");
                    if (attempt == maxRetries)
                        throw; // all attempts failed

                    await Task.Delay(delayMilliseconds);
                }
            }

            throw new InvalidOperationException("Unexpected failure in RetryAsync");
        }
    }
}
