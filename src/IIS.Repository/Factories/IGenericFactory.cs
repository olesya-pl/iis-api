using System;

namespace IIS.Repository.Factories
{
    public interface IGenericFactory
    {
        T Create<T>() where T : new();

        T Create<T, TArg>(TArg arg);

        T Create<T>(Func<T> factoryMethod);
    }
}
