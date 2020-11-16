using System;
using proj_b;

namespace proj_a
{
    class Program
    {
        static void Main(string[] args)
        {
            var c = new Class{
                Message = "repro?!"
            };
            Console.WriteLine(c.GetMessage());
        }
    }
}
