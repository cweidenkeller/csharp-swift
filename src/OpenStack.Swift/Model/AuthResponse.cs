namespace OpenStack.Swift
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	/// <summary>
	/// An Object that holds auth information
	/// </summary>
	public class AuthResponse : BaseResponse
	{

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
	    public AuthResponse(Dictionary<string, string> headers, string reason, int status) :
			base(headers, status, reason)			       
		{
		}
	}
}

