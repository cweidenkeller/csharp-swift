namespace OpenStack.Swift
{
	using System;
	using System.Collections.Generic;

	public class BaseResponse
	{
		/// <summary>
		/// Basic response headers 
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

		public BaseResponse (Dictionary<string,string> headers, int status, string reason)
		{
			Headers = headers;
			Status = status;
			Reason = reason;
		}
	}
}

