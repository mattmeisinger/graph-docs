using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class CompletedAsyncResult<T> : IAsyncResult
    {

        public static T End(IAsyncResult result)
        {
            return ((CompletedAsyncResult<T>)result).value;
        }

        WaitHandle wh = new ManualResetEvent(true);
        T value;
        AsyncCallback callback;
        object state;

        public CompletedAsyncResult(T value, AsyncCallback callback, object state)
        {
            this.value = value;
            this.callback = callback;
            this.state = state;
        }

        public object AsyncState
        {
            get { return state; }
        }

        public WaitHandle AsyncWaitHandle
        {
            get { return wh; }
        }

        public bool CompletedSynchronously
        {
            get { return true; }
        }

        public bool IsCompleted
        {
            get { return true; }
        }

        public AsyncCallback AsyncCallback
        {
            get { return callback; }
        }
    }
}
