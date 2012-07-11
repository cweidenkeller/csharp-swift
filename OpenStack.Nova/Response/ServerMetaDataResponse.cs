namespace OpenStack.Nova
{
	using System;
	using System.Collections.Generic;
	using System.IO;

	public class ServerMetaDataResponse : BaseResponse
	{
		public ServerMetaDataResponse (Dictionary<string, string> headers, string reason, int status):
			base ( headers, reason, status)
		{
		}
	}
}

