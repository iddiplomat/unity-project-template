using System;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using Source.Infrastructure.FSM.Factory;
using VContainer;

namespace Source.Infrastructure.FSM.Tests
{
    public sealed class ProjectStateFactoryTests
    {
        [Test]
        public void CreateState_Transient_ReturnsDistinctInstances()
        {
            using var container = BuildContainer(b =>
            {
                b.Register<FactoryStatePlain>(Lifetime.Transient);
            });

            var factory = new ProjectStateFactory(container);
            var a = factory.CreateState<FactoryStatePlain>();
            var b = factory.CreateState<FactoryStatePlain>();
            Assert.That(a, Is.Not.SameAs(b));
        }

        [Test]
        public void CreateState_Singleton_ReturnsSameInstance()
        {
            using var container = BuildContainer(b =>
            {
                b.Register<FactoryStatePlain>(Lifetime.Singleton);
            });

            var factory = new ProjectStateFactory(container);
            var a = factory.CreateState<FactoryStatePlain>();
            var b = factory.CreateState<FactoryStatePlain>();
            Assert.That(a, Is.SameAs(b));
        }

        [Test]
        public void CreateState_Unregistered_ThrowsVContainerException()
        {
            using var container = BuildContainer(_ => { });
            var factory = new ProjectStateFactory(container);
            Assert.Throws<VContainerException>(() => factory.CreateState<FactoryStatePlain>());
        }

        [Test]
        public void CreateState_WithDependencies_InjectsFromContainer()
        {
            using var container = BuildContainer(b =>
            {
                b.Register<FactoryDependency>(Lifetime.Singleton);
                b.Register<FactoryStateWithDependency>(Lifetime.Transient);
            });

            var factory = new ProjectStateFactory(container);
            var state = factory.CreateState<FactoryStateWithDependency>();
            Assert.That(state.Dependency, Is.Not.Null);
            Assert.That(state.Dependency, Is.SameAs(container.Resolve<FactoryDependency>()));
        }

        private static IObjectResolver BuildContainer(Action<ContainerBuilder> configure)
        {
            var builder = new ContainerBuilder();
            configure(builder);
            return builder.Build();
        }

        private sealed class FactoryStatePlain : IProjectState
        {
            public UniTask Enter() => UniTask.CompletedTask;
            public UniTask Exit() => UniTask.CompletedTask;
        }

        private sealed class FactoryDependency
        {
        }

        private sealed class FactoryStateWithDependency : IProjectState
        {
            public FactoryStateWithDependency(FactoryDependency dependency) => Dependency = dependency;

            public FactoryDependency Dependency { get; }

            public UniTask Enter() => UniTask.CompletedTask;
            public UniTask Exit() => UniTask.CompletedTask;
        }
    }
}
