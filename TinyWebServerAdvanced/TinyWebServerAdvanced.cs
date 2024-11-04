using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace CompuMaster.Web.TinyWebServerAdvanced
{
    /// <summary> 
    /// Listens for the specified request, and executes the given handler
    /// </summary>
    /// <example>
    /// Examplary simple start: 
    /// <code>
    /// var server = new WebServer(request => { return "<h1>Hello world!</h1>"; }, "http://localhost:8080/hello/");
    /// server.Run();
    /// ....
    /// server.Stop();
    /// </code>
    /// </example>
    public class WebServer
    {
        private HttpListener _listener;
#if NET_FRAMEWORK 
        private CompuMaster.Web.TinyWebServerAdvanced.TemporaryBoundSslCertificate _tempSslCertificate;
#endif
        private readonly HttpRequestHandlerFactory _requestHandlerFactory = null;
        private readonly HttpRequestContextHandlerFactory _requestContextHandlerFactory = null;
        private readonly Func<HttpListenerRequest, string> _contentHandler = null;
        private readonly Func<HttpListenerRequest, System.Collections.Specialized.NameValueCollection> _responseHeadersHandler;

        private bool _disposed 
        { 
            get
            {
                return _listener == null;
            } 
        }

        private void ListenerSetup(string[] urls)
        {
            _listener = new HttpListener();
            if (urls == null || urls.Length == 0)
                throw new ArgumentException("prefixes");

            foreach (string s in UrlsWithStarExploded(urls))
            {
                _listener.Prefixes.Add(s);
            }
        }

        private void ListenerStart()
        {
            _listener.Start();
#if NET_FRAMEWORK
            var httpsIpPorts = new System.Collections.Generic.List<string>();
            foreach (string prefix in _listener.Prefixes)
            {
                if (prefix.ToLowerInvariant().StartsWith("https://"))
                {
                    var ipPort = prefix.Substring("https://".Length);
                    ipPort = ipPort.Substring(0, ipPort.IndexOf("/"));
                    httpsIpPorts.Add(ipPort);
                }
            }
            if (httpsIpPorts.Count != 0)
                _tempSslCertificate = new CompuMaster.Web.TinyWebServerAdvanced.TemporaryBoundSslCertificate(httpsIpPorts.ToArray());
#endif
        }

        public WebServer(Func<HttpListenerRequest, string> handler, params string[] urls)
        {
            if (handler == null)
                throw new ArgumentException("method");
            _urlsToListen = urls;
            _contentHandler = handler;
        }

        public WebServer(Func<HttpListenerRequest, string> contentHandler, Func<HttpListenerRequest, System.Collections.Specialized.NameValueCollection> responseHeadersHandler, params string[] urls)
        {
            if (contentHandler == null)
                throw new ArgumentException("method");
            _urlsToListen = urls;
            _contentHandler = contentHandler;
            _responseHeadersHandler = responseHeadersHandler;
        }

        public WebServer(HttpRequestHandlerFactory requestHandlerFactory,  params string[] urls)
        {
            if (requestHandlerFactory == null)
                throw new ArgumentException("method");
            _urlsToListen = urls;
            _requestHandlerFactory = requestHandlerFactory;
        }


        public WebServer(HttpRequestContextHandlerFactory requestContextHandlerFactory, params string[] urls)
        {
            if (requestContextHandlerFactory == null)
                throw new ArgumentException("method");
            _urlsToListen = urls;
            _requestContextHandlerFactory = requestContextHandlerFactory;
        }

        public string[] Prefixes()
        {
            if (_listener == null)
                return _urlsToListen;
            else
                return _listener.Prefixes.ToArray();
        }

        private string[] _urlsToListen { get; set; }

        string[] UrlsWithStarExploded(string[] urls)
        {
            var _result = new System.Collections.Generic.List<string>();
            foreach (string url in urls)
            {
                if (!url.Contains("*"))
                {
                    _result.Add(url);
                }
                else
                {
                    var IPs = new System.Collections.Generic.List<string>();
                    bool IPv4Support = false;
                    bool IPv6Support = false;
                    foreach (System.Net.NetworkInformation.NetworkInterface ni in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
                        if (ni.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up && (ni.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Wireless80211 | ni.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Ethernet))
                        {
                            foreach (System.Net.NetworkInformation.UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                            {
                                if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                                {
                                    IPv4Support = true;
                                    IPs.Add(ip.Address.ToString());
                                }
                                else if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                                {
                                    IPv6Support = true;
                                    IPs.Add("[" + StripScopeIdFromIpV6Address(ip.Address.ToString()) + "]");
                                }

                            }
                        }
                    if (IPv4Support && !(IPs.Contains("127.0.0.1"))) IPs.Add("127.0.0.1");
                    if (IPv6Support && !(IPs.Contains("[::1]"))) IPs.Add("[::1]");
                    foreach (string IP in IPs)
                        _result.Add(url.Replace("*", IP));                  
                }
            }
            return _result.ToArray();
        }

        private string StripScopeIdFromIpV6Address(string ipV6Address)
        {
            if (ipV6Address.Contains("%"))
                return ipV6Address.Substring(0, ipV6Address.IndexOf("%"));
            else
                return ipV6Address;
        }

        public void Run()
        {
            if (!_disposed && this._listener.IsListening)
            {
                throw new Exception("Webserver already running");
            }
            else
            {
                this.ListenerSetup(_urlsToListen);
                this.ListenerStart();
                ThreadPool.QueueUserWorkItem(o =>
                {
                    while (_listener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem(c =>
                        {
                            var ctx = c as HttpListenerContext;
                            if (ctx != null)
                            {
                                try
                                {
                                    if (_requestContextHandlerFactory != null)
                                    {
                                        HttpRequestContextHandler _currentRequestContextHandler = _requestContextHandlerFactory.CreateRequestHandlerInstance(ctx);
                                        _currentRequestContextHandler.GetResponse();
                                    }
                                    else if (_requestHandlerFactory != null)
                                    {
                                        HttpRequestHandler _currentRequestHandler = _requestHandlerFactory.CreateRequestHandlerInstance(ctx.Request);
                                        ctx.Response.StatusCode = _currentRequestHandler.GetResponseHttpCode();
                                        var buf = Encoding.UTF8.GetBytes(_currentRequestHandler.GetResponseContent());
                                        System.Collections.Specialized.NameValueCollection _responseHeaders = _currentRequestHandler.GetResponseHeaders();
                                        foreach (string s in _responseHeaders)
                                            ctx.Response.Headers[s] = _responseHeaders[s];
                                        ctx.Response.ContentLength64 = buf.Length;
                                        ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                                    }
                                    else
                                    {
                                        var responseStr = _contentHandler(ctx.Request);
                                        System.Collections.Specialized.NameValueCollection _responseHeaders = _responseHeadersHandler(ctx.Request);
                                        var buf = Encoding.UTF8.GetBytes(responseStr);
                                        foreach (string s in _responseHeaders)
                                            ctx.Response.Headers[s] = _responseHeaders[s];
                                        ctx.Response.ContentLength64 = buf.Length;
                                        ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                                    }
                                }
                                finally
                                {
                                    ctx.Response.OutputStream.Close();
                                }
                            }
                        }, _listener.GetContext());
                    }
                });
            }
        }

        public void Stop()
        {
            if (!_disposed)
            {
                _listener.Stop();
                _listener.Close();
                _listener = null;
            }
        }

        ~WebServer()
        {
            Stop();
        }
    }
}