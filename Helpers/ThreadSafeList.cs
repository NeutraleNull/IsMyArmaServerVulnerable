using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IsMyArmaServerVulnerableApi.Helpers
{
    public class ThreadSafeList<T>
    {
        private readonly List<T> _list;
        private readonly SemaphoreSlim _gate;

        public ThreadSafeList()
        {
            _list = new List<T>();
            _gate = new SemaphoreSlim(1);
        }

        public ThreadSafeList(IEnumerable<T> items)
        {
            _list = new List<T>(items);
            _gate = new SemaphoreSlim(1);
        }

        public async Task<List<T>> GetList(CancellationToken cancellationToken)
        {
            await _gate.WaitAsync(cancellationToken);
            var tmp = new List<T>(_list);
            _gate.Release();
            return tmp;
        }

        public async Task AddAsync(T item, CancellationToken cancellationToken)
        {
            await _gate.WaitAsync(cancellationToken);
            _list.Add(item);
            _gate.Release();
        }

        public async Task AddRangeAsync(IEnumerable<T> item, CancellationToken cancellationToken)
        {
            await _gate.WaitAsync(cancellationToken);
            _list.AddRange(item);
            _gate.Release();
        }

        public async Task RemoveItemAsync(T item, CancellationToken cancellationToken)
        {
            await _gate.WaitAsync(cancellationToken);
            _list.Remove(item);
            _gate.Release();
        }

    }
}
