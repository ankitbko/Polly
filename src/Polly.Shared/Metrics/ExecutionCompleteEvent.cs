using System;

namespace Polly.Metrics
{
    public class ExecutionCompleteEvent : PollyEvent
    {
        public OutcomeType Outcome { get; set; }

        public static long[] AppendEventToBucket(long[] initialCountArray, ExecutionCompleteEvent pollyEvent)
        {
            initialCountArray[(int)pollyEvent.Outcome]++;
            return initialCountArray;
        }
    }
}