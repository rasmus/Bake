using System;

namespace Bake.ValueObjects
{
    public class CookResult
    {
        public string Name { get; }
        public TimeSpan Time { get; }
        public bool Success { get; }

        public CookResult(
            string name,
            TimeSpan time,
            bool success)
        {
            Name = name;
            Time = time;
            Success = success;
        }
    }
}
