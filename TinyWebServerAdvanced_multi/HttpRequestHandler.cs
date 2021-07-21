using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CompuMaster.Web.TinyWebServerAdvanced
{
    public abstract class HttpRequestHandler
    {
        public readonly HttpListenerRequest Request;
        public HttpRequestHandler(HttpListenerRequest request)
        {
            this.Request = request;
        }
        public abstract string GetResponseContent();
        public abstract System.Collections.Specialized.NameValueCollection GetResponseHeaders();
        public abstract int GetResponseHttpCode();
    }
}
