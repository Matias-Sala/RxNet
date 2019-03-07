using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Threading.Tasks;

namespace RxNet
{
    public class Store<T> where T : IState
    {
        private Func<T, IAction, T> _reducer;
        private IEffect _effect;

        public Store(T state,
                     Func<T, IAction, T> reducer,
                     IEffect effect = null)
        {
            _reducer = reducer;
            _effect = effect;

            State = state;
            
        }

        public T State { get; protected set; }

        public event EventHandler<ActionDispachedEventArgs> ActionDispached;

        public async void Dispach(IAction action)
        {

            if (_effect != null)
            {
                // buscamos en la clase effect alguna funcion que coincida con la que viene en el parametro
                var _methodInfo = _effect.GetType().GetMethod(action.GetType().Name);

                if (_methodInfo != null)
                {
                    // si encontramos una la ejecutamos

                    OnActionDispached(new ActionDispachedEventArgs(action));

                    Object[] _accion = new Object[] { action };

                    var newAction = await (Task<Action>)_methodInfo.Invoke(_effect, _accion);

                    Dispach(newAction);
                }
                else
                {                    
                    State = _reducer(State, action);
                    OnActionDispached(new ActionDispachedEventArgs(action));
                }
            }
            else
            {                
                State = _reducer(State, action);
                OnActionDispached(new ActionDispachedEventArgs(action));
            }
        }

        protected virtual void OnActionDispached(ActionDispachedEventArgs e)
        {
            ActionDispached?.Invoke(this, e);
        }

        public P Select<P>(string propName)
        {
            return (P)State.GetType()
                .GetProperty(propName)
                .GetValue(State, null)
                .DeepClone();
        }

        public P Select<P>(Func<T, P> function)
        {

            return function(State).DeepClone();

        }
    }


    public class ActionDispachedEventArgs : EventArgs
    {
        public ActionDispachedEventArgs(IAction _actionDispached)
        {
            ActionDispached = _actionDispached;
        }

        public IAction ActionDispached { get; set; }
    }
}
