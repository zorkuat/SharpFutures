
using System;
using System.Text.RegularExpressions;

namespace FunctionalCore
{
    public class UXServices
    {
        public Func<int, int> Cuadrado = x => (x * x);
        public Func<byte[], Result<string, Exception>> ToUTF8 = data => new Result<String, Exception>(System.Text.Encoding.UTF8.GetString(data));
        public Func<string, Result<int, Exception>> WordCounter = paragraph => new Result<int, Exception>(paragraph.Split(" ").GetLength(0));
        public Func<string, Result<Uri, Exception>> FirstLink = (input) =>
            {
                Regex rx = new Regex("href=\\\"(http[^\\\"]+)\\\"", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                Match matches = rx.Match(input);
                int ls = matches.Value.Length;
                string link = matches.Value.Substring(6, ls - 7);
                return new Result<Uri, Exception>(new Uri(link));
            };
        public Func<int, Result<int, Exception>> UpTo = x => new Result<int, Exception>(new System.Random().Next(x));
        public Func<(int, int), int> Sum = (x) => x.Item1 + x.Item2;
        public Func<int, Func<int, Result<int, Exception>>> multiplier = (n) => (int x) => ((x % n) == 0)
                                                                                                 ? new Result<int, Exception>(x)
                                                                                                 : new Result<int, Exception>(new Exception($"Invalid {x} % {n} == {x % n}"));
        public Func<int, int> Trace = (i) =>
            {
                Console.WriteLine(i);
                return i;
            };

        public Func<Exception, Exception> Handler = (e) => {
            Console.WriteLine(e);
            return e;
        };

        public Func<Exception, Result<int,Exception>> FlatHandler = (e) => {
            Console.WriteLine(e);
            return new Result<int,Exception>(e);
        };
    }
}
