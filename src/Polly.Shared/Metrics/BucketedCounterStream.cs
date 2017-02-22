using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Diagnostics;

namespace Polly.Metrics
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class BucketedCounterStream<Event, Bucket, Output> : IDisposable where Event : PollyEvent
    {
        protected readonly TimeSpan samplingDuration;
        protected readonly short numberOfWindows;
        protected IObservable<Bucket> bucketedStream;

        private Func<IObservable<Event>, IObservable<Bucket>> reduceBucketToSummary;
        private BehaviorSubject<Output> counterSubject;
        private IDisposable subscription;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="samplingDuration"></param>
        /// <param name="numberOfWindows"></param>
        public BucketedCounterStream(IEventStream<Event> stream, TimeSpan samplingDuration, short numberOfWindows, Func<Bucket, Event, Bucket> appendRawEventToBucket)
        {
            this.samplingDuration = samplingDuration;
            this.numberOfWindows = numberOfWindows;
            var bucketSize = TimeSpan.FromMilliseconds(samplingDuration.TotalMilliseconds / numberOfWindows);
            counterSubject = new BehaviorSubject<Output>(EmptyOutputValue());
            reduceBucketToSummary = (eventBucket) => eventBucket.Aggregate(EmptyBucketValue(), appendRawEventToBucket);

            List<Bucket> emptyBucketsToStart = new List<Bucket>();
            for (int i = 0; i < numberOfWindows; i++)
            {
                emptyBucketsToStart.Add(EmptyBucketValue());
            }

            bucketedStream = Observable.Defer(
                () => stream
                    .Observe()
                    .Window(bucketSize)
                    .SelectMany(reduceBucketToSummary)
                    .StartWith(emptyBucketsToStart));
        }

        public Output GetLatest()
        {
            StartCachingStreamValuesIfUnstarted();
            Output value;
            if (counterSubject.TryGetValue(out value))
            {
                return value;
            }
            else
            {
                return EmptyOutputValue();
            }
        }

        protected void StartCachingStreamValuesIfUnstarted()
        {
            if (subscription == null)
            {
                subscription = Observe().Subscribe(counterSubject);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal abstract Bucket EmptyBucketValue();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal abstract Output EmptyOutputValue();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract IObservable<Output> Observe();

        public void Dispose()
        {
            if (subscription != null)
                subscription.Dispose();
            if (counterSubject != null)
                counterSubject.Dispose();
        }
    }
}
