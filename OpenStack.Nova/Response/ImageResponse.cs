
namespace OpenStack.Nova
{	using System;
	using System.Collections.Generic;
	using System.IO;
	public class ImageResponse : BaseResponse
	{
		public ImageResponse (Dictionary<string, string> headers, string reason, int status) :
			base ( headers, reason, status)
		{
		}
	}
}

