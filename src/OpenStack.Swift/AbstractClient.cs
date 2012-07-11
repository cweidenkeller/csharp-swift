namespace OpenStack.Swift
{
	using System;
	using System.IO;
	using System.Collections.Generic;

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
}

