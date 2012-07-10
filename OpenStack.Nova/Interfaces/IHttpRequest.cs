namespace OpenStack.Nova
{
	using System;
	using System.IO;

    public interface IHttpRequest
	{
		bool AllowWriteStreamBuffering{ set; get; }
		bool SendChunked { set; get; }
		long ContentLength { set; get; }
		Stream GetRequestStream();
		IHttpResponse GetResponse();
	}
}

