using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Net;
using System.Text;
using System.IO;
using System.Collections.Specialized;
using System.Globalization;
using Mishavad_API.Models;
using System.Threading.Tasks;

namespace Mishavad_API.Helpers
{
    public class UploadHelper
    {
        public class UploaderResponse {
            public HttpStatusCode StatusCode;
            public string FilePath;
            public int FileServerId;
            public string Message;
            public int? BF_Idx;
            public UploaderResponse(HttpStatusCode statusCode, string messsage, string filePath, int serverId, int? bf_idx=null) {
                FilePath = filePath;
                Message = messsage;
                FileServerId = serverId;
                StatusCode = statusCode;
                BF_Idx = bf_idx;
            }
        }

        public class UploadFile
        {
            public UploadFile()
            {
                ContentType = "application/octet-stream"; //i.e. "arbitrary binary data" (in RFC 2046)
            }
            public string Name { get; set; }
            public string Filename { get; set; }
            public string ContentType { get; set; }
            public byte[] DataBytes { get; set; }
        }

       /*
        public static WebResponse UploadFiles(string address, IEnumerable<UploadFile> files, NameValueCollection values)
        {
            var debug_log = "";
            var request = WebRequest.Create(address);
            request.Method = "POST";
            var boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x", NumberFormatInfo.InvariantInfo);
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            boundary = "--" + boundary;

            using (var requestStream = request.GetRequestStream())
            {
                System.Diagnostics.Debug.WriteLine("Acc Num and Token Passed to Uploader.php:");
                // Write the values
                foreach (string name in values.Keys)
                {
                    var buffer = Encoding.ASCII.GetBytes(boundary + Environment.NewLine);
                    requestStream.Write(buffer, 0, buffer.Length);
                    buffer = Encoding.ASCII.GetBytes(string.Format("Content-Disposition: form-data; name=\"{0}\"{1}{1}", name, Environment.NewLine));
                    requestStream.Write(buffer, 0, buffer.Length);
                    buffer = Encoding.UTF8.GetBytes(values[name] + Environment.NewLine);
                    System.Diagnostics.Debug.WriteLine(values[name]);
                    requestStream.Write(buffer, 0, buffer.Length);
                }

                // Write the files
                foreach (var file in files)
                {
                    var buffer = Encoding.ASCII.GetBytes(boundary + Environment.NewLine);
                    requestStream.Write(buffer, 0, buffer.Length);
                    var text = string.Format("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"{2}", file.Name, file.Filename, Environment.NewLine);
                    debug_log += text;
                    buffer = Encoding.UTF8.GetBytes(text);
                    requestStream.Write(buffer, 0, buffer.Length);
                    buffer = Encoding.ASCII.GetBytes(string.Format("Content-Type: {0}{1}{1}", file.ContentType, Environment.NewLine));
                    requestStream.Write(buffer, 0, buffer.Length);
                    requestStream.Write(file.DataBytes, 0 ,file.DataBytes.Length);
                    buffer = Encoding.ASCII.GetBytes(Environment.NewLine);
                    requestStream.Write(buffer, 0, buffer.Length);
                }

                var boundaryBuffer = Encoding.ASCII.GetBytes(boundary + "--");
                requestStream.Write(boundaryBuffer, 0, boundaryBuffer.Length);
            }
            try
            {
                return request.GetResponse();
                
               
                
            }
            catch (WebException e)
            {
               // throw new Exception(debug_log);
                return e.Response;
            }
            
        }
*/
        public static Dictionary<FileServerTokenType, int> SizeLimits =
            new Dictionary<FileServerTokenType, int>();

        public static async Task<UploaderResponse> UploadBase64ImageAsync(
            ApplicationDbContext ctx, string userId, string base64Image, FileServerTokenType tokenType, bool encrypt=false)
        {

            byte[] imgData = Convert.FromBase64String(
                    base64Image.Substring(base64Image.IndexOf("base64,") + 7));

            if (imgData.Length > SizeLimits[tokenType])
            {
                return new UploaderResponse(HttpStatusCode.BadRequest, "Large Image:" + imgData.Length.ToString(), "", 0);
            }
            

            var token = FileServerTokenManager.GenerateRandomToken();
            var hash = FileServerTokenManager.GenerateHash(token);

            var user = ctx.Users.Find(int.Parse(userId));
            if (user == null)
                throw new Exception("Invalid userId");

            var fst = new FileServerToken
            {
                TokenHash = hash,
                TokenExpDateUtc = DateTime.UtcNow.Add(FileServerTokenManager.TokenTimeSpan),
                AccountNumber = userId,
                FileTokenType = tokenType
            };

            ctx.FileServerTokens.Add(fst);
            await ctx.SaveChangesAsync();

            var values = new System.Collections.Specialized.NameValueCollection();
            values.Add("accountNumber", fst.AccountNumber);
            values.Add("token", System.Web.HttpUtility.UrlEncode(token));
            int? bf_idx = null;
            if (encrypt)
            {
                bf_idx = EncryptionService.NewBF_Idx();
                imgData = EncryptionService.EncryptBytes(imgData, (int)bf_idx);
            }
            var files = new[]
           {
                    new UploadFile
                    {
                        Name = "file",
                        Filename = "myfile.jpg",
                        DataBytes = imgData
                    }
                };

            var fileServerId = 1; //TODO FUTURE: Replace by file server selection logic
            string filePath="";
            //TODO: What if upload fails?
            string responseText = "";
            string debug_log = "";
            HttpStatusCode status;
            using (var uploaderResponse = await UploadFilesAsync(
                "http://" + ctx.FileServers.First().ServerIP + "/uploader.php", files, values))
            {

                HttpWebResponse httpResponse = (HttpWebResponse)(uploaderResponse);
                using (var data = uploaderResponse.GetResponseStream())
                {
                    using (var reader = new StreamReader(data))
                    {
                         responseText =  await reader.ReadToEndAsync();
                    }
                }
                status = httpResponse.StatusCode;
                if (status == HttpStatusCode.OK || status == HttpStatusCode.Created)
                {
                    filePath = responseText;
                     debug_log += "Image Path:" + responseText + "\r\n";
                }
                else {
                      debug_log += "Image Upload Error Code:" + httpResponse.StatusCode + "|";
                      debug_log += responseText + "\r\n";
                }
            }
                return new UploaderResponse(status,debug_log, filePath, fileServerId,bf_idx );
        }

        public static async Task<WebResponse> UploadFilesAsync(string address, IEnumerable<UploadFile> files, NameValueCollection values)
        {
            var debug_log = "";
            var request = WebRequest.Create(address);
            request.Method = "POST";
            var boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x", NumberFormatInfo.InvariantInfo);
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            boundary = "--" + boundary;

            using (var requestStream = await request.GetRequestStreamAsync())
            {
                System.Diagnostics.Debug.WriteLine("Acc Num and Token Passed to Uploader.php:");
                // Write the values
                foreach (string name in values.Keys)
                {
                    var buffer = Encoding.ASCII.GetBytes(boundary + Environment.NewLine);
                    await requestStream.WriteAsync(buffer, 0, buffer.Length);
                    buffer = Encoding.ASCII.GetBytes(string.Format("Content-Disposition: form-data; name=\"{0}\"{1}{1}", name, Environment.NewLine));
                    await requestStream.WriteAsync(buffer, 0, buffer.Length);
                    buffer = Encoding.UTF8.GetBytes(values[name] + Environment.NewLine);
                    await requestStream.WriteAsync(buffer, 0, buffer.Length);
                }

                // Write the files
                foreach (var file in files)
                {
                    var buffer = Encoding.ASCII.GetBytes(boundary + Environment.NewLine);
                    await requestStream.WriteAsync(buffer, 0, buffer.Length);
                    var content_disposition = string.Format("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"{2}", file.Name, file.Filename, Environment.NewLine);
                    debug_log += content_disposition;
                    buffer = Encoding.UTF8.GetBytes(content_disposition);
                    await requestStream.WriteAsync(buffer, 0, buffer.Length);
                    buffer = Encoding.ASCII.GetBytes(string.Format("Content-Type: {0}{1}{1}", file.ContentType, Environment.NewLine));
                    await requestStream.WriteAsync(buffer, 0, buffer.Length);
                    await requestStream.WriteAsync(file.DataBytes, 0, file.DataBytes.Length);
                    buffer = Encoding.ASCII.GetBytes(Environment.NewLine);
                    await requestStream.WriteAsync(buffer, 0, buffer.Length);
                }

                var boundaryBuffer = Encoding.ASCII.GetBytes(boundary + "--");
                await requestStream.WriteAsync(boundaryBuffer, 0, boundaryBuffer.Length);
            }
            try
            {
                return await request.GetResponseAsync();

            }
            catch (WebException e)
            {
                // throw new Exception(debug_log);
                return e.Response;
            }

        }
    }
}