namespace FluxUv
{
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    internal class Pool<T> where T : IPoolObject, new()
    {
        private readonly ConcurrentStack<T> _pool;

        public Pool() : this(128)
        {
        }

        public Pool(int initialCount)
        {
            _pool = new ConcurrentStack<T>(Initial(initialCount));
        }

        public T Pop()
        {
            T item;
            return _pool.TryPop(out item) ? item : new T();
        }

        public void Push(T item)
        {
            item.Reset();
            _pool.Push(item);
        }

        private static IEnumerable<T> Initial(int count)
        {
            return Enumerable.Repeat(0, count).Select(_ => new T());
        }
    }
}