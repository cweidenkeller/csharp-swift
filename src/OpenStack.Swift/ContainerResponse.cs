namespace OpenStack.Nova
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	/// <summary>
	/// Used for container requests
	/// </summary>
	public class ContainerResponse
	{
		/// <summary>
		/// Headers from the container request
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
		/// <summary>
		/// A List of objects will be null if not a get request
		/// </summary>
		public readonly List<Dictionary<string, string>> Objects;

	    /// <summary>
	    /// Initializes a new instance of the <see><cref>Openstack.Swift.ContainerResponse</cref></see> class.
	    /// </summary>
	    /// <param name='headers'>
	    /// The response headers
	    /// </param>
	    /// <param name='reason'>
	    /// The status description
	    /// </param>
	    /// <param name='status'>
	    /// The status number of the request
	    /// </param>
	    /// <param name='objects'>
	    /// A list of objects otherwise null
	    /// </param>
	    public ContainerResponse(Dictionary<string, string> headers, string reason, int status, List<Dictionary<string, string>> objects)
		{
			Headers = headers;
			Status = status;
			Reason = reason;
			Objects = objects;
		}
	}
}

