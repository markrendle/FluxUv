namespace FluxUv
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal class Pool<T> where T : IPoolObject
    {
        private Func<T> _creator;
        private readonly ConcurrentStack<T> _pool;
        private readonly string _tName;
        private int _count;

        public Pool()
        {
            _pool = new ConcurrentStack<T>();
            _tName = typeof (T).Name;
        }

        public void Init(IEnumerable<T> items, Func<T> creator)
        {
            var itemArray = items.ToArray();
            _pool.PushRange(itemArray);
            _count = itemArray.Length;
            _creator = creator;
        }

        public T Pop()
        {
            T item;
            if (!_pool.TryPop(out item))
            {
                item = _creator();
                Console.WriteLine("{0} {1} instances", Interlocked.Increment(ref _count), _tName);
            }
            return item;
        }

        public void Push(T item)
        {
            item.Reset();
            _pool.Push(item);
        }
    }
}