using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
//using System.ServiceModel;
using SslCertBinding.Net;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace CompuMaster.Web.TinyWebServerAdvanced
{
    public class CertificateStoreUtil : IDisposable
    {
        public readonly string IssuerDistinguishedName = "CN=CMTinyWebServerAdvanced";

        public X509Certificate2 MakeSelfSignedWebserverCert(string[] ipAddresses, string[] dnsNames)
        {
            var rsa = RSA.Create("2048"); // generate asymmetric key pair
            var req = new CertificateRequest(IssuerDistinguishedName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            req.CertificateExtensions.Add(
                new X509KeyUsageExtension(
                    X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.DataEncipherment | X509KeyUsageFlags.KeyEncipherment,
                    false));
            req.CertificateExtensions.Add(
                new X509EnhancedKeyUsageExtension(
                    new OidCollection
                    {
                    new Oid("1.3.6.1.5.5.7.3.1")
                    },
                    false));
            
            var altNames = new SubjectAlternativeNameBuilder();
            foreach (string ipAddress in ipAddresses)
                altNames.AddIpAddress(IPAddress.Parse(ipAddress));
            foreach (string dnsName in dnsNames)
                altNames.AddDnsName(dnsName);
            req.CertificateExtensions.Add(altNames.Build());
            var cert = req.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(25));
            cert.FriendlyName = "CompuMaster TinyWebServerAdvanced Self-Signed";
            byte[] certPfxExportWithPrivateKeyData = cert.Export(X509ContentType.Pfx);
            //File.WriteAllBytes("d:\\temp\\mycert.pfx", cert.Export(X509ContentType.Pfx)); 
            cert = new X509Certificate2(certPfxExportWithPrivateKeyData,"", X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);
            return cert;
        }
        public void InstallCert(X509Certificate2 cert)
        {
            DoInLocalMachineCertStores(certStore => {
                certStore.Add(cert);
            });
        }


        public void RemoveCert(string thumbprint)
        {
            CertificateUtil.RemoveCertificateBindingToIpPort(thumbprint);

            DoInLocalMachineCertStores(certStore => {
                var certs = certStore.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
                certStore.RemoveRange(certs);
            });
        }


        private void DoInLocalMachineCertStores(Action<X509Store> action)
        {
            var storeNames = new[] { StoreName.My, StoreName.AuthRoot, };
            foreach (var storeName in storeNames)
            {
                var store = new X509Store(storeName, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadWrite | OpenFlags.OpenExistingOnly | OpenFlags.MaxAllowed);
                action(store);
                store.Close();
            }
        }
       
        public void CleanupCertsOfIssuerDistinguishedName()
        {
            DoInLocalMachineCertStores(certStore => {
                var certs = certStore.Certificates.Find(X509FindType.FindByIssuerDistinguishedName, IssuerDistinguishedName, false);
                
                //remove all endpoints from all found certs thumbprints
                foreach (X509Certificate2 cert in certs)
                {
                    var CertificateStore = new CertificateStoreUtil();
                    CertificateStore.RemoveCert(cert.Thumbprint);
                }

                //remove all certs
                certStore.RemoveRange(certs);
            });
        }
      
        public static class IpEndpointTools
        {
            public static bool IpEndpointIsAvailableForListening(IPEndPoint ipPort)
            {
                IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
                IPEndPoint[] tcpEndPointArray = ipGlobalProperties.GetActiveTcpListeners();
                return !tcpEndPointArray.Contains(ipPort);
            }

            public static IPEndPoint ParseIpEndPoint(string str)
            {
                string ip;
                string port;
                if (str.StartsWith("["))
                {
                    ip = str.Split(']')[0].Substring(1);
                    port = str.Split(']')[1].Substring(1);
                }
                else
                {
                    ip = str.Split(':')[0];
                    port = str.Split(':')[1];
                }
                return new IPEndPoint(IPAddress.Parse(ip), int.Parse(port));
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
                    this.CleanupCertsOfIssuerDistinguishedName();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        ~CertificateStoreUtil()
        {
            this.Dispose();
        }
    }
}
