using System;
using System.Collections.Generic;
using Common.Enums;
using Common.ServiceLocator;
namespace Common.EventsSystem
{
    public class EventManager : ILocatableService
    {
        public Action<EGameState> OnGameStateChanged;
        
        private readonly Dictionary<EGameEvent, object> _events = new();
        private EGameState _gameStatus;

        public Event<T> GetEvent<T>(EGameEvent eventName)
        {
            if (_events.TryGetValue(eventName, out var e) && e is Event<T> typedEvent)
            {
                return typedEvent;
            }

            var newEvent = new Event<T>();
            _events[eventName] = newEvent;
            return newEvent;
        }

        public Event GetEvent(EGameEvent eventName)
        {
            if (_events.TryGetValue(eventName, out var e) && e is Event typedEvent)
            {
                return typedEvent;
            }

            var newEvent = new Event();
            _events[eventName] = newEvent;
            return newEvent;
        }

        public Dictionary<EGameEvent, object> GetSubscribedEvents()
        {
            return _events;
        }

        public EGameState GameStatus
        {
            get => _gameStatus;
            set
            {
                _gameStatus = value;
                OnGameStateChanged?.Invoke(_gameStatus);
            }
        }
    }
    
    public class Event
    {
        private event Action _event;

        public void Subscribe(Action subscriber)
        {
            _event += subscriber;
        }

        public void Unsubscribe(Action subscriber)
        {
            _event -= subscriber;
        }

        public void Invoke()
        {
            _event?.Invoke();
        }
    }
}