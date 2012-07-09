using System;
using System.Net;
using System.Net.Security;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Web;

namespace OpenStack.Swift
{
    /// <summary>
    /// This delegate should be used to signal when <see><cref>Openstack.Swift.Client.PutObject</cref></see> completes uploading an object.
    /// </summary>
    public delegate void OperationCompleteCallback();

    /// <summary>
    /// This delegate should be used to signal bytes written by <see><cref>Openstack.Swift.Client.PutObject</cref></see> .
    /// </summary>
    public delegate void ProgressCallback(int bytes_written);

    /// <summary>
    /// ClientException is thrown by <see><cref>Openstack.Swift.Client</cref></see>
    /// </summary>
    public class ClientException : ApplicationException
	{
        /// <summary>
        /// Returns the HTTP Status Code from the response server.
        /// </summary>
		public readonly int Status;

        /// <summary>
        /// Initializes a new instance of the <see><cref>Openstack.Swift.ClientException</cref></see> class.
        /// </summary>
        /// <param name='message'>
        /// A <see cref="System.String"/> The exception message.
        /// </param>
        /// <param name='status'>
        /// A <see cref="System.Int32"/> that is the status code from the responding server.
        /// </param>
        public ClientException(string message, int status) : base(message)
		{
			Status = status;
		}
	}
	/// <summary>
	/// Client.
	/// </summary>
	public class SwiftClient : Client
	{
		/// <summary>
		/// Used to configure the http response timeout.
		/// </summary>
		public int Timeout = 50000;

	    /// <summary>
	    /// Set for chunk sizes on <see><cref>Openstack.Swift.Client.PutObject</cref></see> and <see><cref>Openstack.Swift.Client.GetObject</cref></see> .
	    /// </summary>
	    public int ChunkSize = 65536;
		private readonly IHttpRequestFactory _http_factory;
		/// <summary>
		/// Set me if you would like to be notified when PutObject completes.
		/// </summary>
		public OperationCompleteCallback OperationComplete;
		/// <summary>
		/// Set me if you would like to have the bytes written passed back to you.
		/// </summary>
		public ProgressCallback Progress;

	    /// <summary>
	    /// Initializes a new instance of the <see><cref>Openstack.Swift.Client</cref></see> class.
	    /// </summary>
	    public SwiftClient()
		{
			_http_factory = new HttpRequestFactory();
		}

	    /// <summary>
	    /// Initializes a new instance of the <see><cref>Openstack.Swift.Client</cref></see> class.
	    /// </summary>
	    /// <param name='http_factory'>
	    /// Implement the <see><cref>Openstack.Swift.IHttpRequestFactory</cref></see> interface for a custom HttpRequestFactory.
	    /// </param>
	    public SwiftClient(IHttpRequestFactory http_factory)
		{
			_http_factory = http_factory;
		}
		private string _encode(string string_to_encode)
		{
			return HttpUtility.UrlPathEncode(string_to_encode);
		}
		private bool _validator (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
	    {
	 	    return true;
	    }
		/// <summary>
		/// Disables SSL certificate validation.
		/// </summary>
		public override void DisableSSLCertificateValidation()
		{
            ServicePointManager.ServerCertificateValidationCallback = _validator;	
		}

	    /// <summary>
	    /// Gets Your Auth Info.
	    /// </summary>
	    /// <returns>
	    /// <paramref><name>Openstack.Swift.AuthResponse</name></paramref> </returns>
	    /// <param name='url'>
	    /// <see cref="string"/> of the Storage Url you would like to make a request to.
	    /// </param>
	    /// <param name='user'>
	    /// <see cref="string"/> Your username
	    /// </param>
	    /// <param name='key'>
	    /// <see cref="string"/> Your ApiKey
	    /// </param>
	    /// <param name='headers'>
	    /// <see><cref>T:System.Collections.Generic.Dictionary{string,string}</cref></see> Custom headers for your request.
	    /// </param>
	    /// <param name='query'>
	    /// <see><cref>T:System.Collections.Generic.Dictionary{string,string}</cref></see> Custom query strings used for your request.
	    /// </param>
	    /// <param name='snet'>
	    /// <see cref="System.Boolean"/> 
	    /// </param>
	    public override AuthResponse GetAuth(string url, string user, string key, Dictionary<string, string> headers, Dictionary<string, string> query, bool snet)
		{
			headers["X-Auth-User"] = user;
			headers["X-Auth-Key"] = key;
			IHttpRequest request = _http_factory.GetHttpRequest("GET", url, headers, query);
			IHttpResponse response = request.GetResponse();
			headers = response.Headers;
			if (snet)
			{
				var uri = new Uri(headers["x-storage-url"]);
				headers["x-storage-url"] = uri.Scheme + "://snet-" + uri.Host + uri.PathAndQuery;
			}
			response.Close();
			return new AuthResponse(headers, response.Reason, response.Status);
		}
		/// <summary>
		/// Gets the account.
		/// </summary>
		/// <returns>
		/// The account.
		/// </returns>
		/// <param name='url'>
		/// URL.
		/// </param>
		/// <param name='token'>
		/// Token.
		/// </param>
		/// <param name='headers'>
		/// Headers.
		/// </param>
		/// <param name='query'>
		/// Query.
		/// </param>
		/// <param name='full_listing'>
		/// Full_listing.
		/// </param>
		public override AccountResponse GetAccount(string url, string token, Dictionary<string, string> headers, Dictionary<string, string> query, bool full_listing)
		{
			headers["X-Auth-Token"] = token;
			query["format"] = "xml";
			var request = _http_factory.GetHttpRequest("GET", url, headers, query);
			var response = request.GetResponse();
			var reader = new XmlTextReader(response.ResponseStream);
		    var containers = new List<Dictionary<string, string>>();
			var info = new Dictionary<string, string>();
		    while (reader.Read())
		    {   
				switch (reader.NodeType)
				{
				    case XmlNodeType.Element:
				        if (reader.Name != "xml" && 
					        reader.Name != "container" && 
					        reader.Name != "account")
					    {
				            var key = reader.Name;
						    reader.Read();
						    info.Add(key, reader.Value);
					    }
					    break;
				    case XmlNodeType.EndElement:
					    if (reader.Name == "container")
					    {
						    containers.Add(info);
						    info = new Dictionary<string, string>();
					    }
					     break;
				}
			}
			if (full_listing)
			{
			    do
				{
					var nmarker = containers.Count - 1;
					query["marker"] = containers[nmarker]["name"];
					var tmp = GetAccount(url, token, headers, query, false);
				    if ((tmp.Containers).Count > 0)
					{
						containers.AddRange(tmp.Containers);
					}
					else
					{
						break;
					}
				} while (true);
				
			}
			response.Close();
			return new AccountResponse(response.Headers, response.Reason, response.Status, containers);
		}
		/// <summary>
		/// Heads the account.
		/// </summary>
		/// <returns>
		/// The account.
		/// </returns>
		/// <param name='url'>
		/// URL.
		/// </param>
		/// <param name='token'>
		/// Token.
		/// </param>
		/// <param name='headers'>
		/// Headers.
		/// </param>
		/// <param name='query'>
		/// Query.
		/// </param>
		public override AccountResponse HeadAccount(string url, string token, Dictionary<string, string> headers, Dictionary<string, string> query)
		{
			headers["X-Auth-Token"] = token;
			IHttpRequest request = _http_factory.GetHttpRequest("HEAD", url, headers, query);
			IHttpResponse response = request.GetResponse();
			response.Close();
			return new AccountResponse(response.Headers, response.Reason, response.Status, null);
		}
		/// <summary>
		/// Posts the account.
		/// </summary>
		/// <returns>
		/// The account.
		/// </returns>
		/// <param name='url'>
		/// URL.
		/// </param>
		/// <param name='token'>
		/// Token.
		/// </param>
		/// <param name='headers'>
		/// Headers.
		/// </param>
		/// <param name='query'>
		/// Query.
		/// </param>
		public override AccountResponse PostAccount(string url, string token, Dictionary<string, string> headers, Dictionary<string, string> query)
		{
			headers["X-Auth-Token"] = token;
			IHttpRequest request = _http_factory.GetHttpRequest("POST", url, headers, query);
		    IHttpResponse response = request.GetResponse();
			response.Close();
			return new AccountResponse(response.Headers, response.Reason, response.Status, null);
		}
		/// <summary>
		/// Gets the container.
		/// </summary>
		/// <returns>
		/// The container.
		/// </returns>
		/// <param name='url'>
		/// URL.
		/// </param>
		/// <param name='token'>
		/// Token.
		/// </param>
		/// <param name='container'>
		/// Container.
		/// </param>
		/// <param name='headers'>
		/// Headers.
		/// </param>
		/// <param name='query'>
		/// Query.
		/// </param>
		/// <param name='full_listing'>
		/// Full_listing.
		/// </param>
		public override ContainerResponse GetContainer(string url, string token, string container, Dictionary<string, string> headers, Dictionary<string, string> query, bool full_listing)
		{
			headers["X-Auth-Token"] = token;
			query["format"] = "xml";
			var request = _http_factory.GetHttpRequest("GET", url + "/" + _encode(container), headers, query);
			var response = request.GetResponse();
			var reader = new XmlTextReader(response.ResponseStream);
			var objects = new List<Dictionary<string, string>>();
			var info = new Dictionary<string, string>();
		    while (reader.Read())
		    {
		        switch (reader.NodeType)
				{
				    case XmlNodeType.Element:
				        if (reader.Name != "xml" && 
					        reader.Name != "container" && 
					        reader.Name != "object")
					    { 
				            var key = reader.Name;
						    reader.Read();
						    info.Add(key, reader.Value);
					    }
					    break;
				    case XmlNodeType.EndElement:
					    if (reader.Name == "object")
					    {
						    objects.Add(info);
						    info = new Dictionary<string, string>();
					    }
					     break;
				}
		    }
		    if (full_listing)
			{
			    do
				{
				    var nmarker = objects.Count - 1; 
				    query["marker"] = objects[nmarker].ContainsKey("name") ? objects[nmarker]["name"] : objects[nmarker]["subdir"];
					var tmp = GetContainer(url, token, container, headers, query, false);
				    if (tmp.Objects.Count > 0)
					{
			            objects.AddRange(tmp.Objects);
					}
					else
					{
						break;
					}
				}while (true);
			}
			response.Close();
			return new ContainerResponse(response.Headers, response.Reason, response.Status, objects);
		}
		/// <summary>
		/// Heads the container.
		/// </summary>
		/// <returns>
		/// A <see name="Openstack.Swift.ContainerResponse"/> object.
		/// </returns>
		/// <param name='url'>
		/// The Storage URL
		/// </param>
		/// <param name='token'>
		/// Your Auth Token
		/// </param>
		/// <param name='container'>
		/// The containers name
		/// </param>
		/// <param name='headers'>
		/// Any custom headers needed for the request
		/// </param>
		/// <param name='query'>
		/// Any custom query strings needed for the request
		/// </param>
		public override ContainerResponse HeadContainer(string url, string token, string container, Dictionary<string, string> headers, Dictionary<string, string> query)
		{
			headers["X-Auth-Token"] = token;
			IHttpRequest request = _http_factory.GetHttpRequest("HEAD", url + "/" + _encode(container), headers, query);
			IHttpResponse response = request.GetResponse();
			response.Close();
			return new ContainerResponse(response.Headers, response.Reason, response.Status, null);
		}
		/// <summary>
		/// Does a put request against this container. 
		/// </summary>
		/// <returns>
		///  A <see name="Openstack.Swift.ContainerResponse"/> object.
		/// </returns>
		/// <param name='url'>
		/// The Storage Url
		/// </param>
		/// <param name='token'>
		/// The Auth Token
		/// </param>
		/// <param name='container'>
		/// The containers name
		/// </param>
		/// <param name='headers'>
		/// Any custom headers needed for the request
		/// </param>
		/// <param name='query'>
		/// Any custom query strings needed for the request
		/// </param>
		public override ContainerResponse PutContainer(string url, string token, string container, Dictionary<string, string> headers, Dictionary<string, string> query)
		{
			headers["X-Auth-Token"] = token;
			IHttpRequest request = _http_factory.GetHttpRequest("PUT", url + "/"  + _encode(container), headers, query);
			IHttpResponse response = request.GetResponse();
			response.Close();
			return new ContainerResponse(response.Headers, response.Reason, response.Status, null);
		}
		/// <summary>
		/// Posts to a container. You can add new metadata this way
		/// </summary>
		/// <returns>
		/// A <see name="Openstack.Swift.ContainerResponse"/> object.
		/// </returns>
		/// <param name='url'>
		/// The Storage Url
		/// </param>
		/// <param name='token'>
		/// The Auth Token.
		/// </param>
		/// <param name='container'>
		/// The container name
		/// </param>
		/// <param name='headers'>
		/// Any custom headers needed for the request
		/// </param>
		/// <param name='query'>
		/// Any custom query strings needed for the request
		/// </param>
		public override ContainerResponse PostContainer(string url, string token, string container, Dictionary<string, string> headers, Dictionary<string, string> query)
		{
			headers["X-Auth-Token"] = token;
			IHttpRequest request = _http_factory.GetHttpRequest("POST", url + '/' + _encode(container), headers, query);
			IHttpResponse response = request.GetResponse();
			response.Close();
			return new ContainerResponse(response.Headers, response.Reason, response.Status, null);
		}
		/// <summary>
		/// Deletes the container.
		/// </summary>
		/// <returns>
		/// A <see name="Openstack.Swift.ContainerResponse"/> object.
		/// </returns>
		/// <param name='url'>
		/// The Storage Url
		/// </param>
		/// <param name='token'>
		/// The Auth Token
		/// </param>
		/// <param name='container'>
		/// The container name
		/// </param>
		/// <param name='headers'>
		/// Any Custom Headers needed for the request
		/// </param>
		/// <param name='query'>
		/// Any custom query strings needed for the request
		/// </param>
		public override ContainerResponse DeleteContainer(string url, string token, string container, Dictionary<string, string> headers, Dictionary<string, string> query)
		{
			headers["X-Auth-Token"] = token;
			IHttpRequest request = _http_factory.GetHttpRequest("DELETE", url + '/' + _encode(container), headers, query);
			IHttpResponse response = request.GetResponse();
			response.Close();
			return new ContainerResponse(response.Headers, response.Reason, response.Status, null);
		}
		/// <summary>
		/// Gets the object.
		/// </summary>
		/// <returns>
		/// A <see name="Openstack.Swift.ObjectResponse"/>
		/// </returns>
		/// <param name='url'>
		/// The storage Url
		/// </param>
		/// <param name='token'>
		/// The Auth Token
		/// </param>
		/// <param name='container'>
		/// The container name
		/// </param>
		/// <param name='name'>
		/// The name of the object
		/// </param>
		/// <param name='headers'>
		/// Any custom headers needed for the request
		/// </param>
		/// <param name='query'>
		/// Any custom query strings needed for the request
		/// </param>
		public override ObjectResponse GetObject(string url, string token, string container, string name, Dictionary<string, string> headers, Dictionary<string, string> query)
		{
            headers["X-Auth-Token"] = token;
			IHttpRequest request = _http_factory.GetHttpRequest("GET", url + '/' + _encode(container) + '/' + _encode(name), headers, query);
			IHttpResponse response = request.GetResponse();
			return new ObjectResponse(response.Headers, response.Reason, response.Status, response.ResponseStream);
		}
		/// <summary>
		/// heads the object.
		/// </summary>
		/// <returns>
		/// A <see name="Openstack.Swift.ObjectResponse"/>
		/// </returns>
		/// <param name='url'>
		/// The storage Url
		/// </param>
		/// <param name='token'>
		/// The Auth Token
		/// </param>
		/// <param name='container'>
		/// The container name
		/// </param>
		/// <param name='name'>
		/// The name of the object
		/// </param>
		/// <param name='headers'>
		/// Any custom headers needed for the request
		/// </param>
		/// <param name='query'>
		/// Any custom query strings needed for the request
		/// </param>
		public override ObjectResponse HeadObject(string url, string token, string container, string name, Dictionary<string, string> headers, Dictionary<string, string> query)
		{
			headers["X-Auth-Token"] = token;
			IHttpRequest request = _http_factory.GetHttpRequest("HEAD", url + '/' + _encode(container) + '/' + _encode(name), headers, query);
			IHttpResponse response = request.GetResponse();
			response.Close();
			return new ObjectResponse(response.Headers, response.Reason, response.Status, null);
		}
		/// <summary>
		/// Posts the object.
		/// </summary>
		/// <returns>
		/// A <see name="Openstack.Swift.ObjectResponse"/>
		/// </returns>
		/// <param name='url'>
		/// The storage Url
		/// </param>
		/// <param name='token'>
		/// The Auth Token
		/// </param>
		/// <param name='container'>
		/// The container name
		/// </param>
		/// <param name='name'>
		/// The name of the object
		/// </param>
		/// <param name='headers'>
		/// Any custom headers needed for the request
		/// </param>
		/// <param name='query'>
		/// Any custom query strings needed for the request
		/// </param>
		public override ObjectResponse PostObject(string url, string token, string container, string name, Dictionary<string, string> headers, Dictionary<string, string> query)
		{
			headers["X-Auth-Token"] = token;
			IHttpRequest request = _http_factory.GetHttpRequest("POST", url + '/' + _encode(container) + '/' + _encode(name), headers, query);
			IHttpResponse response = request.GetResponse();
			response.Close();
			return new ObjectResponse(response.Headers, response.Reason, response.Status, null);
		}

	    /// <summary>
	    /// Puts the object.
	    /// </summary>
	    /// <returns>
	    /// A <see name="Openstack.Swift.ObjectResponse"/>
	    /// </returns>
	    /// <param name='url'>
	    /// The storage Url
	    /// </param>
	    /// <param name='token'>
	    /// The Auth Token
	    /// </param>
	    /// <param name='container'>
	    /// The container name
	    /// </param>
	    /// <param name='name'>
	    /// The name of the object
	    /// </param>
	    /// <param name="contents">
	    /// The contents of the object
	    /// </param>
	    /// <param name='headers'>
	    /// Any custom headers needed for the request
	    /// </param>
	    /// <param name='query'>
	    /// Any custom query strings needed for the request
	    /// </param>
	    public override ObjectResponse PutObject(string url, string token, string container, string name, Stream contents, Dictionary<string, string> headers, Dictionary<string, string> query)
		{
			var total_length = 0;
			headers["X-Auth-Token"] = token;
			var request = _http_factory.GetHttpRequest("PUT", url + '/' + _encode(container) + '/' + _encode(name), headers, query);
			request.AllowWriteStreamBuffering = false;
			//Default Content Length is -1 if one is not set.
			//Lets Chunk Dat Body yo.
			if (request.ContentLength == -1)
			{
				request.SendChunked = true;
			}
			var req_stream = request.GetRequestStream();
			var buff = new byte[ChunkSize];
			const int offset = 0;
	        int read;
			while((read = contents.Read(buff, offset, ChunkSize)) > 0)
			{
			    if (Progress != null)
			    {
					total_length += read;
					Progress(total_length);
			    }
				req_stream.Write(buff, offset, read);
			}
			if (OperationComplete != null)
			{
				OperationComplete();
			}
			req_stream.Close();
			contents.Close();
            var response = request.GetResponse();
			response.Close();
			return new ObjectResponse(response.Headers, response.Reason, response.Status, null);
		}
		/// <summary>
		/// Deletes the object.
		/// </summary>
		/// <returns>
		/// A <see name="Openstack.Swift.ObjectResponse"/>
		/// </returns>
		/// <param name='url'>
		/// The storage Url
		/// </param>
		/// <param name='token'>
		/// The Auth Token
		/// </param>
		/// <param name='container'>
		/// The container name
		/// </param>
		/// <param name='name'>
		/// The name of the object
		/// </param>
		/// <param name='headers'>
		/// Any custom headers needed for the request
		/// </param>
		/// <param name='query'>
		/// Any custom query strings needed for the request
		/// </param>
		public override ObjectResponse DeleteObject(string url, string token, string container, string name, Dictionary<string, string> headers, Dictionary<string, string> query)
		{
			headers["X-Auth-Token"] = token;
			IHttpRequest request = _http_factory.GetHttpRequest("DELETE", url + '/' + _encode(container) + '/' + _encode(name), headers, query);
			IHttpResponse response = request.GetResponse();
			response.Close();
			return new ObjectResponse(response.Headers, response.Reason, response.Status, null);
		}
	}
	/// <summary>
	/// I client.
	/// </summary>
	public abstract class Client
	{
		public abstract void DisableSSLCertificateValidation();
		public abstract AuthResponse GetAuth(string url, string user, string key, Dictionary<string, string> headers, Dictionary<string, string> query, bool snet);
		public abstract AccountResponse GetAccount(string url, string token, Dictionary<string, string> headers, Dictionary<string, string> query, bool full_listing);
		public abstract AccountResponse HeadAccount(string url, string token, Dictionary<string, string> headers, Dictionary<string, string> query);
		public abstract AccountResponse PostAccount(string url, string token, Dictionary<string, string> headers, Dictionary<string, string> query);
		public abstract ContainerResponse GetContainer(string url, string token, string container, Dictionary<string, string> headers, Dictionary<string, string> query, bool full_listing);
		public abstract ContainerResponse HeadContainer(string url, string token, string container, Dictionary<string, string> headers, Dictionary<string, string> query);
		public abstract ContainerResponse PostContainer(string url, string token, string container, Dictionary<string, string> headers, Dictionary<string, string> query);
		public abstract ContainerResponse PutContainer(string url, string token, string container, Dictionary<string, string> headers, Dictionary<string, string> query);
		public abstract ContainerResponse DeleteContainer(string url, string token, string container, Dictionary<string, string> headers, Dictionary<string, string> query);
		public abstract ObjectResponse GetObject(string url, string token, string container, string name, Dictionary<string, string> headers, Dictionary<string, string> query);
		public abstract ObjectResponse HeadObject(string url, string token, string container, string name, Dictionary<string, string> headers, Dictionary<string, string> query);
		public abstract ObjectResponse PostObject(string url, string token, string container, string name, Dictionary<string, string> headers, Dictionary<string, string> query);
		public abstract ObjectResponse PutObject(string url, string token, string container, string name, Stream contents, Dictionary<string, string> headers, Dictionary<string, string> query);
		public abstract ObjectResponse DeleteObject(string url, string token, string container, string name, Dictionary<string, string> headers, Dictionary<string, string> query);
		
	}
	public interface IHttpRequestFactory
	{
		IHttpRequest GetHttpRequest(string method, string url, Dictionary<string, string> headers, Dictionary<string, string> query);
	}
	public class HttpRequestFactory : IHttpRequestFactory
	{
		public IHttpRequest GetHttpRequest(string method, string url, Dictionary<string, string> headers, Dictionary<string, string> query)
		{
			return new HttpRequest(method, url, headers, query);
		}
	}
    public interface IHttpRequest
	{
		bool AllowWriteStreamBuffering{ set; get; }
		bool SendChunked { set; get; }
		long ContentLength { set; get; }
		Stream GetRequestStream();
		IHttpResponse GetResponse();
	}
	public class HttpRequest : IHttpRequest
	{
	    private const int MillisecondsInOneHour = 60 * 60 * 1000;

		public bool AllowWriteStreamBuffering
		{ 
			set { _request.AllowWriteStreamBuffering = value; }
			get { return _request.AllowWriteStreamBuffering; }
		}
		public bool SendChunked
		{ 
			set { _request.SendChunked = value; } 
			get { return _request.SendChunked; }
		}
		public long ContentLength
		{
			set { _request.ContentLength = value; }
			get { return _request.ContentLength; }
		}
	    public int Timeout
	    {
            set { _request.Timeout = value; }
            get { return _request.Timeout; }
	    }
		private readonly HttpWebRequest _request;
		public HttpRequest(string method, string url, Dictionary<string, string> headers, Dictionary<string, string> query)
		{
		    var uriQuery = query != null && query.Count > 0 ? _add_query(query) : "";
		    _request = (HttpWebRequest) WebRequest.Create(url + uriQuery);
            Timeout = MillisecondsInOneHour;
			_add_headers(headers);
			_request.Method = method;
		}
		public IHttpResponse GetResponse()
		{
			try
			{
	            return new HttpResponse((HttpWebResponse)_request.GetResponse());
			}
			catch (WebException e)
			{
			    if (e.Response == null)
				{
					throw new ClientException("Timeout!", -1);
				}
			    
                throw new ClientException("Error: " + _request.RequestUri + " Unable to " + _request.Method + " " + ((HttpWebResponse)e.Response).StatusCode, (int)((HttpWebResponse)e.Response).StatusCode);
			}
		}
		public Stream GetRequestStream()
		{
			return _request.GetRequestStream();
		}
		private string _add_query(Dictionary<string, string> query)
		{
			int count = 0;
			string query_string = "";
			if (query.Count > 0)
			{
				query_string = "?";
			    foreach (KeyValuePair<string, string> query_pair in query)
				{
					++ count;
					query_string += HttpUtility.UrlEncode(query_pair.Key) + "=" + HttpUtility.UrlEncode(query_pair.Value);
					if (count < query.Count)
					{
						query_string += "&";
					}
				}
			}
		    return query_string;
		}
	    private void _add_headers(Dictionary<string, string> headers)
		{
			foreach (var header in headers)
			{
				switch (header.Key.ToLower())
				{
				case "accept":
					_request.Accept = header.Value;
					break;
				case "connection":
					_request.Connection = header.Value;
					break;
				case "content-length":
					_request.ContentLength = Int64.Parse(header.Value);
					break;
				case "content-type":
					_request.ContentType = header.Value;
					break;
				case "expect":
					_request.Expect = header.Value;
				    break;
				case "if-modified-since":
				    _request.IfModifiedSince = Convert.ToDateTime(header.Value);
					break;
				case "range":
					_request.AddRange(Convert.ToInt32(header.Value));
					break;
				case "referer":
					_request.Referer = header.Value;
					break;
				case "transfer-encoding":
					_request.SendChunked = Convert.ToBoolean(header.Value);
					break;
				case "user-agent":
					_request.UserAgent = header.Value;
					break;
				default:
				    _request.Headers.Add(header.Key, header.Value);
					break;
				}
			}
		}
	}
	public interface IHttpResponse
	{
		int Status { get; }
		string Reason { get; }
		Stream ResponseStream { get; }
		Dictionary<string, string> Headers { get; }
		void Close();
	}
	public class HttpResponse : IHttpResponse
	{
		public int Status 
		{ 
		    get { return _response == null ? -1 : (int)_response.StatusCode; } 
	    }
		public string Reason 
		{ 
			get { return _response == null ? null : _response.StatusDescription; } 
		}
		public Stream  ResponseStream 
		{ 
			get { return _response == null ? null : _response.GetResponseStream(); } 
		}
		public Dictionary<string, string> Headers 
		{ 
			//Return null if _response is null. If not null and headers have been processed return them or process them and return them.
			get { return _response == null ? null : (_processed_headers ?? _process_headers()); } 
		}
		private readonly HttpWebResponse _response;
		private Dictionary<string, string> _processed_headers;
		public HttpResponse(HttpWebResponse response)
		{
			_response = response;
		}
        public void Close()
	    {
			_response.Close();
		}
		private Dictionary<string, string> _process_headers()
		{
			_processed_headers = new Dictionary<string, string>();
			foreach (string key in _response.Headers.Keys)
			{
			    _processed_headers.Add(key.ToLower(), _response.Headers.Get(key));
		    }
			return _processed_headers;
		}
	}
	/// <summary>
	/// An Object that holds auth information
	/// </summary>
	public class AuthResponse
	{
		/// <summary>
		/// response headers.
		/// </summary>
		public readonly Dictionary<string, string> Headers;
		/// <summary>
		/// response status from the server.
		/// </summary>
		public readonly int Status;
		/// <summary>
		/// The status description from the server.
		/// </summary>
		public readonly string Reason;

	    /// <summary>
	    /// Initializes a new instance of the <see><cref>Openstack.Swift.AuthResponse</cref></see> class.
	    /// </summary>
	    /// <param name='headers'>
	    /// Response headers
	    /// </param>
	    /// <param name='reason'>
	    /// Response status description.
	    /// </param>
	    /// <param name='status'>
	    /// Response status from the server.
	    /// </param>
	    public AuthResponse(Dictionary<string, string> headers, string reason, int status)
		{
			Headers = headers;
			Status = status;
			Reason = reason;
		}
	}
	/// <summary>
	/// An object containing AccountResponse information
	/// </summary>
	public class AccountResponse
	{
		/// <summary>
		/// Account Headers
		/// </summary>
	    public readonly Dictionary<string, string> Headers;
		/// <summary>
		/// the Status number of the request
		/// </summary>
		public readonly int Status;
		/// <summary>
		/// The status description of the request
		/// </summary>
		public readonly string Reason;
		/// <summary>
		/// The container list returned if a get request otherwise this will be null
		/// </summary>
		public readonly List<Dictionary<string, string>> Containers;
		/// <summary>
		/// Initializes a new instance of the <see><cref>Openstack.Swift.AccountResponse</cref></see> class.
		/// </summary>
		/// <param name='headers'>
		/// The response headers
		/// </param>
		/// <param name='reason'>
		/// The status description
		/// </param>
		/// <param name='status'>
		/// The status code of the request
		/// </param>
		/// <param name='containers'>
		/// The Container List if one is needed null otherwise
		/// </param>
	    public AccountResponse(Dictionary<string, string> headers, string reason, int status, List<Dictionary<string, string>> containers)
		{
			Headers = headers;
			Status = status;
			Reason = reason;
			Containers = containers;
		}
	}
	/// <summary>
	/// Used for container requests
	/// </summary>
	public class ContainerResponse
	{
		/// <summary>
		/// Headers from the container request
		/// </summary>
	    public readonly Dictionary<string, string> Headers;
		/// <summary>
		/// The Status code
		/// </summary>
		public readonly int Status;
		/// <summary>
		/// The status description
		/// </summary>
		public readonly string Reason;
		/// <summary>
		/// A List of objects will be null if not a get request
		/// </summary>
		public readonly List<Dictionary<string, string>> Objects;

	    /// <summary>
	    /// Initializes a new instance of the <see><cref>Openstack.Swift.ContainerResponse</cref></see> class.
	    /// </summary>
	    /// <param name='headers'>
	    /// The response headers
	    /// </param>
	    /// <param name='reason'>
	    /// The status description
	    /// </param>
	    /// <param name='status'>
	    /// The status number of the request
	    /// </param>
	    /// <param name='objects'>
	    /// A list of objects otherwise null
	    /// </param>
	    public ContainerResponse(Dictionary<string, string> headers, string reason, int status, List<Dictionary<string, string>> objects)
		{
			Headers = headers;
			Status = status;
			Reason = reason;
			Objects = objects;
		}
	}
	/// <summary>
	/// Used for object responses
	/// </summary>
	public class ObjectResponse
	{
		/// <summary>
		/// Headers from the object request
		/// </summary>
	    public readonly Dictionary<string, string> Headers;
		/// <summary>
		/// The status code of the request
		/// </summary>
		public readonly int Status;
		/// <summary>
		/// The response reason of the request
		/// </summary>
		public readonly string Reason;
		/// <summary>
		/// A Stream of the object data only used for get requests
		/// </summary>
		public readonly Stream ObjectData;

	    /// <summary>
	    /// Initializes a new instance of the <see><cref>Openstack.Swift.ObjectResponse</cref></see> class.
	    /// </summary>
	    /// <param name='headers'>
	    /// The headers from the request
	    /// </param>
	    /// <param name='reason'>
	    /// The status discription of the request
	    /// </param>
	    /// <param name='status'>
	    /// The status code from the request
	    /// </param>
	    /// <param name='object_data'>
	    /// A stream of object data will be null if not a get request
	    /// </param>
	    public ObjectResponse(Dictionary<string, string> headers, string reason, int status, Stream object_data)
		{
			Headers = headers;
			Status = status;
			Reason = reason;
			ObjectData = object_data;
		}
	}
}