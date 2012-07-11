namespace OpenStack.Nova
{
	using System;
	using System.Collections.Generic;
	using System.IO;

	public class BaseResponse
	{
		/// <summary>
		/// Headers from request
		/// </summary>
	    public readonly Dictionary<string, string> Headers;
		/// <summary>
		/// The Status code
		/// </summary>
		public readonly int Status;
		/// <summary>
		/// The status description
		/// </summary>
		public readonly string Reason;

		public BaseResponse (Dictionary<string, string> headers, string reason, int status)
		{	
			this.Headers = headers;
			this.Reason = reason;
			this.Status = status;
		}
	}
}

