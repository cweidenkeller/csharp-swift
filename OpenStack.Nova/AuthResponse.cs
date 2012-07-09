namespace OpenStack.Nova
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	/// <summary>
	/// An Object that holds auth information
	/// </summary>
	public class AuthResponse
	{
		/// <summary>
		/// response headers.
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

	    /// <summary>
	    /// Initializes a new instance of the <see><cref>Openstack.Swift.AuthResponse</cref></see> class.
	    /// </summary>
	    /// <param name='headers'>
	    /// Response headers
	    /// </param>
	    /// <param name='reason'>
	    /// Response status description.
	    /// </param>
	    /// <param name='status'>
	    /// Response status from the server.
	    /// </param>
	    public AuthResponse(Dictionary<string, string> headers, string reason, int status)
		{
			Headers = headers;
			Status = status;
			Reason = reason;
		}
	}
}

