using System;
using System.Threading.Tasks;


namespace FunctionalCore
{

    /// <summary>
    /// Result type with acceptance of two kind of parameters:
    /// - Successful: The function has ended with a success.
    /// - Failure: The function has ended with an error, fail or unsuccess result. Commonly, it have an structure of exception type:
    ///         - Exception message.
    ///         - Exception code.
    ///         - Exception data compilation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="E"></typeparam>
    public class Result<T, E>
    {

        /// <summary>
        /// Creator: With data type. Internal parameter Success is true and have some T Data.
        /// </summary>
        /// <param name="data"></param>
        public Result(T data) => Successful = (true, data);

        /// <summary>
        /// Creator: With Error type. Internal parameter Failure is true. 
        /// </summary>
        /// <param name="data"></param>
        public Result(E data) => Failure = (true, data);

        public (bool, T) Successful { get; }
        public (bool, E) Failure { get; }
    }

    public static class Extension
    {
        /// <summary>
        /// Functor MAP:
        ///     Evaluates a Result:
        ///         º onSuccess(T)->(U)
        ///         º onFailure(E)->(E)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <typeparam name="E"></typeparam>
        /// <param name="result"></param>
        /// <param name="onSuccess"></param>
        /// <param name="onFailure"></param>
        /// <returns></returns>
        public static Result<U, E> Map<T, U, E>(this Result<T, E> result, Func<T, U> onSuccess, Func<E, E> onFailure)
        {
            return result.Failure.Item1
                ? new Result<U, E>(onFailure(result.Failure.Item2))
                : new Result<U, E>(onSuccess(result.Successful.Item2));
        }

        /// <summary>
        /// Functor FlatMAP
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="E"></typeparam>
        /// <param name="result"></param>
        /// <param name="onSuccess"></param>
        /// <param name="onFailure"></param>
        /// <returns></returns>
        public static Result<U, E> FlatMap<T, U, E>(this Result<T, E> result, Func<T, Result<U, E>> onSuccess, Func<E, Result<U, E>> onFailure)
        {
            return result.Failure.Item1
                ? onFailure(result.Failure.Item2)
                : onSuccess(result.Successful.Item2);
        }

        /// <summary>
        /// Composition of generic functions of two parameters.
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <typeparam name="B"></typeparam>
        /// <typeparam name="C"></typeparam>
        /// <param name="f"></param>
        /// <param name="g"></param>
        /// <returns></returns>
        public static Func<A, C> Compose<A, B, C>(Func<A, B> f, Func<B, C> g)
        {
            return x => g(f(x));
        }

        /// <summary>
        /// The same as composition BUT as extension of a Function Func(A)->(B)
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <typeparam name="B"></typeparam>
        /// <typeparam name="C"></typeparam>
        /// <param name="f"></param>
        /// <param name="g"></param>
        /// <returns></returns>
        public static Func<A, C> Then<A, B, C>(this Func<A, B> f, Func<B, C> g)
        {
            return x => g(f(x));
        }

        public static Result<(A, B), E> Zip<A, B, E>(Result<A, E> ra, Result<B, E> rb)
        {
            return ra.Successful.Item1
                    ? rb.Successful.Item1
                    ? new Result<(A, B), E>((ra.Successful.Item2, rb.Successful.Item2))
                    : new Result<(A, B), E>(rb.Failure.Item2)
                    : new Result<(A, B), E>(ra.Failure.Item2);
        }

        public static Func<A,Result<C, E>> FlatThen<A, B, C, E>(this Func<A, Result<B, E>> f, Func<B, Result<C, E>> g, Func<E, Result<C, E>> onFailure)
        {
            return x => f(x).FlatMap(g,onFailure);
        }
    }


    public static class ExtensionFuture
    {
        /// <summary>
        /// Definition of RunAction.
        /// RunAction is a callback box. f(Result(T,E)) -> ()
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="E"></typeparam>
        /// <param name="callback"></param>
        public delegate void RunAction<T,E>(Action<Result<T,E>> callback);

        public struct FutureResult<T, E>
        {
            public RunAction<T, E> Run { get; }

            public FutureResult(RunAction<T, E> action) => Run = action;
        }

        public static FutureResult<U, E> Map<T, U, E>(this FutureResult<T, E> t, Func<T,U> f, Func<E,E> e)
        {
            return new FutureResult<U, E>(
                new RunAction<U, E>(
                    (callback)=>{
                        void act(Result<T, E> result) => callback(result.Map(f, e));
                        t.Run(act);
                    }
                )
            );
        }

        public static FutureResult<U, E> Map<T, U, E>(this FutureResult<T, E> t, Func<T, Result<U, E>> f, Func<E,Result<U,E>> e)
        {
            return new FutureResult<U, E>(
                new RunAction<U, E>(
                    (callback) => {
                        void act(Result<T, E> result) => callback(result.FlatMap(f, e));

                        t.Run(act);
                    }
                )
            );
        }

        public static FutureResult<U, E> FlatMap<T, U, E>(this FutureResult<T, E> t, Func<T, FutureResult<U, E>> f)
        {
            return new FutureResult<U, E>(
                new RunAction<U, E>(
                    (callback) => {
                        void act(Result<T, E> result) {
                            if (result.Successful.Item1)
                            {
                                f(result.Successful.Item2).Run(callback);
                            }
                            else {
                                callback(new Result<U,E>(result.Failure.Item2));
                            }
                        }
                        t.Run(act);
                    }
                )
            );
        }

        public static FutureResult<T, E> Retry<T, E>(this FutureResult<T, E> t, int upTo) {
            return new FutureResult<T, E>(
                new RunAction<T, E>(
                    (callback) =>
                    {
                        void tryFuture(FutureResult<T,E> f, int remaining) {

                            void act(Result<T,E> result) {
                                if (result.Successful.Item1)
                                {
                                    callback(new Result<T, E>(result.Successful.Item2));
                                }
                                else {
                                    if (remaining > 0)
                                    {
                                        Console.WriteLine($"Retry n: {upTo-remaining}");
                                        Console.WriteLine(result.Failure.Item2);
                                        tryFuture(f, remaining - 1);
                                    }
                                    else {
                                        callback(new Result<T, E>(result.Failure.Item2));
                                    }
                                }
                            }
                            f.Run(act);
                        }
                        tryFuture(t, upTo);
                    }
                )
            );
        }

        public static FutureResult<(A, B), E> Zip<A, B, E>(FutureResult<A, E> fa, FutureResult<B, E> fb) {
            return new FutureResult<(A, B), E>(
                (callback) =>
                {
                    Result<A, E> ra = null;
                    Result<B, E> rb = null;

                    void actA(Result<A, E> result) => ra = result;
                    void actB(Result<B, E> result) => rb = result;

                    void actAA() => fa.Run(actA);

                    void actAB() => fb.Run(actB);

                    Task t = Task.WhenAll(Task.Run(actAA), Task.Run(actAB)).ContinueWith(x => { callback(Extension.Zip(ra, rb));});
                    try
                    {
                        t.Wait();
                    }
                    catch { }
                }
            );
        }
    }

    /*public static class ExtensionTask
    {
        public static Task<Result<U, E>> Map<U, T, E>(this Task<Result<T,E>> t, Func<T, U> f)
        {
            return new Task<Result<U, E>>(() => t.Result.Map(f, null));
        }

        public static Task<Result<U,E>> Map<U,T,E>(this Task<Result<T,E>> t, Func<T,Result<U,E>> f)
        {
            return new Task<Result<U, E>>(() => t.Result.FlatMap(f,null));
        }

        public static Task<Result<U, E>> FlatMap<U, T, E>(this Task<Result<T, E>> t, Func<T, Task<Result<U,E>>> f)
        {
            Result<T, E> r = t.Result;
            return r.Successful.Item1
                ? new Task<Result<U, E>>(() => f(r.Successful.Item2).Result)
                : new Task<Result<U, E>>(() => new Result<U,E>(r.Failure.Item2));
        }

        public static Task<Result<T, E>> Retry<T, E>(this Task<Result<T, E>> t, int upTo)
        {
            return new Task<Result<T, E>>(() =>
              {
                  TryTask(t, upTo);
                  return t.Result;
              }
            ) ;

            void TryTask(Task<Result<T, E>> t, int remaining)
            {
                if (t.Result.Failure.Item1)
                {
                    Console.WriteLine($"Failure: Attempt number {upTo - remaining}");
                    if (remaining > 0)
                    {
                        TryTask(t, remaining - 1);
                    }
                }
            }
        }
    }*/
}
