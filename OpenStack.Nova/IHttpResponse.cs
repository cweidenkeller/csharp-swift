namespace OpenStack.Nova
{
	using System;
	using System.Collections.Generic;
	using System.IO;

	public interface IHttpResponse
	{
		int Status { get; }
		string Reason { get; }
		Stream ResponseStream { get; }
		Dictionary<string, string> Headers { get; }
		void Close();
	}
}

