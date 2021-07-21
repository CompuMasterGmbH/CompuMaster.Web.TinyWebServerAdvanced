using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CompuMaster.Web.TinyWebServerAdvanced
{
    public abstract class HttpRequestHandlerFactory
    {
        public abstract HttpRequestHandler CreateRequestHandlerInstance(HttpListenerRequest request);
    }
}
