using System;

namespace Polly.CircuitBreaker
{
    public class HealthCount
    {
        public int Successes { get; set; }

        public int Failures { get; set; }

        public int Total { get { return Successes + Failures; } }

        public long StartedAt { get; set; }

        internal HealthCount Accumulate(long[] bucketEventCounts)
        {
            var failures = Failures;
            var successes = Successes;
            failures += (int)bucketEventCounts[1];
            successes += (int)bucketEventCounts[0];
            return new HealthCount { Failures = failures, Successes = successes};
        }
    }
}
