using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ApiGateway.Models
{
    public class NotifierStreamContent : StreamContent
    {
        public NotifierStreamContent(Stream stream) : base (stream) { }
        public NotifierStreamContent(Stream stream, Action<Exception> onComplete) : base(stream) 
        { 
            this.OnComplete = onComplete; 
        }

        public Action<Exception> OnComplete { get; set; }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            var t = base.SerializeToStreamAsync(stream, context);
            t.ContinueWith(x =>
            {
                if (this.OnComplete != null)
                {
                    if (x.IsFaulted)
                        this.OnComplete(x.Exception.GetBaseException());
                    else
                        this.OnComplete(null);
                }
            }, TaskContinuationOptions.ExecuteSynchronously);
            return t;
        }
    }
}