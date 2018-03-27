using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Stannieman.DI
{
    public class Container : IContainer
    {
        private readonly Dictionary<Type, object> _singletons = new Dictionary<Type, object>();
        private readonly List<Registration> _registrations = new List<Registration>();
        private readonly object _lock = new object();
        private readonly ContainerConfiguration _configuration;
        private readonly Container _parentContainer;

        public event Action<object> Activated;

        public Container(ContainerConfiguration configuration)
        {
            _configuration = configuration.Clone();
        }

        private Container(Container _container) : this(_container._configuration)
        {
            _parentContainer = _container;
        }

        public void RegisterPerRequest(Type registrationType, Type implementationType, string key = null)
        {
            Register(registrationType, implementationType, false, key);
        }

        public void RegisterSingleton(Type registrationType, Type implementationType, string key = null)
        {
            Register(registrationType, implementationType, true, key);
        }

        private void Register(Type registrationType, Type implementationType, bool registerSingleton, string key)
        {
            lock (_lock)
            {
                if (_registrations.Any(x => x.Key == key && x is TypeRegistration && ((TypeRegistration)x).ImplementationType == implementationType))
                {
                    throw new ContainerException(ContainerErrorCodes.TypeAlreadyRegistered, $"The implementation type {implementationType.FullName} was already registered with a different registration type for the same key {key}.");
                }

                if (registerSingleton)
                {
                    _registrations.Add(new SingletonTypeRegistration(registrationType, key, implementationType));
                    return;
                }

                _registrations.Add(new PerRequestTypeRegistration(registrationType, key, implementationType));
            }
        }

        public void RegisterHandler(Type registrationType, Func<Container, object, object> handler, string key = null)
        {
            lock (_lock)
            {
                _registrations.Add(new HandlerRegistration(registrationType, key, handler));
            }
        }

        public object GetSingleInstance(Type registrationType, string key = null)
        {
            return GetSingleInstance(registrationType, null, key);
        }

        public IEnumerable<object> GetAllInstances(Type registrationType, string key = null)
        {
            return GetAllInstances(registrationType, null, key);
        }

        private object GetSingleInstance(Type registrationType, object parent, string key)
        {
            lock (_lock)
            {
                if (registrationType == null)
                {
                    return null;
                }

                var registrations = _registrations.Where(x => x.RegistrationType == registrationType && x.Key == key);

                var registrationsCount = registrations.Count();
                if (registrationsCount > 1)
                {
                    // More than 1 implementation type registerd for this registration type.
                    // No idea which one to pick.
                    throw new ContainerException(ContainerErrorCodes.MultipleImplementationTypesRegistered, $"Cannot get a single instance for registration type {registrationType.FullName} because multiple implementation types have been registered for this registration type.");
                }

                if (registrationsCount == 1)
                {
                    // There is exactly 1 implementation type registered for this registration type.
                    // This is ok, so construct it.
                    var registration = registrations.First();
                    if (registration is TypeRegistration typeRegistration)
                    {
                        return GetImplementationInstance(typeRegistration);
                    }

                    return ((HandlerRegistration)registration).Handler(this, parent);
                }

                if (typeof(Enumerable).GetTypeInfo().IsAssignableFrom(registrationType.GetTypeInfo()) && registrationType.GetTypeInfo().IsGenericType)
                {
                    var listType = registrationType.GetTypeInfo().GenericTypeArguments[0];
                    return GetAllInstances(listType, parent, key);
                }

                return GetDefault(registrationType);
            }
        }

        private IEnumerable<object> GetAllInstances(Type registrationType, object parent, string key)
        {
            lock (_lock)
            {
                if (registrationType == null)
                {
                    return new object[0];
                }

                var registrations = _registrations.Where(x => x.RegistrationType == registrationType && x.Key == key);
                if (!registrations.Any())
                {
                    return new object[0];
                }

                var typeRegistrations = registrations.OfType<TypeRegistration>();
                var handlerRegistrations = registrations.OfType<HandlerRegistration>();

                var array = Array.CreateInstance(registrationType, registrations.Count());

                var i = 0;
                foreach (var typeRegistration in typeRegistrations)
                {
                    array.SetValue(GetImplementationInstance(typeRegistration), i);
                    i++;
                }
                foreach (var handlerRegistration in handlerRegistrations)
                {
                    array.SetValue(handlerRegistration.Handler(this, parent));
                    i++;
                }

                return array as IEnumerable<object>;
            }
        }

        private object GetImplementationInstance(TypeRegistration registration)
        {
            if (registration is SingletonTypeRegistration)
            {
                if (_singletons.ContainsKey(registration.ImplementationType))
                {
                    return _singletons[registration.ImplementationType];
                }

                var singleton = ConstructInstance(registration.ImplementationType);

                _singletons.Add(registration.ImplementationType, singleton);
                return singleton;
            }

            return ConstructInstance(registration.ImplementationType);
        }

        private object ConstructInstance(Type implementationType)
        {
            var constructorArguments = GetConstructorArguments(implementationType);
            var instance = constructorArguments.Any()
                ? Activator.CreateInstance(implementationType, constructorArguments)
                : Activator.CreateInstance(implementationType);
            InjectProperties(instance);
            Activated?.Invoke(instance);
            return instance;
        }

        private void InjectProperties(object instance)
        {
            if (_configuration.EnablePropertyInjection)
            {
                var injectables = instance.GetType().GetRuntimeProperties().Where(x => x.CanRead && x.CanWrite && x.GetValue(instance) == null);
                foreach (var injectable in injectables)
                {
                    var key = GetKeyFromInjectable(injectable);
                    var injectInstance = GetSingleInstance(injectable.PropertyType, instance, key);
                    injectable.SetValue(instance, injectInstance);
                }
            }
        }

        private object[] GetConstructorArguments(Type implementationType)
        {
            var args = new List<object>();
            var constructor = FindEligibleConstructor(implementationType);

            if (constructor != null)
            {
                args.AddRange(constructor.GetParameters().Select(x =>
                {
                    var key = GetKeyFromInjectable(x);
                    return GetSingleInstance(x.ParameterType, this, key);
                }));
            }

            return args.ToArray();
        }

        private ConstructorInfo FindEligibleConstructor(Type type)
        {
            var publicConstructors = type.GetTypeInfo().DeclaredConstructors.Where(x => x.IsPublic);

            ConstructorInfo candidate = null;
            int candidateParametersCount = -1;

            foreach (var constructor in publicConstructors)
            {
                var parameters = constructor.GetParameters();
                var nParameters = parameters.Length;
                var resolvableParameters = 0;

                if (nParameters < candidateParametersCount)
                {
                    continue;
                }

                foreach (var parameter in constructor.GetParameters())
                {
                    var key = GetKeyFromInjectable(parameter);
                    if (IsGenericEnumerable(parameter.ParameterType))
                    {
                        if (IsRegistered(parameter.ParameterType, key))
                        {
                            resolvableParameters++;
                        }
                    }
                    else
                    {
                        if (IsSingleRegistered(parameter.ParameterType, key))
                        {
                            resolvableParameters++;
                        }
                    }
                }

                if (resolvableParameters == nParameters && nParameters > candidateParametersCount)
                {
                    candidate = constructor;
                    candidateParametersCount = nParameters;
                }
            }

            return candidate;
        }

        private string GetKeyFromInjectable(PropertyInfo property)
        {
            var keyAttribute = property.GetCustomAttribute<DependencyKey>(false);
            if (keyAttribute == null)
            {
                return null;
            }

            return keyAttribute.Key;
        }

        private string GetKeyFromInjectable(ParameterInfo parameter)
        {
            var keyAttribute = parameter.GetCustomAttribute<DependencyKey>(false);
            if (keyAttribute == null)
            {
                return null;
            }

            return keyAttribute.Key;
        }

        public bool IsRegisterd<T>(string key = null)
        {
            return IsRegistered(typeof(T), false, key);
        }

        public bool IsRegistered(Type type, string key = null)
        {
            return IsRegistered(type, false, key);
        }

        public bool IsSingleRegisterd<T>(string key = null)
        {
            return IsRegistered(typeof(T), true, key);
        }

        public bool IsSingleRegistered(Type type, string key = null)
        {
            return IsRegistered(type, true, key);
        }

        public bool IsRegistered(Type type, bool single, string key = null)
        {
            if (single)
            {
                return !IsGenericEnumerable(type) && _registrations.Count(x => x.RegistrationType == type && x.Key == key) == 1;
            }

            return _registrations.Any(x => x.RegistrationType == type)
                || (IsGenericEnumerable(type) && _registrations.Any(x => x.RegistrationType == type.GetTypeInfo().GenericTypeArguments[0]));
        }

        private bool IsGenericEnumerable(Type type)
        {
            return typeof(Enumerable).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo())
                    && type.GetTypeInfo().IsGenericType;
        }

        public Container GetChildContainer()
        {
            lock (_lock)
            {
                return new Container(this);
            }
        }

        private object GetDefault(Type type)
        {
            return type.GetTypeInfo().IsValueType
                ? Activator.CreateInstance(type)
                : null;
        }

        private abstract class Registration
        {
            public Type RegistrationType { get; private set; }
            public string Key { get; private set; }

            public Registration(Type registrationType, string key)
            {
                RegistrationType = registrationType;
                Key = key;
            }
        }

        private abstract class TypeRegistration : Registration
        {
            public Type ImplementationType { get; private set; }

            public TypeRegistration(Type registrationType, string key, Type implementationType) : base(registrationType, key)
            {
                ImplementationType = implementationType;
            }
        }

        private class PerRequestTypeRegistration : TypeRegistration
        {
            public PerRequestTypeRegistration(Type registrationType, string key, Type implementationType) : base(registrationType, key, implementationType) { }
        }

        private class SingletonTypeRegistration : TypeRegistration
        {
            public SingletonTypeRegistration(Type registrationType, string key, Type implementationType) : base(registrationType, key, implementationType) { }
        }

        private class HandlerRegistration : Registration
        {
            public Func<Container, object, object> Handler { get; private set; }

            public HandlerRegistration(Type registrationType, string key, Func<Container, object, object> handler) : base(registrationType, key)
            {
                Handler = handler;
            }
        }
    }
}
