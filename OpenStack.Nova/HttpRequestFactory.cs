namespace OpenStack.Nova
{
	using System;
	using System.Collections.Generic;

	public class HttpRequestFactory : IHttpRequestFactory
	{
		public IHttpRequest GetHttpRequest(string method, string url, Dictionary<string, string> headers, Dictionary<string, string> query)
		{
			return new HttpRequest(method, url, headers, query);
		}
	}
}

