using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;

namespace BinaryKits.Zpl.Labelary
{
    public class LabelaryClient : IDisposable
    {
        private readonly string _apiEndpoint;
        private bool _disposed;

        public LabelaryClient(
            string apiEndpoint = "http://api.labelary.com/v1/printers")
        {
            _apiEndpoint = apiEndpoint;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
        }

        /// <summary>
        /// Get a label preview image from the Labelary API
        /// </summary>
        public byte[] GetPreview(
            string zplData,
            PrintDensity printDensity,
            LabelSize labelSize)
        {
            var dpi = printDensity.ToString().Substring(2);
            var zpl = Encoding.UTF8.GetBytes(zplData);

            //without setting this, a comma separator might be used for the size numbers in the URL
            var specifier = "G";
            var culture = CultureInfo.CreateSpecificCulture("en-US");
            var width = labelSize.WidthInInch.ToString(specifier, culture);
            var height = labelSize.HeightInInch.ToString(specifier, culture);

            var url = string.Format("{0}/{1}/labels/{2}x{3}/0/", _apiEndpoint, dpi, width, height);
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = zpl.Length;

            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(zpl, 0, zpl.Length);
            }

            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    ((IDisposable)ex.Response).Dispose();
                }
                return new byte[0];
            }

            using (response)
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return new byte[0];
                }

                using (var responseStream = response.GetResponseStream())
                using (var memoryStream = new MemoryStream())
                {
                    responseStream.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
        }
    }
}
