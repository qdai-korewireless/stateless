using System;
using System.Linq;
using Stateless;

namespace StatelessDemo
{
    class Program
    {
        private enum Triggers
        {
            TriggerState1,
            TriggerState2,
            TriggerState3,
            TriggerCompleteState,
            TriggerErrorState


        }
        private enum States
        {
            Start,
            State1,
            State2,
            State3,
            Complete,
            Error

        }

        private bool success = false;

        private StateMachine<States, Triggers> MyStateMachine;

        public Program()
        {
        }


        public void Run()
        {
            MyStateMachine = new StateMachine<States, Triggers>(States.Start);

            MyStateMachine.Configure(States.Start)
                .OnExit(s => PrintStateOnExit())
                .Permit(Triggers.TriggerState1, States.State1);

            MyStateMachine.Configure(States.State1)
                .OnEntry(s => DoSomeThing(true))
                .OnExit(s => PrintStateOnExit())
                .PermitIf(Triggers.TriggerState2, States.State2, () => success)
                .PermitIf(Triggers.TriggerErrorState, States.Error, () => !success);

            MyStateMachine.Configure(States.State2)
                .OnEntry(s => DoSomeThing(false))
                .OnExit(s => PrintStateOnExit())
                .PermitIf(Triggers.TriggerState3, States.State3, () => success)
                .PermitIf(Triggers.TriggerErrorState, States.Error, () => !success);

            MyStateMachine.Configure(States.State3)
                .OnEntry(s => DoSomeThing(true))
                .OnExit(s => PrintStateOnExit())
                .PermitIf(Triggers.TriggerCompleteState, States.Complete, () => success)
                .PermitIf(Triggers.TriggerErrorState, States.Error, () => !success);

            MyStateMachine.Configure(States.Complete)
                .OnEntry(s => PrintStateOnEntry());


            MyStateMachine.Configure(States.Error)
                .OnEntry(s => PrintStateOnEntry());

            FireAll();

            Console.ReadKey(true);
        }

        void FireAll()
        {
            while (MyStateMachine.PermittedTriggers.Count() > 0)
                MyStateMachine.PermittedTriggers.ToList().ForEach((t) =>
                {
                    MyStateMachine.Fire(t);
                });

            Console.WriteLine(MyStateMachine.ToDotGraph());
        }

        void DoSomeThing(bool result)
        {
            success = result;
        }

        void PrintStateOnEntry()
        {
            Console.WriteLine(string.Format("Entered state : {0}", MyStateMachine.State.ToString()));
        }

        void PrintStateOnExit()
        {
            Console.WriteLine(string.Format("Exited state : {0}", MyStateMachine.State.ToString()));
        }

        static void Main(string[] args)
        {
            Program p = new Program();
            p.Run();
        }
    }


}


