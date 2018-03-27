using System;
using System.Collections.Generic;

namespace Stannieman.DI
{
    public interface IContainer
    {
        event Action<object> Activated;

        IEnumerable<object> GetAllInstances(Type registrationType, string key = null);
        Container GetChildContainer();
        object GetSingleInstance(Type registrationType, string key = null);
        bool IsRegisterd<T>(string key = null);
        bool IsRegistered(Type type, string key = null);
        bool IsRegistered(Type type, bool single, string key = null);
        bool IsSingleRegisterd<T>(string key = null);
        bool IsSingleRegistered(Type type, string key = null);
        void RegisterHandler(Type registrationType, Func<Container, object, object> handler, string key = null);
        void RegisterPerRequest(Type registrationType, Type implementationType, string key = null);
        void RegisterSingleton(Type registrationType, Type implementationType, string key = null);
    }
}