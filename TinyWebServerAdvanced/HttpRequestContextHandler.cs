using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CompuMaster.Web.TinyWebServerAdvanced
{
    public abstract class HttpRequestContextHandler
    {
        public readonly HttpListenerContext Context;
        public HttpRequestContextHandler(HttpListenerContext context)
        {
            this.Context = context;
        }
        public abstract void GetResponse();
    }
}
