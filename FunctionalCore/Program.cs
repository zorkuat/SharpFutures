using System;
using static FunctionalCore.ExtensionFuture;



namespace FunctionalCore
{
    class Test {
        public Result<int, Exception> FuncTestStarter()
        {
            return new Result<int, Exception>(8);
        }

        public Result<int, Exception> FuncTestAdd(int i)
        {
            return new Result<int, Exception>(i + 1);
        }

        public Result<int, Exception> FuncTestFlatAdd(Result<int,Exception> r)
        {
            switch (r.Failure.Item1)
            {
                case true:
                    return new Result<int, Exception>(new Exception("Fallo de FlatAdd"));
                case false:
                    return new Result<int, Exception>(r.Successful.Item2 + 1);
            }
        }
    }

    class Funciones
    {
        public Func<int, int> Cuadrado = x => (x * x);
        public Func<int, int> PorDos = x => (x * 2);
    }

    class Program
    {
        static void Main(string[] args)
        {

            /////////////////////////////////////////
            // Declaración de servicios utilizados //
            /////////////////////////////////////////
            SessionServices servicios = new SessionServices();
            UXServices ux = new UXServices();

            ////////////////
            // Parámetros //
            ////////////////
            Uri u1 = new Uri("http://example.com");
            Uri u2 = new Uri("http://apple.com/");


            FutureResult<int, Exception> wcFirstLink(Uri url)
            {
                return servicios.get(url)
                    .Map<byte[], Uri, Exception>(ux.ToUTF8.FlatThen(ux.FirstLink,null), null)
                    .FlatMap(servicios.get)
                    .Map<byte[], int, Exception>(ux.ToUTF8.FlatThen(ux.WordCounter,null).FlatThen(ux.UpTo,null),null);
            }

            FutureResult<int, Exception> f1 = wcFirstLink(u1);
            FutureResult<int, Exception> f2 = wcFirstLink(u2);

            FutureResult<int, Exception> wow = Zip(f1, f2)
                .Map(ux.Sum, ux.Handler)
                .Map(ux.Trace, ux.Handler)
                .Map(ux.multiplier(13), ux.FlatHandler)
                .Retry(1000);

            wow.Run(
                res => {
                    if (res.Successful.Item1)
                    {
                        Console.WriteLine(res.Successful.Item2);
                    }
                    else {
                        Console.WriteLine(res.Failure.Item2);
                    }
                }
            );

        }


    }
}
