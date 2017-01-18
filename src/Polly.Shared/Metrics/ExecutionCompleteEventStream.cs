using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text;
using System.Reactive.Linq;
using System.Diagnostics;

namespace Polly.Metrics
{
    public class ExecutionCompleteEventStream : IEventStream<ExecutionCompleteEvent>, IDisposable
    {
        private Subject<ExecutionCompleteEvent> writeOnlySubject;
        private IObservable<ExecutionCompleteEvent> readOnlyStream;

        public ExecutionCompleteEventStream()
        {
            writeOnlySubject = new Subject<ExecutionCompleteEvent>();
            //writeOnlySubject = new BehaviorSubject<ExecutionCompleteEvent>(new ExecutionCompleteEvent() { Outcome = OutcomeType.Successful });
            readOnlyStream = writeOnlySubject.Publish().RefCount();
        }

        public IObservable<ExecutionCompleteEvent> Observe() => readOnlyStream.Do(e => Debug.WriteLine("readOnly: " + e.Outcome));

        public void Write(ExecutionCompleteEvent pollyEvent)
        {
            writeOnlySubject.OnNext(pollyEvent);
        }

        public void Dispose()
        {
            if (writeOnlySubject != null)
                writeOnlySubject.Dispose();
        }
    }
}
