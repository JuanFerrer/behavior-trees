﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentBehaviorTree;
using FluentBehaviorTree.Utilities;

namespace BTTests
{
    [TestClass]
    public class APITests
    {
        private int randomLeafExecuted;
        private int rngSeed = 42;

        // Leaf nodes
        [TestMethod]
        public void ActionTest()
        {
            Assert.AreEqual(Status.SUCCESS, Action(ReturnSUCCESS));
            Assert.AreEqual(Status.FAILURE, Action(ReturnFAILURE));
            Assert.AreEqual(Status.RUNNING, Action(ReturnRUNNING));
            Assert.AreEqual(Status.ERROR,   Action(ReturnERROR));
        }

        [TestMethod]
        public void ConditionTest()
        {
            Assert.AreEqual(Status.SUCCESS, Condition(() => { return true; }));
            Assert.AreEqual(Status.FAILURE, Condition(() => { return false; }));
        }

        [TestMethod]
        public void SubtreeTest()
        {
            Assert.AreEqual(Status.SUCCESS, Subtree(ReturnSUCCESS));
            Assert.AreEqual(Status.FAILURE, Subtree(ReturnFAILURE));
            Assert.AreEqual(Status.RUNNING, Subtree(ReturnRUNNING));
            Assert.AreEqual(Status.ERROR,   Subtree(ReturnERROR));
        }

        // Composites
        [TestMethod]
        public void SequenceTest()
        {
            // Only succeeds when all are true
            Assert.AreEqual(Status.SUCCESS, Sequence(true,  true,  true));
            Assert.AreEqual(Status.FAILURE, Sequence(true,  true,  false));
            Assert.AreEqual(Status.FAILURE, Sequence(true,  false, false));
            Assert.AreEqual(Status.FAILURE, Sequence(false, false, false));
            Assert.AreEqual(Status.FAILURE, Sequence(false, true,  true));
            Assert.AreEqual(Status.FAILURE, Sequence(false, true,  false));
            Assert.AreEqual(Status.FAILURE, Sequence(true,  false, true));
            Assert.AreEqual(Status.FAILURE, Sequence(false, false, true));
        }

        [TestMethod]
        public void SelectorTest()
        {
            // Only fails when all are false
            Assert.AreEqual(Status.SUCCESS, Selector(true,  true,  true));
            Assert.AreEqual(Status.SUCCESS, Selector(true,  true,  false));
            Assert.AreEqual(Status.SUCCESS, Selector(true,  false, false));
            Assert.AreEqual(Status.FAILURE, Selector(false, false, false));
            Assert.AreEqual(Status.SUCCESS, Selector(false, true,  true));
            Assert.AreEqual(Status.SUCCESS, Selector(false, true,  false));
            Assert.AreEqual(Status.SUCCESS, Selector(true,  false, true));
            Assert.AreEqual(Status.SUCCESS, Selector(false, false, true));
        }

        [TestMethod]
        public void RandomSequenceTest()
        {
            // Only succeeds when all are true
            Assert.AreEqual(Status.SUCCESS, RandomSequence(true, true, true));
            Assert.AreEqual(Status.FAILURE, RandomSequence(true, true, false));
            Assert.AreEqual(Status.FAILURE, RandomSequence(true, false, false));
            Assert.AreEqual(Status.FAILURE, RandomSequence(false, false, false));
            Assert.AreEqual(Status.FAILURE, RandomSequence(false, true, true));
            Assert.AreEqual(Status.FAILURE, RandomSequence(false, true, false));
            Assert.AreEqual(Status.FAILURE, RandomSequence(true, false, true));
            Assert.AreEqual(Status.FAILURE, RandomSequence(false, false, true));

            // Also check it's being executed at random
            RandomSystem.Seed(rngSeed);
            // For the next five shuffles, list order should be:
            // 2, 1, 3
            // 3, 2, 1
            // 2, 3, 1
            // 1, 2, 3
            // 3, 2, 1
            RandomSequence(false, true, false);
            Assert.AreEqual(1, randomLeafExecuted);
            RandomSequence(false, true, false);
            Assert.AreEqual(3, randomLeafExecuted);
            RandomSequence(false, true, true);
            Assert.AreEqual(1, randomLeafExecuted);
            RandomSequence(false, false, false);
            Assert.AreEqual(1, randomLeafExecuted);
            RandomSequence(false, false, true);
            Assert.AreEqual(2, randomLeafExecuted);
        }

        [TestMethod]
        public void RandomSelectorTest()
        {
            Assert.AreEqual(Status.SUCCESS, RandomSelector(true, true, true));
            Assert.AreEqual(Status.SUCCESS, RandomSelector(true, true, false));
            Assert.AreEqual(Status.SUCCESS, RandomSelector(true, false, false));
            Assert.AreEqual(Status.FAILURE, RandomSelector(false, false, false));
            Assert.AreEqual(Status.SUCCESS, RandomSelector(false, true, true));
            Assert.AreEqual(Status.SUCCESS, RandomSelector(false, true, false));
            Assert.AreEqual(Status.SUCCESS, RandomSelector(true, false, true));
            Assert.AreEqual(Status.SUCCESS, RandomSelector(false, false, true));

            // Also check it's being executed at random
            RandomSystem.Seed(rngSeed);
            // For the next five shuffles, list order should be:
            // 2, 1, 3
            // 3, 2, 1
            // 2, 3, 1
            // 1, 2, 3
            // 3, 2, 1
            RandomSelector(false, true, false);
            Assert.AreEqual(2, randomLeafExecuted);
            RandomSelector(false, true, false);
            Assert.AreEqual(2, randomLeafExecuted);
            RandomSelector(false, true, true);
            Assert.AreEqual(2, randomLeafExecuted);
            RandomSelector(false, false, true);
            Assert.AreEqual(3, randomLeafExecuted);
            RandomSelector(false, false, true);
            Assert.AreEqual(3, randomLeafExecuted);
        }

        // Decorators
        [TestMethod]
        public void NegatorTest()
        {
            // Only fails when all are false
            Assert.AreEqual(Status.FAILURE, Negator(ReturnSUCCESS));
            Assert.AreEqual(Status.SUCCESS, Negator(ReturnFAILURE));
            Assert.AreEqual(Status.RUNNING, Negator(ReturnRUNNING));
            Assert.AreEqual(Status.ERROR,   Negator(ReturnERROR));
        }

        [TestMethod]
        public void RepeaterTest()
        {
            Assert.AreEqual(Status.SUCCESS, Repeater(5, 5));
            Assert.AreEqual(Status.SUCCESS, Repeater(2, 5));
            Assert.AreEqual(Status.SUCCESS, Repeater(0, 5));
            Assert.AreEqual(Status.RUNNING, Repeater(5, 1));
            Assert.AreEqual(Status.ERROR,   Repeater(0, 0));
        }

        [TestMethod]
        public void RepeatUntilFailTest()
        {
            Assert.AreEqual(Status.SUCCESS, RepeatUntilFail(5, 5));
            Assert.AreEqual(Status.SUCCESS, RepeatUntilFail(2, 5));
            Assert.AreEqual(Status.SUCCESS, RepeatUntilFail(0, 5));
            Assert.AreEqual(Status.RUNNING, RepeatUntilFail(5, 1));
            Assert.AreEqual(Status.ERROR,   RepeatUntilFail(0, 0));
        }

        [TestMethod]
        public void SucceederTest()
        {
            // Only fails when all are false
            Assert.AreEqual(Status.SUCCESS, Succeeder(ReturnSUCCESS));
            Assert.AreEqual(Status.SUCCESS, Succeeder(ReturnFAILURE));
            Assert.AreEqual(Status.RUNNING, Succeeder(ReturnRUNNING));
            Assert.AreEqual(Status.ERROR,   Succeeder(ReturnERROR));
        }

        [TestMethod]
        public void TimerTest()
        {
            Assert.AreEqual(Status.SUCCESS, Timer(100, 100));
            Assert.AreEqual(Status.RUNNING, Timer(300, 100));
            Assert.AreEqual(Status.SUCCESS, Timer(100, 300));
        }

                Status ReturnSUCCESS()
        {
            return Status.SUCCESS;
        }

        Status ReturnFAILURE()
        {
            return Status.FAILURE;
        }

        Status ReturnRUNNING()
        {
            return Status.RUNNING;
        }

        Status ReturnERROR()
        {
            return Status.ERROR;
        }

        bool ReturnTrue()
        {
            return true;
        }

        bool ReturnFalse()
        {
            return false;
        }

        Status Action(Func<Status> function)
        {
            BehaviorTree bt = new BehaviorTreeBuilder("ActionTest")
                .Do("Action", function)
                .End();

            Status result = bt.Tick();

            return result;
        }

        Status Condition(Func<bool> function)
        {
            BehaviorTree bt = new BehaviorTreeBuilder("ConditionTest")
                .If("Condition", function)
                .End();

            Status result = bt.Tick();

            return result;
        }

        Status Subtree(Func<Status> function)
        {
            BehaviorTree subtree = new BehaviorTreeBuilder("Subtree")
                .Do("Action", function)
                .End();

            BehaviorTree bt = new BehaviorTreeBuilder("SubtreeTest")
                .Do("Action", subtree)
                .End();

            Status result = bt.Tick();

            return result;
        }

        Status Sequence(bool b1, bool b2, bool b3)
        {
            BehaviorTree bt = new BehaviorTreeBuilder("SequenceTest")
                .Sequence("Sequence")
                    .If("Condition 1", () => { return b1; })
                    .If("Condition 2", () => { return b2; })
                    .If("Condition 3", () => { return b3; })
                    .End()
                .End();

            Status result = bt.Tick();

            return result;
        }

        Status Selector(bool b1, bool b2, bool b3)
        {
            BehaviorTree bt = new BehaviorTreeBuilder("SelectorTest")
                .Selector("Selector")
                    .If("Condition 1", () => { return b1; })
                    .If("Condition 2", () => { return b2; })
                    .If("Condition 3", () => { return b3; })
                    .End()
                .End();

            Status result = bt.Tick();

            return result;
        }

        Status RandomSequence(bool b1, bool b2, bool b3)
        {
            BehaviorTree bt = new BehaviorTreeBuilder("RandomSequenceTest")
                .RandomSequence("RandomSequence")
                    .If("Condition 1", () => 
                    {
                        randomLeafExecuted = 1;
                        return b1;
                    })
                    .If("Condition 2", () =>
                    {
                        randomLeafExecuted = 2;
                        return b2;
                    })
                    .If("Condition 3", () =>
                    {
                        randomLeafExecuted = 3;
                        return b3;
                    })
                    .End()
                .End();

            Status result = bt.Tick();

            return result;
        }

        Status RandomSelector(bool b1, bool b2, bool b3)
        {
            BehaviorTree bt = new BehaviorTreeBuilder("RandomSelectorTest")
                .RandomSelector("Random selector")
                    .If("Condition 1", () =>
                    {
                        randomLeafExecuted = 1;
                        return b1;
                    })
                    .If("Condition 2", () =>
                    {
                        randomLeafExecuted = 2;
                        return b2;
                    })
                    .If("Condition 3", () =>
                    {
                        randomLeafExecuted = 3;
                        return b3;
                    })
                    .End()
                .End();

            Status result = bt.Tick();

            return result;
        }

        Status Negator(Func<Status> function)
        {
            BehaviorTree bt = new BehaviorTreeBuilder("NegatorTest")
                .Not("Negator")
                    .Do("Action", function)
                .End();

            Status result = bt.Tick();

            return result;
        }

        Status Repeater(int timesUntilSuccess, int timesToTick)
        {
            BehaviorTree bt = new BehaviorTreeBuilder("RepeaterTest")
                .Repeat("Repeater", timesUntilSuccess)
                    .Do("Action", () =>{ return Status.SUCCESS; })
                .End();
            Status result = Status.ERROR;
            for (int i = 0; i < timesToTick; ++i)
            {
                result = bt.Tick();
            }

            return result;
        }

        Status RepeatUntilFail(int timesUntilFail, int timesToTick)
        {
            int attempts = 0;
            BehaviorTree bt = new BehaviorTreeBuilder("RepeatUntilFailTest")
                .RepeatUntilFail("RepeatUntilFail")
                    .Do("Action", () =>
                    {
                        if (attempts < timesUntilFail - 1)
                        {
                            attempts++;
                            return Status.SUCCESS;
                        }
                        else
                        {
                            return Status.FAILURE;
                        }
                    })
                .End();
            Status result = Status.ERROR;
            for (int i = 0; i < timesToTick; ++i)
            {
                result = bt.Tick();
            }

            return result;
        }

        Status Succeeder(Func<Status> function)
        {
            BehaviorTree bt = new BehaviorTreeBuilder("SucceederTest")
                .Ignore("Succeeder")
                    .Do("Action", function)
                .End();

            Status result = bt.Tick();

            return result;
        }

        Status Timer(ulong timeUntilSuccess, int waitTime)
        {
            BehaviorTree bt = new BehaviorTreeBuilder("TimerTest")
                .Wait("Timer", timeUntilSuccess)
                    .Do("Action", ReturnSUCCESS)
                .End();
            Status result = bt.Tick();
            // Sleep for 50 ms longer, just to make sure the task finishes
            System.Threading.Thread.Sleep(waitTime + 50);
            result = bt.Tick();

            return result;
        }
    }
}
