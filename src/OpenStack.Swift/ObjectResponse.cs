namespace OpenStack.Swift
{
	using System;
	using System.Collections.Generic;
	using System.IO;

	/// <summary>
	/// Used for object responses
	/// </summary>
	public class ObjectResponse : BaseResponse
	{
		/// <summary>
		/// A Stream of the object data only used for get requests
		/// </summary>
		public readonly Stream ObjectData;

	    /// <summary>
	    /// Initializes a new instance of the <see><cref>Openstack.Swift.ObjectResponse</cref></see> class.
	    /// </summary>
	    /// <param name='headers'>
	    /// The headers from the request
	    /// </param>
	    /// <param name='reason'>
	    /// The status discription of the request
	    /// </param>
	    /// <param name='status'>
	    /// The status code from the request
	    /// </param>
	    /// <param name='object_data'>
	    /// A stream of object data will be null if not a get request
	    /// </param>
	    public ObjectResponse(Dictionary<string, string> headers, string reason, int status, Stream object_data) :
			 base(headers, status, reason)
		{
			ObjectData = object_data;
		}
	}
}

