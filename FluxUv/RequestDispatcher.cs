namespace FluxUv
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Env = System.Collections.Generic.IDictionary<string,object>;
    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string,object>,System.Threading.Tasks.Task>;

    internal class RequestDispatcher
    {
        private readonly BlockingCollection<Env> _requests = new BlockingCollection<Env>();
        private readonly AppFunc _app;
        private readonly BlockingCollection<FluxEnv> _responses;
        private readonly CancellationToken _token;
        private readonly Action<Task, object> _appContinuation;
        private Task _task;

        public RequestDispatcher(AppFunc app, BlockingCollection<FluxEnv> responses, CancellationToken token)
        {
            _app = app;
            _responses = responses;
            _token = token;
            _appContinuation = AppContinuation;
        }

        public void Dispatch(Env env)
        {
            _requests.Add(env, _token);
        }

        private void AppContinuation(Task task, object state)
        {
            var env = (FluxEnv) state;
            if (_token.IsCancellationRequested) return;
            if (task.IsCanceled) return;
            if (task.IsFaulted)
            {
                Debug.Assert(task.Exception != null);
                env.Exception = task.Exception;
            }
            _responses.Add(env, _token);
        }

        public void Start()
        {
            _task = Task.Run(() =>
            {
                while (!_token.IsCancellationRequested)
                {
                    var env = _requests.Take(_token);
                    _app(env).ContinueWith(_appContinuation, env, _token);
                }
            }, _token);
        }
    }
}