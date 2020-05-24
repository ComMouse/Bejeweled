using System;
using UnityEngine;

namespace Bejeweled.Utility
{
    /// <summary>
    /// Base class for state machine.
    /// </summary>
    public abstract class BaseStateMachineBehaviour<StateType> : MonoBehaviour
        where StateType : Enum
    {
        public abstract StateType State { get; protected set; }

        /// <summary>
        /// Check if the new state can be transited. Default is true.
        /// </summary>
        protected virtual bool CanTransitState(StateType newState) => true;

        /// <summary>
        /// Clean up for old states before state transits.
        /// </summary>
        protected abstract void PreTransitState(StateType oldState, StateType newState);

        /// <summary>
        /// Set up for new states after state transits.
        /// </summary>
        protected abstract void PostTransitState(StateType oldState, StateType newState);

        /// <summary>
        /// Transit to a new state.
        /// </summary>
        public virtual void TransitState(StateType newState)
        {
            // Ignore the same state
            if (State.Equals(newState))
                return;

            StateType oldState = State;

            PreTransitState(oldState, newState);

            State = newState;

            PostTransitState(oldState, newState);
        }
    }
}