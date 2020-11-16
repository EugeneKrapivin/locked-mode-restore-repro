using System;

namespace proj_b
{
    public class Class
    {
        public string Message { get; set; } = "no message";

        public string GetMessage() => $"[{DateTime.UtcNow:o}] - {Message}";
    }
}
