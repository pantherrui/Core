// Copyright 2004-2014 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Castle.DynamicProxy.Tests
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;

	using Castle.DynamicProxy.Generators;
	using Castle.DynamicProxy.Tests.BugsReported;
	using Castle.DynamicProxy.Tests.Interceptors;
	using Castle.DynamicProxy.Tests.InterClasses;
	using Castle.DynamicProxy.Tests.Interfaces;

	using NUnit.Framework;

	[TestFixture]
	public class BasicInterfaceProxyTestCase : BasePEVerifyTestCase
	{
		[Test]
		public void BasicInterfaceProxyWithValidTarget()
		{
			LogInvocationInterceptor logger = new LogInvocationInterceptor();

			IService service = (IService)
			                   generator.CreateInterfaceProxyWithTarget(
			                   	typeof (IService), new ServiceImpl(), logger);

			Assert.AreEqual(3, service.Sum(1, 2));

			Assert.AreEqual("Sum ", logger.LogContents);
		}

		[Test]
		public void Caching()
		{
#pragma warning disable 219
			IService service = (IService)
			                   generator.CreateInterfaceProxyWithTarget(
			                   	typeof (IService), new ServiceImpl(), new StandardInterceptor());
			service = (IService)
			          generator.CreateInterfaceProxyWithTarget(
			          	typeof (IService), new ServiceImpl(), new StandardInterceptor());
			service = (IService)
			          generator.CreateInterfaceProxyWithTarget(
			          	typeof (IService), new ServiceImpl(), new StandardInterceptor());
			service = (IService)
			          generator.CreateInterfaceProxyWithTarget(
			          	typeof (IService), new ServiceImpl(), new StandardInterceptor());
#pragma warning restore 219
		}

		[Test]
		public void BasicInterfaceProxyWithValidTarget2()
		{
			LogInvocationInterceptor logger = new LogInvocationInterceptor();

			IService2 service = (IService2)
			                    generator.CreateInterfaceProxyWithTarget(
			                    	typeof (IService2), new Service2(), logger);

			service.DoOperation2();

			Assert.AreEqual("DoOperation2 ", logger.LogContents);
		}

		[Test]
		public void InterfaceInheritance()
		{
			LogInvocationInterceptor logger = new LogInvocationInterceptor();

			IService service = (IExtendedService)
			                   generator.CreateInterfaceProxyWithTarget(
			                   	typeof (IExtendedService), new ServiceImpl(), logger);

			Assert.AreEqual(3, service.Sum(1, 2));

			Assert.AreEqual("Sum ", logger.LogContents);
		}

		[Test]
		public void Indexer()
		{
			LogInvocationInterceptor logger = new LogInvocationInterceptor();

			InterfaceWithIndexer service = (InterfaceWithIndexer)
			                               generator.CreateInterfaceProxyWithTarget(
			                               	typeof (InterfaceWithIndexer), new ClassWithIndexer(), logger);

			Assert.AreEqual(1, service[1]);

			Assert.AreEqual("get_Item ", logger.LogContents);
		}

		[Test]
#if DOTNET35
		[Ignore("Signature of the body and declaration in a method implementation do not match. https://support.microsoft.com/en-us/kb/960240")]
#endif
		public void ProxyTypeWithMultiDimentionalArrayAsParameter()
		{
			var proxy = generator.CreateInterfaceProxyWithTarget<IClassWithMultiDimentionalArray>(
				new ClassWithMultiDimentionalArray(),
				new LogInvocationInterceptor());
		    proxy.Do(new[] { 1, 2, 3 });
		    proxy.Do2(new[,] { { 1, 2 }, { 3, 4 } });
		    proxy.Do3(new[] { "a", "b", "c" });
		    proxy.Do4(new[,] { { "a", "b" }, { "c", "d" } });
		}

		[Test]
		public void BaseTypeForInterfaceProxyHonored()
		{
			var options = new ProxyGenerationOptions();
			options.BaseTypeForInterfaceProxy = typeof (SimpleClass);
			var proxy = generator.CreateInterfaceProxyWithoutTarget(typeof (IService), Type.EmptyTypes, options);

			Assert.IsInstanceOf<SimpleClass>(proxy);
		}

		[Test]
		public void CantCreateInterfaceTargetedProxyWithoutInterface()
		{
			Assert.Throws<ArgumentException>(delegate {
#pragma warning disable 219
				IService2 service = (IService2)generator.CreateInterfaceProxyWithTargetInterface(typeof(Service2), new Service2());
#pragma warning restore 219
			});
		}

		[Test]
		public void InterfaceTargetTypeProducesInvocationsThatCanChangeTarget()
		{
			LogInvocationInterceptor logger = new LogInvocationInterceptor();
			AssertCanChangeTargetInterceptor invocationChecker = new AssertCanChangeTargetInterceptor();

			IService2 service = (IService2)
			                    generator.CreateInterfaceProxyWithTargetInterface(
			                    	typeof (IService2), new Service2(), invocationChecker, logger);

			service.DoOperation2();

			Assert.AreEqual("DoOperation2 ", logger.LogContents);
		}

		[Test]
		public void ChangingInvocationTargetSucceeds()
		{
			LogInvocationInterceptor logger = new LogInvocationInterceptor();

			IService service = (IService)
			                   generator.CreateInterfaceProxyWithTargetInterface(
			                   	typeof (IService), new AlwaysThrowsServiceImpl(), new ChangeTargetInterceptor(new ServiceImpl()),
			                   	logger);

			Assert.AreEqual(20, service.Sum(10, 10));
		}

		/// <summary>
		/// See http://support.castleproject.org/browse/DYNPROXY-43
		/// </summary>
		[Test]
		public void MethodParamNamesAreReplicated()
		{
			// Try with interface proxy (with target)
			IMyInterface i = generator.CreateInterfaceProxyWithTarget(typeof (IMyInterface), new MyClass(),
			                                                          new StandardInterceptor()) as IMyInterface;

			ParameterInfo[] methodParams = GetMyTestMethodParams(i.GetType());
			Assert.AreEqual("myParam", methodParams[0].Name);
		}

		[Test]
		public void Should_properly_implement_two_interfaces_with_methods_with_identical_signatures()
		{
			object proxy = generator.CreateInterfaceProxyWithoutTarget(typeof (IIdenticalOne), new[] {typeof (IIdenticalTwo)},
			                                                            new DoNothingInterceptor());
			(proxy as IIdenticalOne).Foo();
			(proxy as IIdenticalTwo).Foo();
		}

		[Test]
		public void Should_properly_proxy_class_that_implements_interface_virtually_non_interceptable()
		{
			var proxy = generator.CreateClassProxy(typeof(IdenticalOneVirtual));
			(proxy as IIdenticalOne).Foo();
		}

		[Test]
		public void Should_properly_proxy_class_that_implements_interface_virtually_interceptable()
		{
			var proxy = generator.CreateClassProxy(typeof (IdenticalOneVirtual), new Type[] {typeof (IIdenticalOne)},
			                                       ProxyGenerationOptions.Default);
			(proxy as IIdenticalOne).Foo();
		}

		[Test]
		public void Should_implement_explicitly_duplicate_interface_members()
		{
			Type type =
				generator.CreateInterfaceProxyWithoutTarget(typeof(IIdenticalOne), new[] { typeof(IIdenticalTwo) }).GetType();
			MethodInfo method = type.GetMethod("Foo", BindingFlags.Instance | BindingFlags.Public);
			Assert.IsNotNull(method);
			Assert.AreSame(method, type.GetTypeInfo().GetRuntimeInterfaceMap(typeof(IIdenticalOne)).TargetMethods[0]);
			MethodInfo method2 = type.GetMethod("IIdenticalTwo.Foo", BindingFlags.Instance | BindingFlags.Public);
			Assert.IsNotNull(method2);
		}

		private ParameterInfo[] GetMyTestMethodParams(Type type)
		{
			MethodInfo methodInfo = type.GetMethod("MyTestMethod", BindingFlags.Instance | BindingFlags.Public);
			return methodInfo.GetParameters();
		}

		[Test]
		public void Cannot_proxy_open_generic_type()
		{
			var ex = Assert.Throws<GeneratorException>(() => generator.CreateInterfaceProxyWithoutTarget(typeof(IList<>), new IInterceptor[0]));
			Assert.AreEqual("Can not create proxy for type System.Collections.Generic.IList`1 because it is an open generic type.", ex.Message);
		}

		[Test]
		public void Cannot_proxy_generic_type_with_open_generic_type_parameter()
		{
			var innerType = typeof(IList<>);
			var targetType = innerType.MakeGenericType(typeof(IList<>));
			var ex = Assert.Throws<GeneratorException>(() => generator.CreateInterfaceProxyWithoutTarget(targetType, new IInterceptor[0]));
			StringAssert.StartsWith(
#if __MonoCS__
				"Can not create proxy for type System.Collections.Generic.IList`1[[System.Collections.Generic.IList`1, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]] because type System.Collections.Generic.IList`1 is an open generic type.",
#else
				"Can not create proxy for type IList`1 because type System.Collections.Generic.IList`1 is an open generic type.",
#endif
				ex.Message);
		}

		[Test]
		public void Cannot_proxy_inaccessible_interface()
		{
			var ex = Assert.Throws<GeneratorException>(() =>
				generator.CreateInterfaceProxyWithoutTarget(typeof(PrivateInterface), new IInterceptor[0]));
			StringAssert.StartsWith(
				"Can not create proxy for type Castle.DynamicProxy.Tests.BasicInterfaceProxyTestCase+PrivateInterface because it is not accessible. Make it public, or internal",
				ex.Message);
		}

		[Test]
		public void Cannot_proxy_generic_interface_with_inaccessible_type_argument()
		{
			var ex = Assert.Throws<GeneratorException>(() =>
				generator.CreateInterfaceProxyWithoutTarget(typeof(IList<PrivateInterface>), new IInterceptor[0]));
			StringAssert.StartsWith(
				"Can not create proxy for type System.Collections.Generic.IList`1[[Castle.DynamicProxy.Tests.BasicInterfaceProxyTestCase+PrivateInterface, Castle.Core.Tests, Version=0.0.0.0, Culture=neutral, PublicKeyToken=407dd0808d44fbdc]] because type Castle.DynamicProxy.Tests.BasicInterfaceProxyTestCase+PrivateInterface is not accessible. Make it public, or internal",
				ex.Message);
		}

		[Test]
		public void Cannot_proxy_generic_interface_with_type_argument_that_has_inaccessible_type_argument()
		{
			var expected = string.Format("Can not create proxy for type {0} because type {1} is not accessible. Make it public, or internal",
				typeof(IList<IList<PrivateInterface>>).FullName, typeof(PrivateInterface).FullName);

			var exception = Assert.Throws<GeneratorException>(() => generator.CreateInterfaceProxyWithoutTarget(typeof(IList<IList<PrivateInterface>>), new IInterceptor[0]));
			StringAssert.StartsWith(expected, exception.Message);
		}

		[Test]
		public void Can_proxy_generic_interface()
		{
			generator.CreateInterfaceProxyWithoutTarget(typeof(IList<object>), new IInterceptor[0]);
		}

		private interface PrivateInterface { }
	}

	public class IdenticalOneVirtual:IIdenticalOne
	{
		public virtual string Foo()
		{
			return "Foo";
		}
	}
}