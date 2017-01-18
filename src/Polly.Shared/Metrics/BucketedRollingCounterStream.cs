using Polly.CircuitBreaker;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polly.Metrics
{
    public abstract class BucketedRollingCounterStream<Event, Bucket, Output>: BucketedCounterStream<Event, Bucket, Output> where Event: PollyEvent
    {
        private IObservable<Output> sourceStream;

        public BucketedRollingCounterStream(IEventStream<Event> stream, TimeSpan samplingDuration, 
            short numberOfWindows, Func<Bucket, Event, Bucket> appendRawEventToBucket,
            Func<Output, Bucket, Output> reduceBucket) 
            : base(stream, samplingDuration, numberOfWindows, appendRawEventToBucket)
        {
            Func<IObservable<Bucket>, IObservable<Output>> reduceWindowToSummary =
                window => window.Scan(EmptyOutputValue(), reduceBucket)
                                .Skip(numberOfWindows);

            sourceStream = bucketedStream
                            .Do(t => Debug.WriteLine("Rolling: " + t.ToString()))
                            .Window(numberOfWindows, 1)
                            .Do(t => Debug.WriteLine("Rolling after window : " + t.ToString()))
                            .SelectMany(reduceWindowToSummary)
                            .Publish()
                            .RefCount();
        }

        public override IObservable<Output> Observe() => sourceStream;
    }
}
