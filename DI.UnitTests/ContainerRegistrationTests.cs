using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stannieman.DI.UnitTests.TestTypes;
using System.Linq;

namespace Stannieman.DI.UnitTests
{
    [TestClass]
    public class ContainerRegistrationTests
    {
        [TestMethod]
        public void RegisterPerRequest_RegistersClassAndReturnsDifferentInstanceEveryTime()
        {
            var target = new Container(new ContainerConfiguration());

            target.RegisterPerRequest(typeof(IType4_1), typeof(Type4_1));

            var instance1 = target.GetSingleInstance(typeof(IType4_1));
            var instance2 = target.GetSingleInstance(typeof(IType4_1));

            Assert.AreNotSame(instance1, instance2);
        }

        [TestMethod]
        public void RegisterSingleton_RegistersClassAndReturnsTheSameInstanceEveryTime()
        {
            var target = new Container(new ContainerConfiguration());

            target.RegisterSingleton(typeof(IType4_1), typeof(Type4_1));

            var instance1 = target.GetSingleInstance(typeof(IType4_1));
            var instance2 = target.GetSingleInstance(typeof(IType4_1));

            Assert.AreSame(instance1, instance2);
        }

        [TestMethod]
        public void GetSingleInstance_ReturnsCorrectType()
        {
            var target = new Container(new ContainerConfiguration());

            target.RegisterPerRequest(typeof(IType4_1), typeof(Type4_1));
            target.RegisterPerRequest(typeof(IType4_2), typeof(Type4_2));

            var instance = target.GetSingleInstance(typeof(IType4_2));

            Assert.IsInstanceOfType(instance, typeof(IType4_2));
            Assert.AreEqual(typeof(Type4_2), instance.GetType());
        }

        [TestMethod]
        public void GetSingleInstance_ReturnsNullIfRegistrationTypeNotFound()
        {
            var target = new Container(new ContainerConfiguration());

            target.RegisterPerRequest(typeof(IType4_1), typeof(Type4_1));

            var instance = target.GetSingleInstance(typeof(IType4_2));

            Assert.IsNull(instance);
        }

        [TestMethod]
        public void GetAllInstances_ReturnsEmptyListIfRegistrationTypeNotFound()
        {
            var target = new Container(new ContainerConfiguration());

            target.RegisterPerRequest(typeof(IType4_1), typeof(Type4_1));

            var instance = target.GetAllInstances(typeof(IType4_2));

            Assert.IsFalse(instance.Any());
        }

        #region Register same implementation type again

        [TestMethod]
        public void RegisterPerRequest_ThrowsWhenRegisteringSameImplementationWithoutKey()
        {
            var target = new Container(new ContainerConfiguration());

            target.RegisterPerRequest(typeof(IMultipleInterfaces1), typeof(MultipleInterfacesType));

            var thrown = false;

            try
            {
                target.RegisterPerRequest(typeof(IMultipleInterfaces2), typeof(MultipleInterfacesType));
            }
            catch (ContainerException e)
            {
                thrown = true;
                Assert.AreEqual(ContainerErrorCodes.TypeAlreadyRegistered, e.ErrorCode);
            }

            Assert.IsTrue(thrown);
        }

        [TestMethod]
        public void RegisterPerRequest_ThrowsWhenRegisteringSameImplementationTypeForSameKey()
        {
            var target = new Container(new ContainerConfiguration());

            target.RegisterPerRequest(typeof(IMultipleInterfaces1), typeof(MultipleInterfacesType), "key");

            var thrown = false;
            try
            {
                target.RegisterPerRequest(typeof(IMultipleInterfaces2), typeof(MultipleInterfacesType), "key");
            }
            catch (ContainerException e)
            {
                thrown = true;
                Assert.AreEqual(ContainerErrorCodes.TypeAlreadyRegistered, e.ErrorCode);
            }

            Assert.IsTrue(thrown);
        }

        [TestMethod]
        public void RegisterPerRequest_DoesNotThrowWhenRegisteringSameImplementationTypeForOtherKey()
        {
            var target = new Container(new ContainerConfiguration());

            target.RegisterPerRequest(typeof(IMultipleInterfaces1), typeof(MultipleInterfacesType));
            target.RegisterPerRequest(typeof(IMultipleInterfaces2), typeof(MultipleInterfacesType), "key");
        }

        [TestMethod]
        public void RegisterSingleton_ThrowsWhenRegisteringSameImplementationWithoutKey()
        {
            var target = new Container(new ContainerConfiguration());

            target.RegisterSingleton(typeof(IMultipleInterfaces1), typeof(MultipleInterfacesType));

            var thrown = false;

            try
            {
                target.RegisterSingleton(typeof(IMultipleInterfaces2), typeof(MultipleInterfacesType));
            }
            catch (ContainerException e)
            {
                thrown = true;
                Assert.AreEqual(ContainerErrorCodes.TypeAlreadyRegistered, e.ErrorCode);
            }

            Assert.IsTrue(thrown);
        }

        [TestMethod]
        public void RegisterSingleton_ThrowsWhenRegisteringSameImplementationTypeForSameKey()
        {
            var target = new Container(new ContainerConfiguration());

            target.RegisterSingleton(typeof(IMultipleInterfaces1), typeof(MultipleInterfacesType), "key");

            var thrown = false;

            try
            {
                target.RegisterSingleton(typeof(IMultipleInterfaces2), typeof(MultipleInterfacesType), "key");
            }
            catch (ContainerException e)
            {
                thrown = true;
                Assert.AreEqual(ContainerErrorCodes.TypeAlreadyRegistered, e.ErrorCode);
            }

            Assert.IsTrue(thrown);
        }

        [TestMethod]
        public void RegisterSingleton_DoesNotThrowWhenRegisteringSameImplementationTypeForOtherKey()
        {
            var target = new Container(new ContainerConfiguration());

            target.RegisterSingleton(typeof(IMultipleInterfaces1), typeof(MultipleInterfacesType));
            target.RegisterSingleton(typeof(IMultipleInterfaces2), typeof(MultipleInterfacesType), "key");
        }

        [TestMethod]
        public void RegisterPerRequestAndSingleton_ThrowsWhenRegisteringSameImplementationWithoutKey()
        {
            var target = new Container(new ContainerConfiguration());

            target.RegisterPerRequest(typeof(IMultipleInterfaces1), typeof(MultipleInterfacesType));

            var thrown = false;

            try
            {
                target.RegisterSingleton(typeof(IMultipleInterfaces2), typeof(MultipleInterfacesType));
            }
            catch (ContainerException e)
            {
                thrown = true;
                Assert.AreEqual(ContainerErrorCodes.TypeAlreadyRegistered, e.ErrorCode);
            }

            Assert.IsTrue(thrown);
        }

        [TestMethod]
        public void RegisterPerRequestAndSingleton_ThrowsWhenRegisteringSameImplementationTypeForSameKey()
        {
            var target = new Container(new ContainerConfiguration());

            target.RegisterPerRequest(typeof(IMultipleInterfaces1), typeof(MultipleInterfacesType), "key");

            var thrown = false;

            try
            {
                target.RegisterSingleton(typeof(IMultipleInterfaces2), typeof(MultipleInterfacesType), "key");
            }
            catch (ContainerException e)
            {
                thrown = true;
                Assert.AreEqual(ContainerErrorCodes.TypeAlreadyRegistered, e.ErrorCode);
            }

            Assert.IsTrue(thrown);
        }

        [TestMethod]
        public void RegisterPerRequestAndSingleton_DoesNotThrowWhenRegisteringSameImplementationTypeForOtherKey()
        {
            var target = new Container(new ContainerConfiguration());

            target.RegisterPerRequest(typeof(IMultipleInterfaces1), typeof(MultipleInterfacesType));
            target.RegisterSingleton(typeof(IMultipleInterfaces2), typeof(MultipleInterfacesType), "key");
        }

        [TestMethod]
        public void RegisterSingletonAndPerRequest_ThrowsWhenRegisteringSameImplementationWithoutKey()
        {
            var target = new Container(new ContainerConfiguration());

            target.RegisterSingleton(typeof(IMultipleInterfaces1), typeof(MultipleInterfacesType));

            var thrown = false;

            try
            {
                target.RegisterPerRequest(typeof(IMultipleInterfaces2), typeof(MultipleInterfacesType));
            }
            catch (ContainerException e)
            {
                thrown = true;
                Assert.AreEqual(ContainerErrorCodes.TypeAlreadyRegistered, e.ErrorCode);
            }

            Assert.IsTrue(thrown);
        }

        [TestMethod]
        public void RegisterSingletonAndPerRequest_ThrowsWhenRegisteringSameImplementationTypeForSameKey()
        {
            var target = new Container(new ContainerConfiguration());

            target.RegisterSingleton(typeof(IMultipleInterfaces1), typeof(MultipleInterfacesType), "key");

            var thrown = false;

            try
            {
                target.RegisterPerRequest(typeof(IMultipleInterfaces2), typeof(MultipleInterfacesType), "key");
            }
            catch (ContainerException e)
            {
                thrown = true;
                Assert.AreEqual(ContainerErrorCodes.TypeAlreadyRegistered, e.ErrorCode);
            }

            Assert.IsTrue(thrown);
        }

        [TestMethod]
        public void RegisterSingletonAndPerRequest_DoesNotThrowWhenRegisteringSameImplementationTypeForOtherKey()
        {
            var target = new Container(new ContainerConfiguration());

            target.RegisterSingleton(typeof(IMultipleInterfaces1), typeof(MultipleInterfacesType));
            target.RegisterPerRequest(typeof(IMultipleInterfaces2), typeof(MultipleInterfacesType), "key");
        }

        [TestMethod]
        public void RegisterPerRequest_ThrowsWhenRegisteringSameImplementationTypeWithSameInterfaceWithoutKey()
        {
            var target = new Container(new ContainerConfiguration());

            target.RegisterPerRequest(typeof(IMultipleInterfaces1), typeof(MultipleInterfacesType));

            var thrown = false;

            try
            {
                target.RegisterPerRequest(typeof(IMultipleInterfaces1), typeof(MultipleInterfacesType));
            }
            catch (ContainerException e)
            {
                thrown = true;
                Assert.AreEqual(ContainerErrorCodes.TypeAlreadyRegistered, e.ErrorCode);
            }

            Assert.IsTrue(thrown);
        }

        [TestMethod]
        public void RegisterPerRequest_ThrowsWhenRegisteringSameImplementationTypeWithSameInterfaceTypeWithKey()
        {
            var target = new Container(new ContainerConfiguration());

            target.RegisterPerRequest(typeof(IMultipleInterfaces1), typeof(MultipleInterfacesType), "key");

            var thrown = false;
            try
            {
                target.RegisterPerRequest(typeof(IMultipleInterfaces1), typeof(MultipleInterfacesType), "key");
            }
            catch (ContainerException e)
            {
                thrown = true;
                Assert.AreEqual(ContainerErrorCodes.TypeAlreadyRegistered, e.ErrorCode);
            }

            Assert.IsTrue(thrown);
        }

        [TestMethod]
        public void RegisterPerRequest_DoesNotThrowWhenRegisteringSameImplementationTypeWithSameInterfaceForOtherKey()
        {
            var target = new Container(new ContainerConfiguration());

            target.RegisterPerRequest(typeof(IMultipleInterfaces1), typeof(MultipleInterfacesType));
            target.RegisterPerRequest(typeof(IMultipleInterfaces1), typeof(MultipleInterfacesType), "key");
        }

        [TestMethod]
        public void RegisterSingleton_ThrowsWhenRegisteringSameImplementationTypeWithSameInterfaceWithoutKey()
        {
            var target = new Container(new ContainerConfiguration());

            target.RegisterSingleton(typeof(IMultipleInterfaces1), typeof(MultipleInterfacesType));

            var thrown = false;

            try
            {
                target.RegisterSingleton(typeof(IMultipleInterfaces1), typeof(MultipleInterfacesType));
            }
            catch (ContainerException e)
            {
                thrown = true;
                Assert.AreEqual(ContainerErrorCodes.TypeAlreadyRegistered, e.ErrorCode);
            }

            Assert.IsTrue(thrown);
        }

        [TestMethod]
        public void RegisterSingleton_ThrowsWhenRegisteringSameImplementationTypeWithSameInterfaceTypeWithKey()
        {
            var target = new Container(new ContainerConfiguration());

            target.RegisterSingleton(typeof(IMultipleInterfaces1), typeof(MultipleInterfacesType), "key");

            var thrown = false;
            try
            {
                target.RegisterSingleton(typeof(IMultipleInterfaces1), typeof(MultipleInterfacesType), "key");
            }
            catch (ContainerException e)
            {
                thrown = true;
                Assert.AreEqual(ContainerErrorCodes.TypeAlreadyRegistered, e.ErrorCode);
            }

            Assert.IsTrue(thrown);
        }

        [TestMethod]
        public void RegisterSingleton_DoesNotThrowWhenRegisteringSameImplementationTypeWithSameInterfaceForOtherKey()
        {
            var target = new Container(new ContainerConfiguration());

            target.RegisterSingleton(typeof(IMultipleInterfaces1), typeof(MultipleInterfacesType));
            target.RegisterSingleton(typeof(IMultipleInterfaces1), typeof(MultipleInterfacesType), "key");
        }

        [TestMethod]
        public void RegisterPerRequestAndSingleton_ThrowsWhenRegisteringSameImplementationTypeWithSameInterfaceWithoutKey()
        {
            var target = new Container(new ContainerConfiguration());

            target.RegisterPerRequest(typeof(IMultipleInterfaces1), typeof(MultipleInterfacesType));

            var thrown = false;

            try
            {
                target.RegisterSingleton(typeof(IMultipleInterfaces1), typeof(MultipleInterfacesType));
            }
            catch (ContainerException e)
            {
                thrown = true;
                Assert.AreEqual(ContainerErrorCodes.TypeAlreadyRegistered, e.ErrorCode);
            }

            Assert.IsTrue(thrown);
        }

        [TestMethod]
        public void RegisterPerRequestAndSingleton_ThrowsWhenRegisteringSameImplementationTypeWithSameInterfaceTypeWithKey()
        {
            var target = new Container(new ContainerConfiguration());

            target.RegisterPerRequest(typeof(IMultipleInterfaces1), typeof(MultipleInterfacesType), "key");

            var thrown = false;
            try
            {
                target.RegisterSingleton(typeof(IMultipleInterfaces1), typeof(MultipleInterfacesType), "key");
            }
            catch (ContainerException e)
            {
                thrown = true;
                Assert.AreEqual(ContainerErrorCodes.TypeAlreadyRegistered, e.ErrorCode);
            }

            Assert.IsTrue(thrown);
        }

        [TestMethod]
        public void RegisterPerRequestAndSingleton_DoesNotThrowWhenRegisteringSameImplementationTypeWithSameInterfaceForOtherKey()
        {
            var target = new Container(new ContainerConfiguration());

            target.RegisterPerRequest(typeof(IMultipleInterfaces1), typeof(MultipleInterfacesType));
            target.RegisterSingleton(typeof(IMultipleInterfaces1), typeof(MultipleInterfacesType), "key");
        }

        [TestMethod]
        public void RegisterSingletonAndPerRequest_ThrowsWhenRegisteringSameImplementationTypeWithSameInterfaceWithoutKey()
        {
            var target = new Container(new ContainerConfiguration());

            target.RegisterSingleton(typeof(IMultipleInterfaces1), typeof(MultipleInterfacesType));

            var thrown = false;

            try
            {
                target.RegisterPerRequest(typeof(IMultipleInterfaces1), typeof(MultipleInterfacesType));
            }
            catch (ContainerException e)
            {
                thrown = true;
                Assert.AreEqual(ContainerErrorCodes.TypeAlreadyRegistered, e.ErrorCode);
            }

            Assert.IsTrue(thrown);
        }

        [TestMethod]
        public void RegisterSingletonAndRegisterPerRequest_ThrowsWhenRegisteringSameImplementationTypeWithSameInterfaceTypeWithKey()
        {
            var target = new Container(new ContainerConfiguration());

            target.RegisterSingleton(typeof(IMultipleInterfaces1), typeof(MultipleInterfacesType), "key");

            var thrown = false;
            try
            {
                target.RegisterPerRequest(typeof(IMultipleInterfaces1), typeof(MultipleInterfacesType), "key");
            }
            catch (ContainerException e)
            {
                thrown = true;
                Assert.AreEqual(ContainerErrorCodes.TypeAlreadyRegistered, e.ErrorCode);
            }

            Assert.IsTrue(thrown);
        }

        [TestMethod]
        public void RegisterSingletonAndRegisterPerRequest_DoesNotThrowWhenRegisteringSameImplementationTypeWithSameInterfaceForOtherKey()
        {
            var target = new Container(new ContainerConfiguration());

            target.RegisterSingleton(typeof(IMultipleInterfaces1), typeof(MultipleInterfacesType));
            target.RegisterPerRequest(typeof(IMultipleInterfaces1), typeof(MultipleInterfacesType), "key");
        }

        #endregion

        [TestMethod]
        public void GetSingleInstance_ThrowsIfMoreThan1TypeRegisteredPerRequest()
        {
            var target = new Container(new ContainerConfiguration());

            target.RegisterPerRequest(typeof(ICollectionType), typeof(CollectionComplexTypeImpl1));
            target.RegisterPerRequest(typeof(ICollectionType), typeof(CollectionComplexTypeImpl2));

            var thrown = false;

            try
            {
                var instance = target.GetSingleInstance(typeof(ICollectionType));
            }
            catch (ContainerException e)
            {
                thrown = true;
                Assert.AreEqual(ContainerErrorCodes.MultipleImplementationTypesRegistered, e.ErrorCode);
            }

            Assert.IsTrue(thrown);
        }

        [TestMethod]
        public void GetAllInstances_ReturnsEnumerableWithAllInstances()
        {
            var target = new Container(new ContainerConfiguration());

            target.RegisterPerRequest(typeof(ICollectionType), typeof(CollectionSimpleTypeImpl1));
            target.RegisterPerRequest(typeof(ICollectionType), typeof(CollectionSimpleTypeImpl2));

            var instances = target.GetAllInstances(typeof(ICollectionType));

            Assert.AreEqual(2, instances.Count());
            Assert.IsInstanceOfType(instances.First(), typeof(ICollectionType));
            Assert.IsInstanceOfType(instances.ElementAt(1), typeof(ICollectionType));
            Assert.AreEqual(typeof(CollectionSimpleTypeImpl1), instances.First().GetType());
            Assert.AreEqual(typeof(CollectionSimpleTypeImpl2), instances.ElementAt(1).GetType());
        }

        [TestMethod]
        public void GetAllInstances_ReturnsDifferentInstancesForPerRequestRegistration()
        {
            var target = new Container(new ContainerConfiguration());

            target.RegisterPerRequest(typeof(ICollectionType), typeof(CollectionSimpleTypeImpl1));
            target.RegisterSingleton(typeof(ICollectionType), typeof(CollectionSimpleTypeImpl2));

            var instances1 = target.GetAllInstances(typeof(ICollectionType));
            var instances2 = target.GetAllInstances(typeof(ICollectionType));

            Assert.AreNotSame(instances1.First(), instances2.First());
        }

        [TestMethod]
        public void GetAllInstances_ReturnsSameInstancesForSingletonRegistration()
        {
            var target = new Container(new ContainerConfiguration());

            target.RegisterPerRequest(typeof(ICollectionType), typeof(CollectionSimpleTypeImpl1));
            target.RegisterSingleton(typeof(ICollectionType), typeof(CollectionSimpleTypeImpl2));

            var instances1 = target.GetAllInstances(typeof(ICollectionType));
            var instances2 = target.GetAllInstances(typeof(ICollectionType));

            Assert.AreSame(instances1.ElementAt(1), instances2.ElementAt(1));
        }

        #region Dependency resolution

        [TestMethod]
        public void GetSingleInstance_ResolvesConstructorDependenciesForPerRequestRegistrationWithPropertyInjectionOff()
        {
            var target = new Container(new ContainerConfiguration());

            target.RegisterPerRequest(typeof(IType2), typeof(Type2));
            target.RegisterPerRequest(typeof(IType2_1), typeof(Type2_1));
            target.RegisterPerRequest(typeof(IType2_2), typeof(Type2_2));

            var instance = (Type2)target.GetSingleInstance(typeof(IType2));

            Assert.IsNotNull(instance.Type2_1);
            Assert.AreEqual(typeof(Type2_1), instance.Type2_1.GetType());
        }

        [TestMethod]
        public void GetSingleInstance_DoesNotResolvePropertyDependenciesForPerRequestRegistrationWithPropertyInjectionOff()
        {
            var target = new Container(new ContainerConfiguration());

            target.RegisterPerRequest(typeof(IType2), typeof(Type2));
            target.RegisterPerRequest(typeof(IType2_1), typeof(Type2_1));
            target.RegisterPerRequest(typeof(IType2_2), typeof(Type2_2));

            var instance = (Type2)target.GetSingleInstance(typeof(IType2));

            Assert.IsNull(instance.Type2_2);
        }

        [TestMethod]
        public void GetSingleInstance_ResolvesConstructorDependenciesForPerRequestRegistrationWithPropertyInjectionOn()
        {
            var target = new Container(new ContainerConfiguration { EnablePropertyInjection = true });

            target.RegisterPerRequest(typeof(IType2), typeof(Type2));
            target.RegisterPerRequest(typeof(IType2_1), typeof(Type2_1));
            target.RegisterPerRequest(typeof(IType2_2), typeof(Type2_2));

            var instance = (Type2)target.GetSingleInstance(typeof(IType2));

            Assert.IsNotNull(instance.Type2_1);
            Assert.AreEqual(typeof(Type2_1), instance.Type2_1.GetType());
        }

        [TestMethod]
        public void GetSingleInstance_ResolvesPropertyDependenciesForPerRequestRegistrationWithPropertyInjectionOn()
        {
            var target = new Container(new ContainerConfiguration { EnablePropertyInjection = true });

            target.RegisterPerRequest(typeof(IType2), typeof(Type2));
            target.RegisterPerRequest(typeof(IType2_1), typeof(Type2_1));
            target.RegisterPerRequest(typeof(IType2_2), typeof(Type2_2));

            var instance = (Type2)target.GetSingleInstance(typeof(IType2));

            Assert.IsNotNull(instance.Type2_2);
            Assert.AreEqual(typeof(Type2_2), instance.Type2_2.GetType());
        }

        [TestMethod]
        public void GetSingleInstance_ResolvesConstructorDependenciesForSingletonRegistrationWithPropertyInjectionOff()
        {
            var target = new Container(new ContainerConfiguration());

            target.RegisterSingleton(typeof(IType2), typeof(Type2));
            target.RegisterSingleton(typeof(IType2_1), typeof(Type2_1));
            target.RegisterSingleton(typeof(IType2_2), typeof(Type2_2));

            var instance = (Type2)target.GetSingleInstance(typeof(IType2));

            Assert.IsNotNull(instance.Type2_1);
            Assert.AreEqual(typeof(Type2_1), instance.Type2_1.GetType());
        }

        [TestMethod]
        public void GetSingleInstance_DoesNotResolvePropertyDependenciesForSingletonRegistrationWithPropertyInjectionOff()
        {
            var target = new Container(new ContainerConfiguration());

            target.RegisterSingleton(typeof(IType2), typeof(Type2));
            target.RegisterSingleton(typeof(IType2_1), typeof(Type2_1));
            target.RegisterSingleton(typeof(IType2_2), typeof(Type2_2));

            var instance = (Type2)target.GetSingleInstance(typeof(IType2));

            Assert.IsNull(instance.Type2_2);
        }

        [TestMethod]
        public void GetSingleInstance_ResolvesConstructorDependenciesForSingletonRegistrationWithPropertyInjectionOn()
        {
            var target = new Container(new ContainerConfiguration { EnablePropertyInjection = true });

            target.RegisterSingleton(typeof(IType2), typeof(Type2));
            target.RegisterSingleton(typeof(IType2_1), typeof(Type2_1));
            target.RegisterSingleton(typeof(IType2_2), typeof(Type2_2));

            var instance = (Type2)target.GetSingleInstance(typeof(IType2));

            Assert.IsNotNull(instance.Type2_1);
            Assert.AreEqual(typeof(Type2_1), instance.Type2_1.GetType());
        }

        [TestMethod]
        public void GetSingleInstance_ResolvesPropertyDependenciesForSingletonRegistrationWithPropertyInjectionOn()
        {
            var target = new Container(new ContainerConfiguration { EnablePropertyInjection = true });

            target.RegisterSingleton(typeof(IType2), typeof(Type2));
            target.RegisterSingleton(typeof(IType2_1), typeof(Type2_1));
            target.RegisterSingleton(typeof(IType2_2), typeof(Type2_2));

            var instance = (Type2)target.GetSingleInstance(typeof(IType2));

            Assert.IsNotNull(instance.Type2_2);
            Assert.AreEqual(typeof(Type2_2), instance.Type2_2.GetType());
        }

        #endregion
    }
}
