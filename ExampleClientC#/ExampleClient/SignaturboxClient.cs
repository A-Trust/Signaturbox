using RestSharp;
using System.Net;
using System.Text;
using System.Text.Json;

namespace ExampleClient
{
    public class SignaturboxClient
    {
        /// <summary>
        /// A REST Client used to ease interaction with a  signature server
        /// </summary>

        private RestClient client;
        private string apiKey;

        /// <summary>
        /// Initializes the SignaturboxClient
        /// </summary>
        /// <param name="baseUrl">The base URL of the signature server</param>
        /// <param name="apiKey">Your API-Key used for Authentication on the signature server</param>
        public SignaturboxClient(string baseUrl, string apiKey)
        {
            client = new RestClient(baseUrl);
            this.apiKey = apiKey;
        }


        /// <summary>
        /// Creates a new batch. Enables you to upload documents that should be signed
        /// </summary>
        /// <param name="redirectUrl">The URL to redirect to, when the signature has completed successfully</param>
        /// <param name="errorUrl">The URL to redirect to, when an error occures</param>
        /// <returns>TicketID for this batch</returns>
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

        /// <summary>
        /// Starts the signature process for a batch
        /// </summary>
        /// <param name="ticket">The TicketID of the batch that should be signed</param>
        /// <param name="handySigParameter">Parameters for Handy-Signatur.AT</param>
        /// <returns>The URL location of where to sign the specified batch</returns>
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

        /// <summary>
        /// Adds a document to the batch
        /// </summary>
        /// <param name="ticket">TicketID of the batch to which the document is added to</param>
        /// <param name="file">The path to the template file that will be uploaded</param>
        /// <param name="location">Location of the signature (optional)</param>
        /// <param name="reason">Reason of the signature (optional)</param>
        /// <returns>DocumentID of the uploaded document</returns>
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

        /// <summary>
        /// Adds a new document to a document batch with a specific template
        /// </summary>
        /// <param name="ticket">TichetID of the batch, the document will be added to</param>
        /// <param name="file">The path to the template file that will be uploaded</param>
        /// <param name="templateId">TemplateID of the template being used</param>
        /// <param name="location">Location of the signature (optional)</param>
        /// <param name="reason">The reason for the upload (optional)</param>
        /// <returns>DocumentID of the uploaded document</returns>
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

        /// <summary>
        /// Adds a new document to a document batch with a specific template 
        /// </summary>
        /// <param name="ticket">TichetID of the batch, the document will be added to</param>
        /// <param name="file">The path to the template file that will be uploaded</param>
        /// <param name="templateId">TemplateID of the used template</param>
        /// <param name="location">Location of the signature</param>
        /// <param name="reason">The reason of the signature</param>
        /// <param name="page">The page of the signature seal</param>
        /// <param name="x">X0 in userspace units</param>
        /// <param name="y">Y0 in userspace units</param>
        /// <param name="w">X1 in userspace units</param>
        /// <param name="h">Y1 in userspace units</param>
        /// <returns>DocumentID of the uploaded document</returns>
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

        /// <summary>
        /// Downloads the specified document from the signature server 
        /// </summary>
        /// <param name="ticket">TichetID of the batch, the document is added to</param>
        /// <param name="documentId">DocumentID of the downloaded document</param>
        /// <param name="documentName">The name of the returned document</param>
        /// <param name="documentData">The data of the returned document</param>
        /// <returns>DocumentID of the uploaded document</returns>
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

        /// <summary>
        /// Uploads the given template file to the signature 
        /// </summary>
        /// <param name="file">The path of the template file that will be uploaded</param>
        /// <returns>TemplateID of the uploaded template</returns>
        public int UploadTemplate(string file)
        {
            GetBaseRequest("abc", Method.Head);
            RestRequest request = GetBaseRequest("templates", Method.Post);
            if (!File.Exists(file))
            {
                return -1;
            }
            request.AddFile(Path.GetFileName(file), file);
            RestResponse response = client.Execute(request);
            int templateId = -1;
            if (response.StatusCode == HttpStatusCode.Created)
            {
                templateId = GetLastPartFromLocationHeaderAsInt(response);
            }
            return templateId;
        }

        /// <summary>
        /// Replaces the specified template with the given template file
        /// </summary>
        /// <param name="file">The path to the template file that will replace the specified templateID</param>
        /// <param name="templateId">TemplateID of the template that should be replaced</param>
        /// <returns>Did the replacement succeed or not (bool)</returns>
        public bool ReplaceTemplate(string file, int templateId)
        {
            RestRequest request = GetBaseRequest("templates/{id}", Method.Put);
            request.AddUrlSegment("id", templateId.ToString());
            if (!File.Exists(file))
            {
                throw new FileNotFoundException();
            }
            request.AddFile(Path.GetFileName(file), file);
            RestResponse response = client.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Deletes a template from the signature server
        /// </summary>
        /// <param name="templateId">The TemplateID that is to be deleted</param>
        /// <returns>If the template was deleted or not (bool)</returns>
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

        /// <summary>
        /// Gets a template from the signature server
        /// </summary>
        /// <param name="templateId">The TemplateID of the template you want to recieve</param>
        /// <returns>The template data</returns>
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

        /// <summary>
        /// Gets a list of all available templates on the signature server
        /// </summary>
        /// <returns>a list of available templates</returns>
        public List<Template> ListTemplates()
        {            
            RestRequest request = GetBaseRequest("templates", Method.Get);
            RestResponse response = client.Execute(request);

            // TODO: get to work with core .net, sonst NewtonSoft.Json
            string? list = null;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                list = response.Content;
            }
            if (list == null)
            {
                return new List<Template> { };
            }
            JsonDocument jsonDocument = JsonDocument.Parse(list);
            JsonElement jsonArray = jsonDocument.RootElement.GetProperty("templateList");
            List<Template> templateList = JsonSerializer.Deserialize<List<Template>>(jsonArray.GetRawText());
            if (templateList == null)
            {
                return new List<Template> { };
            }
            return templateList;
        }

        /// <summary>
        /// Retrieves the specified header from a REST-Response
        /// </summary>
        /// <param name="response">The RestResponse from which the header is extracted from</param>
        /// <param name="key">The key of the header that should be retrieved</param>
        /// <returns>The specified header value</returns>
        private static string GetHeader(RestResponse response, string key)
        {
            string toReturn = "";
            if (response == null)
            {
                return toReturn;
            }
            foreach (HeaderParameter p in response.Headers)
            {
                if (p.Name == key)
                {
                    toReturn = (string)p.Value;
                }
            }
            return toReturn;
        }

        /// <summary>
        /// Gets the content header of a REST-Response
        /// </summary>
        /// <param name="response">The RestResponse from which the content header is extracted from</param>
        /// <param name="key">The key of the content header that should be retrieved</param>
        /// <returns>The value of the specified content header</returns>
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

        /// <summary>
        /// returns the last part of a url string. Split by "\".
        /// </summary>
        /// <param name="text">The text the last part is extracted from</param>
        /// <param name="splitter">The string that splits the text</param>
        /// <returns>Last part of url string</returns>
        public  static string GetLastPartFromUrl(string text, string splitter)
        {
            string[] textSplit = text.Split(splitter);
            return textSplit[textSplit.Length - 1];
        }

        /// <summary>
        /// Gets the last part from a location header
        /// </summary>
        /// <param name="response">The HTTP response the information is extracted out of</param>
        /// <returns>Integer value of the last part of the location header</returns>
        private static int GetLastPartFromLocationHeaderAsInt(RestResponse response)
        {
            string locationHeader = GetLastPartFromLocationHeader(response);
            return Convert.ToInt32(locationHeader);
        }

        /// <summary>
        /// Gets the last part from a location header
        /// </summary>
        /// <param name="response">The HTTP response the information is extracted out of</param>
        /// <returns>String value of the last part of the location header</returns>
        private static string GetLastPartFromLocationHeader(RestResponse response)
        {
            string loc = GetHeader(response, "Location");
            return GetLastPartFromUrl(loc, "/");
        }

        /// <summary>
        /// Creates a base HTTP-Request and adds your API-Key to the requestheader
        /// </summary>
        /// <param name="resource">The server URL the request is sent to</param>
        /// <param name="method">The HTTP Method (Get, Post, Put, Delete, ...) that will be used for the request</param>
        /// <returns></returns>
        private RestRequest GetBaseRequest(string resource, Method method)
        {
            RestRequest request = new RestRequest(resource, method);
            request.AddHeader("X-API-KEY", apiKey);
            return request;
        }

        /// <summary>
        /// An example method for writing a byte array to a file under the specified destinationDirectory 
        /// </summary>
        /// <param name="filename">The filename of the new file</param>
        /// <param name="destinationDir">The destination directory of the new file</param>
        /// <param name="content"></param>
        /// <returns></returns>
        public bool WriteToFile(string filename, string destinationDir, byte[] content)
        {
            Console.WriteLine("saved " + destinationDir);
            if (!Directory.Exists(destinationDir))
            {
                Console.WriteLine("destination directory does not exist");
                return false;
            }
            System.IO.File.WriteAllBytes(destinationDir + @"\" + filename, content);
            return true;
        }

    }
}

/// <summary>
/// The Template class saves metadata for a template recieved by the ListTemplates Method
/// </summary>
public class Template
{
    public int id { get; set; }
    public string description { get; set; }
}
