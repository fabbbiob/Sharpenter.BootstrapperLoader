﻿using System;
using System.Reflection;
using Autofac;
using Autofac.Extras.CommonServiceLocator;
using Machine.Specifications;
using Moq;
using Sharpenter.BootstrapperLoader.Builder;
using It = Machine.Specifications.It;

namespace Sharpenter.BootstrapperLoader.Tests
{
    [Subject(typeof(BootstrapperLoader))]
    public class BootstrapperLoaderTests
    {
        public interface IFirstDependency { }
        public interface ISecondDependency { }

        public class FirstDependency : IFirstDependency { }
        public class SecondDependency : ISecondDependency { }

        public class Bootstrapper
        {
            public virtual void ConfigureContainer(ContainerBuilder container)
            {

            }

            public virtual void Configure(IFirstDependency first, ISecondDependency second)
            {
                
            }
        }

        public class SomeBootstrapper
        {
            public virtual void SomeConfigureContainer(ContainerBuilder container)
            {

            }

            public virtual void SomeConfigure(IFirstDependency first, ISecondDependency second)
            {

            }
        }

        private static BootstrapperLoader _subject;
        private static ContainerBuilder _containerBuilder;
        private Establish context = () =>
        {
            _containerBuilder = new ContainerBuilder();
        };

        public class With_default_method_name_configuration
        {
            private Establish context = () =>
            {
                var testDll = Assembly.GetExecutingAssembly();
                _bootstrapperMock = new Mock<Bootstrapper>();
                _subject = new LoaderBuilder()
                        .Use(new InMemoryAssemblyProvider(() => new[] { testDll }))
                        .UseInstanceCreator(type => _bootstrapperMock.Object)
                        .Build();
            };

            public class When_triggering_configure_container
            {
                private Because of = () => _subject.TriggerConfigureContainer(_containerBuilder);

                private It should_invoke_default_class_and_container_method =
                    () => _bootstrapperMock.Verify(b => b.ConfigureContainer(_containerBuilder));
            }

            public class When_triggering_configure_with_all_dependencies_registered
            {
                private Establish context = () =>
                {
                    _containerBuilder.RegisterType<FirstDependency>().As<IFirstDependency>();
                    _containerBuilder.RegisterType<SecondDependency>().As<ISecondDependency>();
                };

                private Because of =
                    () => _subject.TriggerConfigure(new AutofacServiceLocator(_containerBuilder.Build()));

                private It should_invoke_default_class_and_configure_method_with_all_registered_dependencies =
                    () =>
                        _bootstrapperMock.Verify(
                            b =>
                                b.Configure(Moq.It.Is<IFirstDependency>(v => v is FirstDependency),
                                    Moq.It.Is<ISecondDependency>(v => v is SecondDependency)));
            }

            public class When_one_or_more_dependency_is_not_registered
            {
                private Establish context = () =>
                {
                    _containerBuilder.RegisterType<FirstDependency>().As<IFirstDependency>();
                };

                private Because of =
                    () =>
                        _exception = Catch.Exception(
                            () => _subject.TriggerConfigure(new AutofacServiceLocator(_containerBuilder.Build())));

                private It should_throw_resolution_exception = () =>
                {
                    _exception.ShouldNotBeNull();
                    _exception.Message.ShouldEqual(
                        "Could not resolve a service of type 'Sharpenter.BootstrapperLoader.Tests.BootstrapperLoaderTests+ISecondDependency' for the parameter 'second' of method 'Configure' on type 'Castle.Proxies.BootstrapperProxy'.");
                };

                private static Exception _exception;
            }
            
            private static Mock<Bootstrapper> _bootstrapperMock;
        }

        public class With_different_class_and_method_configuration
        {
            private Establish context = () =>
            {
                var testDll = Assembly.GetExecutingAssembly();
                _bootstrapperMock = new Mock<SomeBootstrapper>();
                _subject = new LoaderBuilder()
                    .UseInstanceCreator(type => _bootstrapperMock.Object)
                    .Use(new InMemoryAssemblyProvider(() => new[] { testDll }))
                    .ForClass()
                        .WithName("SomeBootstrapper")
                        .ConfigureContainerWith("SomeConfigureContainer")
                        .Methods()
                            .ClearAll()
                            .Call("SomeConfigure")
                    .Build();
            };

            public class When_triggering_configure_container
            {
                private Because of = () => _subject.TriggerConfigureContainer(_containerBuilder);

                private It should_invoke_correct_class_and_container_method =
                    () => _bootstrapperMock.Verify(b => b.SomeConfigureContainer(_containerBuilder));
            }

            public class When_triggering_configure_with_all_dependencies_registered
            {
                private Establish context = () =>
                {
                    _containerBuilder.RegisterType<FirstDependency>().As<IFirstDependency>();
                    _containerBuilder.RegisterType<SecondDependency>().As<ISecondDependency>();
                };

                private Because of =
                    () => _subject.TriggerConfigure(new AutofacServiceLocator(_containerBuilder.Build()));

                private It should_invoke_correct_class_and_configure_method_with_all_registered_dependencies =
                    () =>
                        _bootstrapperMock.Verify(
                            b =>
                                b.SomeConfigure(Moq.It.Is<IFirstDependency>(v => v is FirstDependency),
                                    Moq.It.Is<ISecondDependency>(v => v is SecondDependency)));
            }

            public class When_one_or_more_dependency_is_not_registered
            {
                private Establish context = () =>
                {
                    _containerBuilder.RegisterType<FirstDependency>().As<IFirstDependency>();
                };

                private Because of =
                    () =>
                        _exception = Catch.Exception(
                            () => _subject.TriggerConfigure(new AutofacServiceLocator(_containerBuilder.Build())));

                private It should_throw_resolution_exception = () =>
                {
                    _exception.ShouldNotBeNull();
                    _exception.Message.ShouldEqual(
                        "Could not resolve a service of type 'Sharpenter.BootstrapperLoader.Tests.BootstrapperLoaderTests+ISecondDependency' for the parameter 'second' of method 'SomeConfigure' on type 'Castle.Proxies.SomeBootstrapperProxy'.");
                };

                private static Exception _exception;
            }
            
            private static Mock<SomeBootstrapper> _bootstrapperMock;
        }

        public class When_configure_method_condition_returns_false
        {
            private Establish context = () =>
            {
                var testDll = Assembly.GetExecutingAssembly();
                _bootstrapperMock = new Mock<Bootstrapper>();
                _subject = new LoaderBuilder()
                    .UseInstanceCreator(type => _bootstrapperMock.Object)
                    .Use(new InMemoryAssemblyProvider(() => new[] {testDll}))
                    .ForClass()
                        .Methods()
                        .ClearAll()
                        .Call("Configure").If(() => false)
                    .Build();
            };

            private Because of = () => _subject.TriggerConfigure(new AutofacServiceLocator(_containerBuilder.Build()));

            private It should_not_be_called_when_triggering_loader =
                () =>
                    _bootstrapperMock.Verify(
                        b => b.Configure(Moq.It.IsAny<IFirstDependency>(), Moq.It.IsAny<ISecondDependency>()),
                        Times.Never);

            private static Mock<Bootstrapper> _bootstrapperMock;
        }

        public class When_configure_method_condition_returns_true
        {
            private Establish context = () =>
            {
                _containerBuilder.RegisterType<FirstDependency>().As<IFirstDependency>();
                _containerBuilder.RegisterType<SecondDependency>().As<ISecondDependency>();

                var testDll = Assembly.GetExecutingAssembly();
                _bootstrapperMock = new Mock<Bootstrapper>();
                _subject = new LoaderBuilder()
                    .UseInstanceCreator(type => _bootstrapperMock.Object)
                    .Use(new InMemoryAssemblyProvider(() => new[] { testDll }))
                    .ForClass()
                        .Methods()
                        .ClearAll()
                        .Call("Configure").If(() => true)
                    .Build();
            };

            private Because of = () => _subject.TriggerConfigure(new AutofacServiceLocator(_containerBuilder.Build()));

            private It should_be_called_when_triggering_loader =
                () =>
                    _bootstrapperMock.Verify(
                        b => b.Configure(Moq.It.IsAny<IFirstDependency>(), Moq.It.IsAny<ISecondDependency>()));

            private static Mock<Bootstrapper> _bootstrapperMock;
        }
    }
}
