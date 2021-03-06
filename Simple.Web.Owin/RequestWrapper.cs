using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Owin;
using Simple.Web.Helpers;
using Simple.Web.Http;
using Simple.Web.Owin.FileHandling;

namespace Simple.Web.Owin
{
	public class RequestWrapper : IRequest
	{
		readonly IDictionary<string, object> env;
		readonly IDictionary<string, IEnumerable<string>> headers;
		readonly BodyDelegate bodyDelegate;
		readonly MemoryStream bodyStream = new MemoryStream();
		IEnumerable<PostedFile> postedFiles;

		public RequestWrapper(IDictionary<string, object> environment)
		{
			env = environment;
			headers = (IDictionary<string, IEnumerable<string>>)environment[OwinConstants.RequestHeaders];
			bodyDelegate = (BodyDelegate)environment[OwinConstants.RequestBody];

			bodyDelegate(reader, flush => false, ex=> { }, CancellationToken.None);
		}

		bool reader(ArraySegment<byte> data)
		{
			if (data.Count > 0)
			{
				bodyStream.Write(data.Array, data.Offset, data.Count);
			}
			return true;
		}

		public Uri Url
		{
			get {
				var x = env[OwinConstants.RequestScheme] + "://"
					+ headers["Host"].Single()
					+ env[OwinConstants.RequestPath];

				return new Uri(x,  UriKind.Absolute);
			}
		}

		public IList<string> AcceptTypes
		{
			get {
				return headers["Accept"].Single().Split(',');
			}
		}

		public IDictionary<string, string> QueryString
		{
			get
			{
				return env[OwinConstants.RequestQueryString].ToString().ToQueryDictionary();
			}
		}


		public Stream InputStream
		{
			get
			{
				bodyStream.Seek(0, SeekOrigin.Begin);
				return bodyStream;
			}
		}

		public string ContentType
		{
			get {
				return GetSingleHeaderValue("Content-Type") ?? "text/html"; // POST, HEAD; Get == ""?
			}
		}

		string GetSingleHeaderValue(string headerKey)
		{
			if (!headers.ContainsKey(headerKey)) return null;
			return headers[headerKey].Single();
		}

		public string HttpMethod
		{
			get { return env[OwinConstants.RequestMethod].ToString(); }
		}

		public NameValueCollection Headers
		{
			get {
				return headers.ToNameValueCollection();
			}
		}

		public IEnumerable<IPostedFile> Files
		{
			get {
				if (postedFiles == null) {
					bodyStream.Seek(0, SeekOrigin.Begin);
					var mpp = new MultipartParser(bodyStream, GetMultipartBoundary());
					postedFiles = mpp.Parse().ToArray();
				}
				return postedFiles;
			}
		}

		string GetMultipartBoundary()
		{
			var boundsLine = headers["Content-Type"].First().Trim();

			if (!boundsLine.StartsWith("")) throw new Exception("Can't receive files until post is multipart/form-data; Try adding  enctype=\"multipart/form-data\" to your <form> tag.");
			return boundsLine.SubstringAfterLast('=');
		}

		Dictionary<string, ICookie> cookies;
		public IDictionary<string, ICookie> Cookies
		{
			get {
				if (cookies == null) {
					cookies = new Dictionary<string, ICookie>();
					if (!headers.ContainsKey("Cookie")) return cookies;
					var cookieStrings = headers["Cookie"].First().Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
					foreach (var cookie in cookieStrings) {
						var p = cookie.Split('=');
						var k = p[0];
						var v = p.Length > 1 ? p[1] : p[0];
						//cookies.Add(k, new SimpleCookie(k, v));
					}
				}
				return cookies;
			}
		}
	}
}