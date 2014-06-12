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
        private readonly BlockingCollection<Http>[] _requestQueues;
        private readonly Task[] _requestTasks;
        private readonly AppFunc _app;
        private readonly BlockingCollection<Http> _responses;
        private readonly CancellationToken _token;
        private readonly Action<Task, object> _appContinuation;
        private Task _task;

        public RequestDispatcher(AppFunc app, BlockingCollection<Http> responses, CancellationToken token) : this(app, responses, token, 1)
        {
        }

        public RequestDispatcher(AppFunc app, BlockingCollection<Http> responses, CancellationToken token, int threadCount)
        {
            _requestQueues = new BlockingCollection<Http>[threadCount];
            _requestTasks = new Task[threadCount];
            _app = app;
            _responses = responses;
            _token = token;
            _appContinuation = AppContinuation;
        }

        public void Dispatch(Http http)
        {
            BlockingCollection<Http>.AddToAny(_requestQueues, http);
        }

        private void AppContinuation(Task task, object state)
        {
            var http = (Http) state;
            if (_token.IsCancellationRequested) return;
            if (task.IsCanceled) return;
            if (task.IsFaulted)
            {
                Debug.Assert(task.Exception != null);
                http.Exception = task.Exception;
            }
            http.PrepForWrite();
            _responses.Add(http, _token);
        }

        public void Start()
        {
            for (int i = 0; i < _requestQueues.Length; i++)
            {
                var collection = _requestQueues[i] = new BlockingCollection<Http>(256);
                _requestTasks[i] = Task.Run(() =>
                {
                    while (!_token.IsCancellationRequested)
                    {
                        var http = collection.Take(_token);
                        http.ParseEnv();
                        _app(http.Env).ContinueWith(_appContinuation, http, _token);
                    }
                }, _token);
            }
        }
    }
}