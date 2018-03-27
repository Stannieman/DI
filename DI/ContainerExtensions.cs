using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Stannieman.DI
{
    public static class ContainerExtensions
    {
        public static void RegisterPerRequest<TRegistration, TImplementation>(this IContainer container, string key = null)
        {
            container.RegisterPerRequest(typeof(TRegistration), typeof(TImplementation), key);
        }

        public static void RegisterSingleton<TRegistration, TImplementation>(this IContainer container, string key = null)
        {
            container.RegisterSingleton(typeof(TRegistration), typeof(TImplementation), key);
        }

        public static void RegisterHandler<TRegistration>(this IContainer container, Func<IContainer, object, object> handler, string key = null)
        {
            container.RegisterHandler(typeof(TRegistration), handler, key);
        }

        public static void RegisterAllFromAssemblyByBaseTypePerRequest(this IContainer container, Assembly assembly, Type baseType, string key = null)
        {
            GetTypesFromAssemblyByBaseType(assembly, baseType).ForEach(x => container.RegisterPerRequest(baseType, x.AsType(), key));
        }

        public static void RegisterAllFromAssemblyByBaseTypeSingleton(this IContainer container, Assembly assembly, Type baseType, string key = null)
        {
            GetTypesFromAssemblyByBaseType(assembly, baseType).ForEach(x => container.RegisterSingleton(baseType, x.AsType(), key));
        }

        public static void RegisterAllFromAssemblyByConventionPerRequest(this IContainer container, Assembly assembly, string typeNameEnd, string key = null)
        {
            var types = GetTypesFromAssemblyByConvention(assembly, typeNameEnd);
            foreach (var type in types)
            {
                type.ImplementedInterfaces.ForEach(x => container.RegisterPerRequest(x, type.AsType(), key));
                container.RegisterPerRequest(type.AsType(), type.AsType(), key);
            }
        }

        public static void RegisterAllFromAssemblyByConventionSingleton(this IContainer container, Assembly assembly, string typeNameEnd, string key = null)
        {
            var types = GetTypesFromAssemblyByConvention(assembly, typeNameEnd);
            foreach (var type in types)
            {
                type.ImplementedInterfaces.ForEach(x => container.RegisterSingleton(x, type.AsType(), key));
                container.RegisterSingleton(type.AsType(), type.AsType(), key);
            }
        }

        public static void RegisterInstance<TRegistration>(this IContainer container, object instance, string key = null)
        {
            container.RegisterHandler(typeof(TRegistration), (sourceContainer, parameter) => instance, key);
        }

        public static void RegisterInstance(this IContainer container, Type registrationType, object instance, string key = null)
        {
            container.RegisterHandler(registrationType, (sourceContainer, parameter) => instance, key);
        }

        public static TRegistration GetSingleInstance<TRegistration>(this IContainer container, string key = null)
        {
            return (TRegistration)container.GetSingleInstance(typeof(TRegistration), key);
        }

        public static IEnumerable<TRegistration> GetAllInstances<TRegistration>(this IContainer container, string key = null)
        {
            return container.GetAllInstances(typeof(TRegistration), key) as IEnumerable<TRegistration>;
        }

        private static IEnumerable<TypeInfo> GetTypesFromAssemblyByBaseType(Assembly assembly, Type baseType)
        {
            return assembly.DefinedTypes.Where(x => !x.IsInterface && !x.IsAbstract && baseType.GetTypeInfo().IsAssignableFrom(x));
        }

        private static IEnumerable<TypeInfo> GetTypesFromAssemblyByConvention(Assembly assembly, string typeNameEnd)
        {
            return assembly.DefinedTypes.Where(x => !x.IsInterface && !x.IsAbstract && x.Name.EndsWith(typeNameEnd));
        }
    }
}