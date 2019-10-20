using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SerjTm.Lifehack.Chat
{
    public class Cancel : IDisposable
    {
        public Cancel()
        {
            this.TokenSource = new CancellationTokenSource();
            this.Task = CancellationTokenAsTask(this.TokenSource.Token);
        }
        public readonly CancellationTokenSource TokenSource;
        public CancellationToken Token => TokenSource.Token;
        public readonly Task Task;


        public void Dispose()
        {
            TokenSource.Dispose();
            Task.Dispose();
        }

        public static Task CancellationTokenAsTask(CancellationToken token)
        {
            var tcs = new TaskCompletionSource<bool>();
            token.Register(s => ((TaskCompletionSource<bool>)s).SetException(new OperationCanceledException()), tcs);
            return tcs.Task;
        }
    }

    public static class CancelHlp
    {
        public static async Task<T> OrCancel<T>(this Task<T> task, Cancel cancel)
        {
            var completedTask = await Task.WhenAny(task, cancel.Task);
            var resultTask = completedTask as Task<T>;
            if (resultTask != null)
                return resultTask.Result;
            throw completedTask.Exception as Exception ?? new InvalidOperationException("invalid cancel result");
        }
    }
}
