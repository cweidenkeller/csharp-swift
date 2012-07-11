namespace OpenStack.Swift
{
	using System;
	using System.IO;
	using System.Collections.Generic;
	using System.Net;
	using System.Web;

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
			//Return null if _response is null. If non-null and headers have been processed, just return them.
			// Else process  headers and return them.
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
}

