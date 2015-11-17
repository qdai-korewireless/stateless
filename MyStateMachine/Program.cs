using System;
using System.Linq;
using System.Threading;
using Stateless;

namespace StatelessDemo
{
    class ProfileChange
    {
        private Result result;
        private bool currIsDynamic = true;
        private bool reqStaticIP = true;
        private bool IsDD;
        private bool IsDS;
        private bool IsSD;
        private bool IsSS;

        public ProfileChange()
        {
            IsDD = currIsDynamic && !reqStaticIP;
            IsDS = currIsDynamic && reqStaticIP;
            IsSD = !currIsDynamic && !reqStaticIP;
            IsSS = !currIsDynamic && reqStaticIP;
        }

        #region "Profile Change"

        public void Execute()
        {
            var initState = States.Start;
            ConfigStateMachine(initState);
            FireAll();
            var graph = MyStateMachine.ToDotGraph();
            Console.Write(graph);
            System.Windows.Clipboard.SetText(graph);
            Console.ReadKey(true);
        }

        void FireAll()
        {
            while (MyStateMachine.PermittedTriggers.Count() > 0)
                MyStateMachine.PermittedTriggers.ToList().ForEach((t) =>
                {
                    MyStateMachine.Fire(t);
                });
        }

        void CSPChange()
        {
            Thread.Sleep(TimeSpan.FromSeconds(1));

            result = pendingCount <= 2? Result.Pending : Result.Success;

            pendingCount++;
        }

        void KoreProfileChange()
        {
            result = Result.Success;
        }

        private void ReserveIP()
        {
            result = Result.Success;
        }


        private void UpdateCarrierState(string state)
        {
            Thread.Sleep(TimeSpan.FromSeconds(1));

            result = pendingCount <= 1 ? Result.Pending : Result.Success;

            pendingCount++;
        }

        private void UpdateCarrierStaticIP()
        {
            result = Result.Success;
        }

        void PrintState()
        {
            Console.WriteLine(string.Format("state : {0}", MyStateMachine.State.ToString()));
        }

        #endregion

        #region "State Machine"

        private enum Triggers
        {
            DD_Trigger,
            DS_Trigger,
            SD_Trigger,
            SS_Trigger,
            CSP_Trigger,
            Kore_Update_Trigger,
            Reserve_IP_Trigger,
            Update_Carrier_Static_IP_Trigger,
            Error_Trigger,
            Complete_Trigger


        }

        private enum States
        {
            Start,
            DD,
            DS,
            SS,
            SD,

            Carrier_CSP,
            Kore_Update,
            Reserve_IP,
            Update_Carrier_Static_IP,

            Complete,
            Error

        }

        private enum Result
        {
            Success,
            Pending,
            Failed

        }

        private int pendingCount = 0;

        private StateMachine<States, Triggers> MyStateMachine;

        private void ConfigStateMachine(States initState)
        {
            MyStateMachine = new StateMachine<States, Triggers>(initState);

            MyStateMachine.Configure(States.Start)
                .OnExit(s => PrintState())
                .PermitIf(Triggers.DD_Trigger, States.DD, () => IsDD, "Dynamic to Dynamic")
                .PermitIf(Triggers.DS_Trigger, States.DS, () => IsDS, "Dynamic to Static")
                .PermitIf(Triggers.SD_Trigger, States.SD, () => IsSD, "Static to Dynamic")
                .PermitIf(Triggers.SS_Trigger, States.SS, () => IsSS, "Static to Static");


            MyStateMachine.Configure(States.DD)
                .OnExit(s => PrintState())
                .Permit(Triggers.CSP_Trigger, States.Carrier_CSP, "change Carrier CSP");

            MyStateMachine.Configure(States.DS)
                .OnExit(s => PrintState())
                .Permit(Triggers.Reserve_IP_Trigger, States.Reserve_IP, "reserve IP");

            MyStateMachine.Configure(States.Reserve_IP)
                .OnEntry(ReserveIP)
                .OnExit(s => PrintState())
                .Permit(Triggers.CSP_Trigger, States.Carrier_CSP, "change Carrier CSP");

            MyStateMachine.Configure(States.Carrier_CSP)
                .OnEntry(CSPChange)
                .OnExit(s => PrintState())
                .PermitIf(Triggers.Kore_Update_Trigger, States.Kore_Update, () => IsDD && result == Result.Success, "Carrier CSP changed")
                .PermitIf(Triggers.Update_Carrier_Static_IP_Trigger, States.Update_Carrier_Static_IP, () => IsDS && result == Result.Success, "update carrier static IP")
                .PermitReentryIf(Triggers.CSP_Trigger, () => result == Result.Pending, "Carrier CSP change is pending")
                .PermitIf(Triggers.Error_Trigger, States.Error, () => result == Result.Failed, "Carrier CSP change failed");

            MyStateMachine.Configure(States.Update_Carrier_Static_IP)
                .OnEntry(UpdateCarrierStaticIP)
                .OnExit(s => PrintState())
                .PermitIf(Triggers.Kore_Update_Trigger, States.Kore_Update,()=>result == Result.Success, "update Kore");


            MyStateMachine.Configure(States.Kore_Update)
                .OnEntry(KoreProfileChange)
                .OnExit(s => PrintState())
                .PermitIf(Triggers.Complete_Trigger, States.Complete, () => result == Result.Success, "Kore update succeed")
                .PermitIf(Triggers.Error_Trigger, States.Error, () => result == Result.Failed, "kore update failed");

            MyStateMachine.Configure(States.Complete)
                .OnEntry(s => PrintState());


            MyStateMachine.Configure(States.Error)
                .OnEntry(s => PrintState());
        }

        #endregion
    }
    
    class Program{
    
        [STAThread]
        static void Main(string[] args)
        {
            ProfileChange p = new ProfileChange();
            p.Execute();
        }
    }


}



