using Polly.CircuitBreaker;
using System;
using System.Collections.Generic;
using System.Text;

namespace Polly.Metrics
{
    public class BucketHealthMetrics : IHealthMetrics, IDisposable
    {
        private readonly TimeSpan _samplingDuration;
        private readonly short _numberOfWindows;
        private HealthCountStream _stream;
        private ExecutionCompleteEventStream _eventStream;

        public BucketHealthMetrics(TimeSpan samplingDuration, short numberOfWindows)
        {
            _samplingDuration = samplingDuration;
            _numberOfWindows = numberOfWindows;
            _eventStream = new ExecutionCompleteEventStream();
            _stream = new HealthCountStream(_eventStream, samplingDuration, numberOfWindows);

        }
        public HealthCount GetHealthCount_NeedsLock()
        {
            return _stream.GetLatest();
        }

        public void IncrementFailure_NeedsLock()
        {
            _eventStream.Write(new ExecutionCompleteEvent() { Outcome = OutcomeType.Failure, ExecutionKey= 1 });
        }

        public void IncrementSuccess_NeedsLock()
        {
            _eventStream.Write(new ExecutionCompleteEvent() { Outcome = OutcomeType.Successful, ExecutionKey = 1 });
        }

        public void Reset_NeedsLock()
        {
            Dispose();
            _eventStream = new ExecutionCompleteEventStream();
            _stream = new HealthCountStream(_eventStream, _samplingDuration, _numberOfWindows);
        }

        public void Dispose()
        {
            if (_stream != null)
                _stream.Dispose();
            if (_eventStream != null)
                _eventStream.Dispose();
        }
    }
}
