using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal abstract class TriggerBehaviour
        {
            readonly TTrigger _trigger;
            readonly Func<bool> _guard;
            readonly string _label;

            protected TriggerBehaviour(TTrigger trigger, Func<bool> guard, string label = "")
            {
                _trigger = trigger;
                _guard = guard;
                _label = label;
            }

            public TTrigger Trigger { get { return _trigger; } }
            internal Func<bool> Guard { get { return _guard; } }
            internal string Label { get { return _label; } }

            public bool IsGuardConditionMet
            {
                get
                {
                    return _guard();
                }
            }

            public abstract bool ResultsInTransitionFrom(TState source, object[] args, out TState destination);
        }
    }
}
