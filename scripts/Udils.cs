using System;
using System.Collections.Generic;
using System.Linq;

public class Udils
{
    public class ProgressableAction
    {
        private struct TimedAction
        {
            public float Time;
            public Action Action;
        }

        private List<TimedAction> _timedActions = new List<TimedAction>();
        private List<TimedAction> _currentTimedActions = new List<TimedAction>();
        private float _timer = 0f;
        private bool _triggered = false;

        public ProgressableAction Add(float time, Action action)
        {
            _timedActions.Add(new TimedAction { Time = time, Action = action });

            _timedActions = _timedActions.OrderBy(element => element.Time).ToList();

            return this;
        }

        public void Update(float delta)
        {
            if (!_triggered) return;

            _timer += delta;

            while (_currentTimedActions.Count > 0 && _currentTimedActions[0].Time <= _timer)
            {
                _currentTimedActions[0].Action.Invoke();

                _currentTimedActions.RemoveAt(0);
            }
        }

        public void Start()
        {
            _timer = 0f;
            _triggered = true;
            _currentTimedActions = new List<TimedAction>(_timedActions);

            Update(0f);

        }

        public void Cancel()
        {
            _triggered = false;
        }

        public bool IsInProgress()
        {
            return _triggered && _currentTimedActions.Count > 0;
        }
    }
}