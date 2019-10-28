using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using static FunctionalCore.ExtensionFuture;

namespace FunctionalCore
{
    public class SessionServices
    {
        public void SimpleWebRequest(string urlAddress, Action<Result<byte[], Exception>> completion)
        {

            ServicePointManager.ServerCertificateValidationCallback += ServerCertificateValidationCallback;

            WebClient client = new WebClient();

            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            client.Credentials = CredentialCache.DefaultCredentials;

            byte[] dataBuffer = client.DownloadData(urlAddress);

            Result<byte[], Exception> data = new Result<byte[], Exception>(dataBuffer);
            completion(data);

            /*System.Net.ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            HttpWebRequest request = WebRequest.CreateHttp(urlAddress);
            request.UseDefaultCredentials = true;
            request.UserAgent = "Code Sample Web Client";
            request.ServerCertificateValidationCallback += ServerCertificateValidationCallback;

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                BinaryReader breader = new BinaryReader(receiveStream);
                byte[] buffer = breader.ReadBytes((int)response.ContentLength);
                Result<byte[], Exception> data = new Result<byte[], Exception>(buffer);
                completion(data);
            }
            else
            {
                completion(new Result<byte[], Exception>(new Exception("¡Error en la descarga!")));
            }*/
        }

        public FutureResult<byte[], Exception> get(Uri url)
        {
            return new FutureResult<byte[], Exception>(
                (callback) => SimpleWebRequest(url.ToString(), callback)
                );
        }

        /// <summary>
        /// ERRORRRRRRRRRRRRRRRRRRR
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="sslPolicyErrors"></param>
        /// <returns></returns>
        private static bool ServerCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
            /*if (sslPolicyErrors == SslPolicyErrors.None)
            {
                Console.WriteLine("Certificate OK");
                return true;
            }
            else
            {
                Console.WriteLine("Certificate ERROR");
                Console.WriteLine("X509Certificate [{0}] Policy Error: '{1}'",
                    certificate.Subject,
                    sslPolicyErrors.ToString());
                return false;
            }*/
        }
    }
}
