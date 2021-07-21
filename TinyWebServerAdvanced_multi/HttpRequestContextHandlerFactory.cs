using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CompuMaster.Web.TinyWebServerAdvanced
{
    public abstract class HttpRequestContextHandlerFactory
    {
        public abstract HttpRequestContextHandler CreateRequestHandlerInstance(HttpListenerContext context);
    }
}
