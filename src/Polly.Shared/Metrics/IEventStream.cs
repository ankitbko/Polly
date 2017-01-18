using System;

namespace Polly.Metrics
{
    public interface IEventStream<Event>
    {
        IObservable<Event> Observe();
    }
}
