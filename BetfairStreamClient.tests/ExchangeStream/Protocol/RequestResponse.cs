using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetfairStreamClient.tests.ExchangeStream.Protocol
{
    /// <summary>
    /// Wraps a standard completion source to create a pairing of request message to status message
    /// </summary>
    public class RequestResponse
    {
        private readonly TaskCompletionSource<Model.StatusMessage> _completionSource = new TaskCompletionSource<Model.StatusMessage>();
        public readonly Model.RequestMessage Request;
        public Action<RequestResponse> OnSuccess { get; set; }

        public RequestResponse(int id, Model.RequestMessage request, Action<RequestResponse> onSuccess)
        {
            Id = id;
            Request = request;
            OnSuccess = onSuccess;
        }

        public void ProcesStatusMessage(Model.StatusMessage statusMessage)
        {
            if (statusMessage.StatusCode == Model.StatusMessage.StatusCodeEnum.Success)
            {
                if (OnSuccess != null) OnSuccess(this);
            }
            _completionSource.TrySetResult(statusMessage);
        }

        public Model.StatusMessage Result
        {
            get
            {
                return _completionSource.Task.Result;
            }
        }

        public int Id { get; private set; }

        public Task<Model.StatusMessage> Task
        {
            get
            {
                return _completionSource.Task;
            }
        }

        internal void Cancelled()
        {
            _completionSource.TrySetCanceled();
        }
    }
}
