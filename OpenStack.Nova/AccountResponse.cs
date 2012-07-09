namespace OpenStack.Nova
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	/// <summary>
	/// An object containing AccountResponse information
	/// </summary>
	public class AccountResponse
	{
		/// <summary>
		/// Account Headers
		/// </summary>
	    public readonly Dictionary<string, string> Headers;
		/// <summary>
		/// the Status number of the request
		/// </summary>
		public readonly int Status;
		/// <summary>
		/// The status description of the request
		/// </summary>
		public readonly string Reason;
		/// <summary>
		/// The container list returned if a get request otherwise this will be null
		/// </summary>
		public readonly List<Dictionary<string, string>> Containers;
		/// <summary>
		/// Initializes a new instance of the <see><cref>Openstack.Swift.AccountResponse</cref></see> class.
		/// </summary>
		/// <param name='headers'>
		/// The response headers
		/// </param>
		/// <param name='reason'>
		/// The status description
		/// </param>
		/// <param name='status'>
		/// The status code of the request
		/// </param>
		/// <param name='containers'>
		/// The Container List if one is needed null otherwise
		/// </param>
	    public AccountResponse(Dictionary<string, string> headers, string reason, int status, List<Dictionary<string, string>> containers)
		{
			Headers = headers;
			Status = status;
			Reason = reason;
			Containers = containers;
		}
	}
}

