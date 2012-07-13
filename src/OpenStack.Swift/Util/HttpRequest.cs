namespace OpenStack.Swift
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Net;
	using System.Web;


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
}

