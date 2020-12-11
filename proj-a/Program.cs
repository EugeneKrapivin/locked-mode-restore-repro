using System;
using proj_b;

namespace proj_a
{
    class Program
    {
        static void Main(string[] args)
        {
            var c = new Class{
                Message = "Did we make it?!"
            };
            Console.WriteLine(c.GetMessage());
        }
    }
}
