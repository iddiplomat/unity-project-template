using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using Source.Infrastructure.FSM;
using UnityEngine;
using UnityEngine.TestTools;

namespace Source.Infrastructure.FSM.Tests
{
    public sealed class ProjectStateMachineTests
    {
        [Test]
        public void AddState_DuplicateType_ThrowsArgumentException()
        {
            var fsm = new ProjectStateMachine();
            fsm.AddState(new SmA(new List<string>()));
            Assert.Throws<ArgumentException>(() => fsm.AddState(new SmA(new List<string>())));
        }

        [Test]
        public async Task Enter_UnregisteredState_LogsErrorAndDoesNotTransition()
        {
            ExpectFsmErrorNotFound(nameof(SmA));
            var fsm = new ProjectStateMachine();
            await fsm.Enter<SmA>().AsTask();

            var order = new List<string>();
            fsm.AddState(new SmA(order));
            ExpectFsmLogs(2);
            await fsm.Enter<SmA>().AsTask();
            CollectionAssert.AreEqual(new[] { "Enter:A" }, order);
        }

        [Test]
        public async Task Enter_FirstState_InvokesEnterOnly()
        {
            var order = new List<string>();
            var fsm = new ProjectStateMachine();
            fsm.AddState(new SmA(order));
            ExpectFsmLogs(2);
            await fsm.Enter<SmA>().AsTask();
            CollectionAssert.AreEqual(new[] { "Enter:A" }, order);
        }

        [Test]
        public async Task Enter_SecondState_ExitsFirstThenEntersSecond()
        {
            var order = new List<string>();
            var fsm = new ProjectStateMachine();
            fsm.AddState(new SmA(order));
            fsm.AddState(new SmB(order));
            ExpectFsmLogs(2);
            await fsm.Enter<SmA>().AsTask();
            ExpectFsmLogs(4);
            await fsm.Enter<SmB>().AsTask();
            CollectionAssert.AreEqual(new[] { "Enter:A", "Exit:A", "Enter:B" }, order);
        }

        [Test]
        public async Task Enter_SameRegisteredState_ExitsThenReenters()
        {
            var order = new List<string>();
            var fsm = new ProjectStateMachine();
            fsm.AddState(new SmA(order));
            ExpectFsmLogs(2);
            await fsm.Enter<SmA>().AsTask();
            ExpectFsmLogs(4);
            await fsm.Enter<SmA>().AsTask();
            CollectionAssert.AreEqual(new[] { "Enter:A", "Exit:A", "Enter:A" }, order);
        }

        [Test]
        public async Task Enter_WhileInitialEnterBlocked_QueuesSecondAndProcessesAfter()
        {
            var order = new List<string>();
            var gate = new SmGateEnterA(order);
            var fsm = new ProjectStateMachine();
            fsm.AddState(gate);
            fsm.AddState(new SmB(order));

            ExpectFsmLogs(1);
            var first = fsm.Enter<SmGateEnterA>().AsTask();
            await UniTask.Yield();

            ExpectFsmLogs(1);
            await fsm.Enter<SmB>().AsTask();

            ExpectFsmLogs(6);
            gate.ReleaseEnter();
            await first;

            CollectionAssert.AreEqual(new[] { "Exit:A", "Enter:B" }, order);
        }

        [Test]
        public async Task Enter_WhileExitBlocked_QueuesNextAndPreservesFifo()
        {
            var order = new List<string>();
            var gateExit = new SmGateExitA(order);
            var fsm = new ProjectStateMachine();
            fsm.AddState(gateExit);
            fsm.AddState(new SmB(order));
            fsm.AddState(new SmC(order));

            ExpectFsmLogs(2);
            await fsm.Enter<SmGateExitA>().AsTask();

            var toB = fsm.Enter<SmB>().AsTask();
            await UniTask.Yield();

            ExpectFsmLogs(1);
            await fsm.Enter<SmC>().AsTask();

            ExpectFsmLogs(8);
            gateExit.ReleaseExit();
            await toB;

            CollectionAssert.AreEqual(new[] { "Enter:A", "Exit:A", "Enter:B", "Exit:B", "Enter:C" }, order);
        }

        [Test]
        public async Task Enter_MultipleWhileTransitioning_ProcessedInFifoOrder()
        {
            var order = new List<string>();
            var gateExit = new SmGateExitA(order);
            var fsm = new ProjectStateMachine();
            fsm.AddState(gateExit);
            fsm.AddState(new SmB(order));
            fsm.AddState(new SmC(order));
            fsm.AddState(new SmD(order));

            ExpectFsmLogs(2);
            await fsm.Enter<SmGateExitA>().AsTask();

            var toB = fsm.Enter<SmB>().AsTask();
            await UniTask.Yield();

            ExpectFsmLogs(1);
            await fsm.Enter<SmC>().AsTask();
            ExpectFsmLogs(1);
            await fsm.Enter<SmD>().AsTask();

            ExpectFsmLogs(13);
            gateExit.ReleaseExit();
            await toB;

            CollectionAssert.AreEqual(
                new[] { "Enter:A", "Exit:A", "Enter:B", "Exit:B", "Enter:C", "Exit:C", "Enter:D" },
                order);
        }

        [Test]
        public async Task Enter_UnregisteredWhileTransitioning_LogsErrorAndDoesNotEnqueue()
        {
            var order = new List<string>();
            var gateExit = new SmGateExitA(order);
            var fsm = new ProjectStateMachine();
            fsm.AddState(gateExit);
            fsm.AddState(new SmB(order));

            ExpectFsmLogs(2);
            await fsm.Enter<SmGateExitA>().AsTask();

            var toB = fsm.Enter<SmB>().AsTask();
            await UniTask.Yield();

            ExpectFsmErrorNotFound(nameof(SmMissing));
            await fsm.Enter<SmMissing>().AsTask();

            ExpectFsmLogs(3);
            gateExit.ReleaseExit();
            await toB;

            CollectionAssert.AreEqual(new[] { "Enter:A", "Exit:A", "Enter:B" }, order);
        }

        [Test]
        public async Task Enter_RegisteredWhileTransitioning_LogsQueuedMessage()
        {
            var order = new List<string>();
            var gateEnter = new SmGateEnterA(order);
            var fsm = new ProjectStateMachine();
            fsm.AddState(gateEnter);
            fsm.AddState(new SmB(order));

            ExpectFsmLogs(1);
            var first = fsm.Enter<SmGateEnterA>().AsTask();
            await UniTask.Yield();

            LogAssert.Expect(LogType.Log, new Regex(@"\[FSM\] Transition in progress\. Queued: SmB \(queue size: 1\)"));
            ExpectFsmLogs(6);
            await fsm.Enter<SmB>().AsTask();
            gateEnter.ReleaseEnter();
            await first;

            CollectionAssert.AreEqual(new[] { "Exit:A", "Enter:B" }, order);
        }

        private static void ExpectFsmLogs(int count)
        {
            for (var i = 0; i < count; i++)
            {
                LogAssert.Expect(LogType.Log, new Regex(@"\[FSM\] .+"));
            }
        }

        private static void ExpectFsmErrorNotFound(string typeName)
        {
            LogAssert.Expect(LogType.Error, $"{typeName} state not found!");
        }

        private sealed class SmA : IProjectState
        {
            private readonly string _id;
            private readonly List<string> _order;

            public SmA(List<string> order, string id = "A")
            {
                _order = order;
                _id = id;
            }

            public UniTask Enter()
            {
                _order.Add($"Enter:{_id}");
                return UniTask.CompletedTask;
            }

            public UniTask Exit()
            {
                _order.Add($"Exit:{_id}");
                return UniTask.CompletedTask;
            }
        }

        private sealed class SmB : IProjectState
        {
            private readonly List<string> _order;

            public SmB(List<string> order) => _order = order;

            public UniTask Enter()
            {
                _order.Add("Enter:B");
                return UniTask.CompletedTask;
            }

            public UniTask Exit()
            {
                _order.Add("Exit:B");
                return UniTask.CompletedTask;
            }
        }

        private sealed class SmC : IProjectState
        {
            private readonly List<string> _order;

            public SmC(List<string> order) => _order = order;

            public UniTask Enter()
            {
                _order.Add("Enter:C");
                return UniTask.CompletedTask;
            }

            public UniTask Exit()
            {
                _order.Add("Exit:C");
                return UniTask.CompletedTask;
            }
        }

        private sealed class SmD : IProjectState
        {
            private readonly List<string> _order;

            public SmD(List<string> order) => _order = order;

            public UniTask Enter()
            {
                _order.Add("Enter:D");
                return UniTask.CompletedTask;
            }

            public UniTask Exit()
            {
                _order.Add("Exit:D");
                return UniTask.CompletedTask;
            }
        }

        private sealed class SmMissing : IProjectState
        {
            public UniTask Enter() => throw new InvalidOperationException("unreachable");
            public UniTask Exit() => throw new InvalidOperationException("unreachable");
        }

        private class GateEnterState : IProjectState
        {
            private readonly List<string> _order;
            private readonly string _id;
            private readonly UniTaskCompletionSource _tcs = new();

            protected GateEnterState(List<string> order, string id)
            {
                _order = order;
                _id = id;
            }

            public void ReleaseEnter() => _tcs.TrySetResult();

            public UniTask Enter() => _tcs.Task;

            public UniTask Exit()
            {
                _order.Add($"Exit:{_id}");
                return UniTask.CompletedTask;
            }
        }

        private sealed class SmGateEnterA : GateEnterState
        {
            public SmGateEnterA(List<string> order) : base(order, "A")
            {
            }
        }

        private class GateExitState : IProjectState
        {
            private readonly string _id;
            private readonly List<string> _order;
            private readonly UniTaskCompletionSource _tcs = new();

            protected GateExitState(string id, List<string> order)
            {
                _id = id;
                _order = order;
            }

            public void ReleaseExit() => _tcs.TrySetResult();

            public UniTask Enter()
            {
                _order.Add($"Enter:{_id}");
                return UniTask.CompletedTask;
            }

            public UniTask Exit()
            {
                _order.Add($"Exit:{_id}");
                return _tcs.Task;
            }
        }

        private sealed class SmGateExitA : GateExitState
        {
            public SmGateExitA(List<string> order) : base("A", order)
            {
            }
        }
    }
}
