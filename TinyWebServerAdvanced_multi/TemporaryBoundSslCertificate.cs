using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompuMaster.Web.TinyWebServerAdvanced
{
    public class TemporaryBoundSslCertificate : IDisposable
    {

        public readonly System.Security.Cryptography.X509Certificates.X509Certificate2 TemporaryCertificate;
        public readonly Guid TempAppGuid = new Guid();
        private readonly CertificateStoreUtil CertificateStore = new CertificateStoreUtil();


        public TemporaryBoundSslCertificate(string[] ipPorts)
        {
            var httpsIPs = new List<string>();
            var httpsIPEndPoints = new List<string>();

            //Ensure at least 1 IPEndPoint
            if (ipPorts == null || ipPorts.Length == 0)
            {
                ipPorts = new string[] { "0.0.0.0:8443" };
            }

            foreach (string ipPort in ipPorts)
            {
                if (ipPort.StartsWith("["))
                {
                    httpsIPs.Add(ipPort.Substring(1, ipPort.IndexOf("]")-1));
                }
                else
                {
                    if (ipPort.Contains(":"))
                        httpsIPs.Add(ipPort.Substring(0, ipPort.IndexOf(":")));
                    else
                        httpsIPs.Add(ipPort);
                }
            }

            //Create a self-signed cert
            TemporaryCertificate = CertificateStore.MakeSelfSignedWebserverCert(httpsIPs.ToArray(), new string[] { "localhost", Environment.MachineName });

            //install the new cert into the cert store of localmachine at personal certs folder
            CertificateStore.InstallCert(TemporaryCertificate);

            //bind the cert to the required HTTPS ip-ports
            foreach (string ipPort in ipPorts)
            {
                System.Net.IPEndPoint ipPortEndPoint = CompuMaster.Web.TinyWebServerAdvanced.CertificateStoreUtil.IpEndpointTools.ParseIpEndPoint(ipPort);
                CompuMaster.Web.TinyWebServerAdvanced.CertificateUtil.AddCertificateBinding(TemporaryCertificate.Thumbprint, ipPortEndPoint, TempAppGuid);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // Dient zur Erkennung redundanter Aufrufe.

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (TemporaryCertificate != null)
                        CertificateStore.RemoveCert(TemporaryCertificate.Thumbprint);
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
            Dispose(true);
        }
        #endregion

        ~TemporaryBoundSslCertificate()
        {
            this.Dispose();
        }
    }
}
