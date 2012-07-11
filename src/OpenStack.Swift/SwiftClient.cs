namespace OpenStack.Swift
{
	using System;
	using System.Net;
	using System.Net.Security;
	using System.IO;
	using System.Collections.Generic;
	using System.Security.Cryptography.X509Certificates;
	using System.Xml;
	using System.Web;

    /// <summary>
    /// This delegate should be used to signal when <see><cref>Openstack.Swift.Client.PutObject</cref></see> completes uploading an object.
    /// </summary>
    public delegate void OperationCompleteCallback();

    /// <summary>
    /// This delegate should be used to signal bytes written by <see><cref>Openstack.Client.PutObject</cref></see> .
    /// </summary>
    public delegate void ProgressCallback(int bytes_written);

	/// <summary>
	/// Client.
	/// </summary>
	public class SwiftClient : ISwiftClient
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

		public SwiftClient(int timeout, int chunkSize) 
		{
			Timeout = timeout;
			ChunkSize = chunkSize;
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
		public void DisableSSLCertificateValidation()
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
	    public AuthResponse GetAuth(string url, string user, string key, Dictionary<string, string> headers, Dictionary<string, string> query, bool snet)
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
		public AccountResponse GetAccount(string url, string token, Dictionary<string, string> headers, Dictionary<string, string> query, bool full_listing)
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
							if(!info.ContainsKey(key))
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
		public AccountResponse HeadAccount(string url, string token, Dictionary<string, string> headers, Dictionary<string, string> query)
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
		public AccountResponse PostAccount(string url, string token, Dictionary<string, string> headers, Dictionary<string, string> query)
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
		public ContainerResponse GetContainer(string url, string token, string container, Dictionary<string, string> headers, Dictionary<string, string> query, bool full_listing)
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
							if(!info.ContainsKey(key))
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
		public ContainerResponse HeadContainer(string url, string token, string container, Dictionary<string, string> headers, Dictionary<string, string> query)
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
		public ContainerResponse PutContainer(string url, string token, string container, Dictionary<string, string> headers, Dictionary<string, string> query)
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
		public ContainerResponse PostContainer(string url, string token, string container, Dictionary<string, string> headers, Dictionary<string, string> query)
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
		public ContainerResponse DeleteContainer(string url, string token, string container, Dictionary<string, string> headers, Dictionary<string, string> query)
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
		public ObjectResponse GetObject(string url, string token, string container, string name, Dictionary<string, string> headers, Dictionary<string, string> query)
		{
            headers["X-Auth-Token"] = token;
			IHttpRequest request = _http_factory.GetHttpRequest("GET", url + '/' + _encode(container) + '/' + _encode(name), headers, query);
			IHttpResponse response = request.GetResponse();
			response.Close();
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
		public ObjectResponse HeadObject(string url, string token, string container, string name, Dictionary<string, string> headers, Dictionary<string, string> query)
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
		public ObjectResponse PostObject(string url, string token, string container, string name, Dictionary<string, string> headers, Dictionary<string, string> query)
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
	    public ObjectResponse PutObject(string url, string token, string container, string name, Stream contents, Dictionary<string, string> headers, Dictionary<string, string> query)
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
		public ObjectResponse DeleteObject(string url, string token, string container, string name, Dictionary<string, string> headers, Dictionary<string, string> query)
		{
			headers["X-Auth-Token"] = token;
			IHttpRequest request = _http_factory.GetHttpRequest("DELETE", url + '/' + _encode(container) + '/' + _encode(name), headers, query);
			IHttpResponse response = request.GetResponse();
			response.Close();
			return new ObjectResponse(response.Headers, response.Reason, response.Status, null);
		}
	}

}