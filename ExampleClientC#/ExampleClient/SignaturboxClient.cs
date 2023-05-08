using RestSharp;
using System.Net;
using System.Text;

namespace ExampleClient
{
    public class SignaturboxClient
    {

        private RestClient client;
        private string apiKey;

        public SignaturboxClient(string baseUrl, string apiKey)
        {
            client = new RestClient(baseUrl);
            this.apiKey = apiKey;
        }

        public string? StartBatchSignature(string redirectUrl, string errorUrl)
        {
            RestRequest request = GetBaseRequest("signaturebatches", Method.Post);
            request.AddParameter("RedirectUrl", redirectUrl);
            request.AddParameter("ErrorUrl", errorUrl);
            var response = client.Execute(request);
            if (response.StatusCode == HttpStatusCode.Created) 
            {
                return GetLastPartFromLocationHeader(response);
            }
            return null;
        }

        public string? EndBatchSignature(string ticket, string handySigParameter)
        {
            RestRequest request = GetBaseRequest("signaturebatches/{ticket}/mobileSignature", Method.Post);
            request.AddUrlSegment("ticket", ticket);
            request.AddParameter("handySigParameter", handySigParameter);
            RestResponse response = client.Execute(request);
            if (response.StatusCode == HttpStatusCode.Created)
            {
                return GetHeader(response, "Location");
            }
            return null;
        }

        public int AddDocument(string ticket, string file, string location, string reason)
        {
            RestRequest request = GetBaseRequest("signaturebatches/{ticket}/documents", Method.Post);
            request.AddUrlSegment("ticket", ticket);
            request.AddParameter("location", location);
            request.AddParameter("reason", reason);
            request.AddFile(Path.GetFileName(file), file);
            RestResponse response = client.Execute(request);
            int documentId = -1;
            if (response.StatusCode == HttpStatusCode.Created)
            {
                documentId = GetLastPartFromLocationHeaderAsInt(response);
            }
            return documentId;
        }

        public int AddDocumentTemplate(string ticket, string file, int templateId, string location, string reason)
        {
            RestRequest request = GetBaseRequest("signaturebatches/{ticket}/documents/", Method.Post);
            request.AddUrlSegment("ticket", ticket);
            request.AddParameter("location", location);
            request.AddParameter("reason", reason);
            request.AddParameter("template", templateId.ToString());
            request.AddFile(Path.GetFileName(file), file);
            RestResponse response = client.Execute(request);
            int documentId = -1;
            if (response.StatusCode == HttpStatusCode.Created)
            {
                documentId = GetLastPartFromLocationHeaderAsInt(response);
            }
            return documentId;
        }

        public int AddDocumentTemplateEx(string ticket, string file, int templateId, string location, string reason, int page, int x, int y, int w, int h)
        {
            RestRequest request = GetBaseRequest("signaturebatches/{ticket}/documents/", Method.Post);
            request.AddUrlSegment("ticket", ticket);
            request.AddParameter("location", location);
            request.AddParameter("reason", reason);
            request.AddParameter("template", templateId.ToString());
            request.AddParameter("page", page.ToString());
            request.AddParameter("x", x.ToString());
            request.AddParameter("y", y.ToString());
            request.AddParameter("w", w.ToString());
            request.AddParameter("h", h.ToString());
            request.AddFile(Path.GetFileName(file), file);
            RestResponse response = client.Execute(request);
            int documentId = -1;
            if (response.StatusCode == HttpStatusCode.Created)
            {
                documentId = GetLastPartFromLocationHeaderAsInt(response);
            }
            return documentId;
        }

        public bool GetDocument(string ticket, int documentId, out string? documentName, out byte[]? documentData)
        {
            RestRequest request = GetBaseRequest("signaturebatches/{ticket}/documents/{id}", Method.Delete);
            request.AddUrlSegment("ticket", ticket);
            request.AddUrlSegment("id", documentId.ToString());
            RestResponse response = client.Execute(request);
            bool success = false;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string cd = GetContentHeader(response, "Content-Disposition");
                string fn = cd.Substring(cd.IndexOf("filename=") + 9);
                documentData = response.RawBytes;
                documentName = fn.Split(';')[0];
                success = true;
            }
            else 
            {
                documentData = null;
                documentName = null;
            }
            return success;
        }

        public int UploadTemplate(string file)
        {
            RestRequest request = GetBaseRequest("templates", Method.Post);
            request.AddFile(Path.GetFileName(file), file);
            RestResponse response = client.Execute(request);
            int templateId = -1;
            if (response.StatusCode == HttpStatusCode.Created)
            {
                templateId = GetLastPartFromLocationHeaderAsInt(response);
            }
            return templateId;
        }

        public bool ReplaceTemplate(string file, int templateId)
        {
            RestRequest request = GetBaseRequest("templates/{id}", Method.Put);
            request.AddUrlSegment("id", templateId.ToString());
            request.AddFile(Path.GetFileName(file), file);
            RestResponse response = client.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }
            return false;
        }

        public bool DeleteTemplate(int templateId)
        {
            RestRequest request = GetBaseRequest("templates/{id}", Method.Post);
            request.AddUrlSegment("id", templateId.ToString());
            RestResponse response = client.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }
            return false;
        }

        public byte[]? GetTemplate(int templateId)
        {
            var request = GetBaseRequest("templates/{id}", Method.Get);
            request.AddUrlSegment("id", templateId.ToString());
            var response = client.Execute(request);
            byte[]? templateData = null;
            if (response.StatusCode == HttpStatusCode.OK && response.Content != null)
            {
                templateData = UTF8Encoding.UTF8.GetBytes(response.Content);
            }
            return templateData;
        }

        string? ListTemplates()
        {
            RestRequest request = GetBaseRequest("templates", Method.Get);
            RestResponse response = client.Execute(request);

            string? list = null;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                list = response.Content;
            }
            return list;
        }

        private static string GetHeader(RestResponse response, string key)
        {
            string toReturn = "";
            foreach (HeaderParameter p in response.Headers)
            {
                if (p.Name == key)
                {
                    toReturn = (string)p.Value;
                }
            }
            return toReturn;
        }

        private static string GetContentHeader(RestResponse response, string key)
        {
            string toReturn = "";
            foreach (HeaderParameter p in response.ContentHeaders)
            {
                if (p.Name == key)
                {
                    toReturn = (string)p.Value;
                }
            }
            return toReturn;
        }

        private static int GetLastPartFromLocationHeaderAsInt(RestResponse response)
        {
            string loc = GetHeader(response, "Location");
            string[] a = loc.Split('/');
            return Convert.ToInt32(a[a.Length - 1]); ;
        }

        private static string GetLastPartFromLocationHeader(RestResponse response)
        {
            string loc = GetHeader(response, "Location");
            string[] a = loc.Split('/');
            return a[a.Length - 1];
        }

        private RestRequest GetBaseRequest(string resource, Method method)
        {
            RestRequest request = new RestRequest(resource, method);
            request.AddHeader("X-API-KEY", apiKey);
            return request;
        }
    }
}
