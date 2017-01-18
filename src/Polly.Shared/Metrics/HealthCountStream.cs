using Polly.CircuitBreaker;
using System;
using System.Collections.Generic;
using System.Text;

namespace Polly.Metrics
{
    /// <summary>
    /// 
    /// </summary>
    public class HealthCountStream : BucketedRollingCounterStream<ExecutionCompleteEvent, long[], HealthCount>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="samplingDuration"></param>
        /// <param name="numberOfWindows"></param>
        /// <param name="appendRawEventToBucket"></param>
        /// <param name="reduceBucket"></param>
        public HealthCountStream(IEventStream<ExecutionCompleteEvent> stream, TimeSpan samplingDuration, short numberOfWindows) 
            : base(stream, samplingDuration, numberOfWindows, ExecutionCompleteEvent.AppendEventToBucket, healthCheckAccumulator)
        {
        }

        private static Func<HealthCount, long[], HealthCount> healthCheckAccumulator =
            (healthCount, bucketEventCounts) => healthCount.Accumulate(bucketEventCounts);

        internal override long[] EmptyBucketValue() => new long[2];

        internal override HealthCount EmptyOutputValue() => new HealthCount();
    }
}
