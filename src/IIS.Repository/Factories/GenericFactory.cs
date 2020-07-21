using System;

namespace IIS.Repository.Factories
{
    public class GenericFactory : IGenericFactory
    {
        public T Create<T>() where T : new()
        {
            return new T();
        }

        public T Create<T>(Func<T> factoryMethod)
        {
            return factoryMethod();
        }

        public T Create<T, TArg>(TArg arg)
        {
            return (T)Activator.CreateInstance(typeof(T), arg);
        }
    }
}
