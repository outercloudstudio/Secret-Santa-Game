using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

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

    public class WeightedRandom<ValueType>
    {
        public struct Element
        {
            public float Weight;
            public ValueType Value;
        }

        public List<Element> Elements;

        public WeightedRandom(List<Element> elements)
        {
            Elements = elements;
        }

        public Element GetElement()
        {
            float sum = 0;

            foreach (Element element in Elements)
            {
                sum += element.Weight;
            }

            float target = GD.Randf() * sum;

            float indexSum = 0;

            for (int index = 0; index < Elements.Count; index++)
            {
                indexSum += Elements[index].Weight;

                if (indexSum > target) return Elements[index];
            }

            return Elements[Elements.Count - 1];
        }

        public ValueType Get()
        {
            return GetElement().Value;
        }
    }

    public class ProceduralWeightedRandom<ValueType>
    {
        public List<ValueType> Choices;
        private Func<ValueType, float> _weight;

        public ProceduralWeightedRandom(List<ValueType> list, Func<ValueType, float> weight)
        {
            Choices = list;
            _weight = weight;
        }

        public ValueType Get()
        {
            float sum = 0;

            foreach (ValueType choice in Choices)
            {
                sum += _weight(choice);
            }

            float target = GD.Randf() * sum;

            float indexSum = 0;

            for (int index = 0; index < Choices.Count; index++)
            {
                indexSum += _weight(Choices[index]);

                if (indexSum > target) return Choices[index];
            }

            return Choices[Choices.Count - 1];
        }
    }
}