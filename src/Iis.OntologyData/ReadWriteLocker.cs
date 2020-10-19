using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Iis.OntologyData
{
    public class ReadWriteLocker
    {
        ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public event Action OnCommingChanges;

        public T ReadLock<T>(Func<T> func)
        {
            _lock.EnterReadLock();
            try
            {
                return func();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
        public T WriteLock<T>(Func<T> func)
        {
            _lock.EnterWriteLock();
            try
            {
                var result = func();
                if (_lock.RecursiveWriteCount == 1)
                {
                    OnCommingChanges();
                }
                return result;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        public void WriteLock(Action action)
        {
            _lock.EnterWriteLock();
            try
            {
                action();
                if (_lock.RecursiveWriteCount == 1)
                {
                    OnCommingChanges();
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}
