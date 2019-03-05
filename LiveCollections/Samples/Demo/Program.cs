using System;
using CSharp.Collections.Monadic;
using CSharp.Collections.Monadic.Tasks;

namespace Demo {
    class Program {
        public static async Option<string> Concatenate(Option<string> first, Option<string> second) {
            var a = await first;
            var b = await second;
            return a + b;
        }

        public static async Option<int> Add(Option<int> first, Option<int> second) {
            var a = await first;
            Console.WriteLine("Get first");
            var b = await second;
            Console.WriteLine("Get second");
            var result = a + b;
            Console.WriteLine("Calculate");            
            return result;
        }

        public static async Result<int> Add(Choice<int, Exception> first, Choice<int, Exception> second) {
            var a = await first;
            Console.WriteLine("Get first");
            var b = await second;
            Console.WriteLine("Get second");
            var result = a + b;
            Console.WriteLine("Calculate");
            return result;
        }

        static void Main(string[] args) {
            Console.WriteLine("Hello World!");
            Console.WriteLine(Concatenate(Option.Some("Hello "), Option.Some("World")));
            Console.WriteLine();
            Console.WriteLine(Add(Option.None<int>(), Option.Some(3)));
            Console.WriteLine();
            Console.WriteLine(Add(Option.Some(3), Option.None<int>()));
            Console.WriteLine();
            Console.WriteLine(Add(Option.Some(3), Option.Some(4)));
        }
    }
}
