namespace OpenStack.Nova
{
	using System;
	using System.Collections.Generic;

	public interface IHttpRequestFactory
	{
		IHttpRequest GetHttpRequest(string method, string url, Dictionary<string, string> headers, Dictionary<string, string> query);
	}
}

