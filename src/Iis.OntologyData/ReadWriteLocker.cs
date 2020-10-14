using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Iis.OntologyData
{
    public class ReadWriteLocker
    {
        ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

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
            _lock.EnterReadLock();
            try
            {
                return func();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        public void WriteLock(Action action)
        {
            _lock.EnterReadLock();
            try
            {
                action();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}
