using System;

namespace OpenStack.Swift
{
	public abstract class AbstractClient : ISwiftClient
	{
		#region ISwiftClient implementation
		public abstract AuthResponse GetAuth (string url, string user, string key, System.Collections.Generic.Dictionary<string, string> headers, System.Collections.Generic.Dictionary<string, string> query, bool snet); 

		public abstract  AccountResponse GetAccount (string url, string token, System.Collections.Generic.Dictionary<string, string> headers, System.Collections.Generic.Dictionary<string, string> query, bool full_listing);
	
		public abstract AccountResponse HeadAccount (string url, string token, System.Collections.Generic.Dictionary<string, string> headers, System.Collections.Generic.Dictionary<string, string> query);
	
		public abstract AccountResponse PostAccount (string url, string token, System.Collections.Generic.Dictionary<string, string> headers, System.Collections.Generic.Dictionary<string, string> query);
	
		public abstract ContainerResponse GetContainer (string url, string token, string container, System.Collections.Generic.Dictionary<string, string> headers, System.Collections.Generic.Dictionary<string, string> query, bool full_listing);
	
		public abstract ContainerResponse HeadContainer (string url, string token, string container, System.Collections.Generic.Dictionary<string, string> headers, System.Collections.Generic.Dictionary<string, string> query);
	
		public abstract ContainerResponse PostContainer (string url, string token, string container, System.Collections.Generic.Dictionary<string, string> headers, System.Collections.Generic.Dictionary<string, string> query);
	
		public abstract ContainerResponse PutContainer (string url, string token, string container, System.Collections.Generic.Dictionary<string, string> headers, System.Collections.Generic.Dictionary<string, string> query);
		
		public abstract ContainerResponse DeleteContainer (string url, string token, string container, System.Collections.Generic.Dictionary<string, string> headers, System.Collections.Generic.Dictionary<string, string> query);

		public abstract ObjectResponse GetObject (string url, string token, string container, string name, System.Collections.Generic.Dictionary<string, string> headers, System.Collections.Generic.Dictionary<string, string> query);
		

		public abstract ObjectResponse HeadObject (string url, string token, string container, string name, System.Collections.Generic.Dictionary<string, string> headers, System.Collections.Generic.Dictionary<string, string> query);
		

		public abstract ObjectResponse PostObject (string url, string token, string container, string name, System.Collections.Generic.Dictionary<string, string> headers, System.Collections.Generic.Dictionary<string, string> query);


		public abstract ObjectResponse PutObject (string url, string token, string container, string name, System.IO.Stream contents, System.Collections.Generic.Dictionary<string, string> headers, System.Collections.Generic.Dictionary<string, string> query);


		public abstract ObjectResponse DeleteObject (string url, string token, string container, string name, System.Collections.Generic.Dictionary<string, string> headers, System.Collections.Generic.Dictionary<string, string> query);

		#endregion

	}
}

