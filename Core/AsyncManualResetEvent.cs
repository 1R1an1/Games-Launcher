using System.Threading;
using System.Threading.Tasks;

namespace Games_Launcher.Core
{
    public sealed class AsyncManualResetEvent
    {
        private readonly object _mutex = new object();
        private TaskCompletionSource<bool> _tcs;

        public AsyncManualResetEvent(bool initialState = false)
        {
            _tcs = CreateTcs(initialState);
        }

        private static TaskCompletionSource<bool> CreateTcs(bool set)
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            if (set)
                tcs.TrySetResult(true);
            return tcs;
        }

        /// <summary>
        /// Indica si el evento está en estado "set".
        /// </summary>
        public bool IsSet
        {
            get
            {
                lock (_mutex)
                    return _tcs.Task.IsCompleted;
            }
        }

        /// <summary>
        /// Espera de manera asíncrona hasta que el evento sea activado.
        /// </summary>
        public Task WaitAsync()
        {
            lock (_mutex)
            {
                return _tcs.Task;
            }
        }

        /// <summary>
        /// Espera de manera asíncrona con cancelación.
        /// </summary>
        public async Task WaitAsync(CancellationToken cancellationToken)
        {
            Task waitTask;
            lock (_mutex)
            {
                waitTask = _tcs.Task;
            }

            if (waitTask.IsCompleted)
                return;

            using (cancellationToken.Register(() =>
            {
                // si se cancela, el task de cancelación debe completarse
            }))
            {
                var tcs = new TaskCompletionSource<bool>();
                using (cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken)))
                {
                    await Task.WhenAny(waitTask, tcs.Task).ConfigureAwait(false);

                    // si fue cancelado
                    if (cancellationToken.IsCancellationRequested)
                        cancellationToken.ThrowIfCancellationRequested();
                }
            }
        }

        /// <summary>
        /// Espera de manera sincrónica (bloquea el thread).
        /// </summary>
        public void Wait()
        {
            WaitAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Espera sincrónicamente con cancelación.
        /// </summary>
        public void Wait(CancellationToken cancellationToken)
        {
            WaitAsync(cancellationToken).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Activa el evento.
        /// </summary>
        public void Set()
        {
            lock (_mutex)
            {
                _tcs.TrySetResult(true);
            }
        }

        /// <summary>
        /// Reinicia el evento si ya estaba activado.
        /// </summary>
        public void Reset()
        {
            lock (_mutex)
            {
                if (_tcs.Task.IsCompleted)
                    _tcs = CreateTcs(false);
            }
        }
    }
}
