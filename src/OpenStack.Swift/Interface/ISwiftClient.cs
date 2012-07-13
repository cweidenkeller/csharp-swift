namespace OpenStack.Swift
{
	using System;
	using System.IO;
	using System.Collections.Generic;

	/// <summary>
	/// Methods that all SwiftClient shall implement.
	/// </summary>
	public interface ISwiftClient
	{
	 	void DisableSSLCertificateValidation();
		AuthResponse GetAuth(string url, string user, string key, Dictionary<string, string> headers, Dictionary<string, string> query, bool snet);
		AccountResponse GetAccount(string url, string token, Dictionary<string, string> headers, Dictionary<string, string> query, bool full_listing);
		AccountResponse HeadAccount(string url, string token, Dictionary<string, string> headers, Dictionary<string, string> query);
		AccountResponse PostAccount(string url, string token, Dictionary<string, string> headers, Dictionary<string, string> query);
		ContainerResponse GetContainer(string url, string token, string container, Dictionary<string, string> headers, Dictionary<string, string> query, bool full_listing);
		ContainerResponse HeadContainer(string url, string token, string container, Dictionary<string, string> headers, Dictionary<string, string> query);
		ContainerResponse PostContainer(string url, string token, string container, Dictionary<string, string> headers, Dictionary<string, string> query);
		ContainerResponse PutContainer(string url, string token, string container, Dictionary<string, string> headers, Dictionary<string, string> query);
		ContainerResponse DeleteContainer(string url, string token, string container, Dictionary<string, string> headers, Dictionary<string, string> query);
		ObjectResponse GetObject(string url, string token, string container, string name, Dictionary<string, string> headers, Dictionary<string, string> query);
		ObjectResponse HeadObject(string url, string token, string container, string name, Dictionary<string, string> headers, Dictionary<string, string> query);
		ObjectResponse PostObject(string url, string token, string container, string name, Dictionary<string, string> headers, Dictionary<string, string> query);
		ObjectResponse PutObject(string url, string token, string container, string name, Stream contents, Dictionary<string, string> headers, Dictionary<string, string> query);
		ObjectResponse DeleteObject(string url, string token, string container, string name, Dictionary<string, string> headers, Dictionary<string, string> query);
		
	}
}

