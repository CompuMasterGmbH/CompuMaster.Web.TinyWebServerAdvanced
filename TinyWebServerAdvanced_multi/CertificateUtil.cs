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
    public class CertificateUtil
    {
        public readonly static ICertificateBindingConfiguration config = new CertificateBindingConfiguration();

        public static void AddCertificateBinding(string certificateThumbprint, System.Net.IPEndPoint ipPort, Guid appId)
        {
            // add a new binding record
            var certBinding = new CertificateBinding(certificateThumbprint, StoreName.My, ipPort, appId);
            config.Bind(certBinding); //returns false
        }

        public static CertificateBinding GetCertificateBinding(System.Net.IPEndPoint ipPort)
        {
            // get a binding record
            return config.Query(ipPort)[0];
        }

        public static void DoNotVerifyCertificateRevocation(CertificateBinding certificateBinding)
        {
            // set an option and update the binding record
            certificateBinding.Options.DoNotVerifyCertificateRevocation = true;
            config.Bind(certificateBinding); //returns true
        }

        public static void RemoveCertificateBindingToIpPort(System.Net.IPEndPoint ipPort)
        {
            // remove the binding record
            config.Delete(ipPort);
        }

        public static void RemoveCertificateBindingToIpPort(string thumbprint)
        {
            CertificateBinding[] AllCertBindings = config.Query();
            foreach (CertificateBinding certBinding in AllCertBindings)
            {
                if (certBinding.Thumbprint.ToLowerInvariant() == thumbprint.ToLowerInvariant())
                {
                    // remove the binding record
                    config.Delete(certBinding.IpPort);
                }
            }
        }
    }
}