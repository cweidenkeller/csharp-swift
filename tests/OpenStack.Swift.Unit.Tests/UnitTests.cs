using System.Collections.Generic;
using System.Text;
using System.IO;
using NUnit.Framework;
using System;

namespace OpenStack.Swift.Unit.Tests
{
	[TestFixture]
	public class TestClient
	{
		private readonly SwiftClient _client = new SwiftClient(new FakeHttpRequestFactory());
		private Dictionary<string, string> _headers = new Dictionary<string, string>();
		[SetUp]
		public void setup()
		{
            _headers = new Dictionary<string, string>();
		}
        [Test]
        public void test_default_http_request_timeout_is_one_hour()
        {
            var httpRequest = new HttpRequest(
                "GET", 
                "http://tinyurl.com", 
                new Dictionary<string, string>(), 
                new Dictionary<string, string>());
            Assert.That(httpRequest.Timeout, Is.EqualTo(3600000));
        }
		[Test]
		public void test_get_auth()
		{
			_headers.Add("request-type", "auth");
			var res = _client.GetAuth("", "", "", _headers, new Dictionary<string, string>(), false);
			Assert.True(res.Headers.ContainsKey("x-auth-token"));
			Assert.True(res.Headers["x-auth-token"] == "foo");
			Assert.True(res.Headers.ContainsKey("x-storage-url"));
			Assert.True(res.Headers["x-storage-url"] == "https://foo.com");
			Assert.True(res.Reason == "foo");
			Assert.True(res.Status == 201);
			res = _client.GetAuth("", "", "", _headers, new Dictionary<string, string>(), true);
			Assert.True(res.Headers.ContainsKey("x-auth-token"));
			Assert.True(res.Headers["x-auth-token"] == "foo");
			Assert.True(res.Headers.ContainsKey("x-storage-url"));
			Assert.True(res.Headers["x-storage-url"] == "https://snet-foo.com/");
			Assert.True(res.Reason == "foo");
			Assert.True(res.Status == 201);
		}
		[Test]
		[ExpectedException(typeof(ClientException))]
		public void test_get_auth_fail()
		{
		    _headers.Add("request-type", "auth-fail");
			_client.GetAuth("", "", "", _headers, new Dictionary<string, string>(), false);
		}
		[Test]
		public void test_get_account()
		{
			_headers.Add("request-type", "account");
			AccountResponse res = _client.GetAccount("", "", _headers, new Dictionary<string, string>(), false);
			Assert.True(res.Headers.ContainsKey("x-account-container-count"));
			Assert.True(res.Headers["x-account-container-count"] == "1");
			Assert.True(res.Headers.ContainsKey("x-account-object-count"));
			Assert.True(res.Headers["x-account-object-count"] == "1");
			Assert.True(res.Headers.ContainsKey("x-account-meta-foo"));
			Assert.True(res.Headers["x-account-meta-foo"] == "foo");
			Assert.True(res.Headers.ContainsKey("x-account-bytes-used"));
			Assert.True(res.Headers["x-account-bytes-used"] == "1");
			Assert.True(res.Reason == "foo");
			Assert.True(res.Status == 200);
			Assert.True(res.Containers[0]["count"] == "1");
			Assert.True(res.Containers[0]["bytes"] == "1");
			Assert.True(res.Containers[0]["name"] == "foo");
		}
		[ExpectedException(typeof(ClientException))]
		[Test]
		public void test_get_account_fail()         
		{
			_headers.Add("request-type", "account-fail");
			_client.GetAccount("", "", _headers, new Dictionary<string, string>(), false);
		}
		[Test]
		public void test_head_account()
		{
			_headers.Add("request-type", "account");
			AccountResponse res = _client.HeadAccount("", "", _headers, new Dictionary<string, string>());
			Assert.True(res.Headers.ContainsKey("x-account-container-count"));
			Assert.True(res.Headers["x-account-container-count"] == "1");
			Assert.True(res.Headers.ContainsKey("x-account-object-count"));
			Assert.True(res.Headers["x-account-object-count"] == "1");
			Assert.True(res.Headers.ContainsKey("x-account-meta-foo"));
			Assert.True(res.Headers["x-account-meta-foo"] == "foo");
			Assert.True(res.Headers.ContainsKey("x-account-bytes-used"));
			Assert.True(res.Headers["x-account-bytes-used"] == "1");
			Assert.True(res.Reason == "foo");
			Assert.True(res.Status == 201);
		}
		[Test]
		[ExpectedException(typeof(ClientException))]
		public void test_head_account_fail()
		{
		    _headers.Add("request-type", "account-fail");
			_client.HeadAccount("", "", _headers, new Dictionary<string, string>());
		}
		[Test]
		public void test_post_account()
		{
			_headers.Add("request-type", "account");
			AccountResponse res = _client.PostAccount("", "", _headers, new Dictionary<string, string>());
			Assert.True(res.Reason == "foo");
			Assert.True(res.Status == 201);
		}
		[Test]
		[ExpectedException(typeof(ClientException))]
		public void test_post_account_fail()
		{
			_headers.Add("request-type", "account-fail");
			_client.PostAccount("", "", _headers, new Dictionary<string, string>());
		}
		[Test]
		public void test_get_container()
		{
			_headers.Add("request-type", "container");
			ContainerResponse res = _client.GetContainer("", "", "", _headers, new Dictionary<string, string>(), false);
			Assert.True(res.Headers.ContainsKey("x-container-object-count"));
			Assert.True(res.Headers["x-container-object-count"] == "1");
			Assert.True(res.Headers.ContainsKey("x-container-bytes-used"));
			Assert.True(res.Headers["x-container-bytes-used"] == "1");
			Assert.True(res.Headers.ContainsKey("x-container-meta-foo"));
			Assert.True(res.Headers["x-container-meta-foo"] == "foo");
			Assert.True(res.Reason == "foo");
			Assert.True(res.Status == 200);
			Assert.True(res.Objects[0]["content_type"] == "foo/foo");
		    Assert.True(res.Objects[0]["hash"] == "foo");
			Assert.True(res.Objects[0]["last_modified"] == "foo");
			Assert.True(res.Objects[0]["bytes"] == "1");
			Assert.True(res.Objects[0]["name"] == "foo");
		}
		[Test]
		[ExpectedException(typeof(ClientException))]
		public void test_get_container_fail()
		{
			_headers.Add("request-type", "container-fail");
			_client.GetContainer("", "", "", _headers, new Dictionary<string, string>(), false);
		}
		[Test]
		public void test_head_container()
		{
			_headers.Add("request-type", "container");
			ContainerResponse res = _client.HeadContainer("", "", "", _headers, new Dictionary<string, string>());
			Assert.True(res.Headers.ContainsKey("x-container-object-count"));
			Assert.True(res.Headers["x-container-object-count"] == "1");
			Assert.True(res.Headers.ContainsKey("x-container-bytes-used"));
			Assert.True(res.Headers["x-container-bytes-used"] == "1");
			Assert.True(res.Headers.ContainsKey("x-container-meta-foo"));
			Assert.True(res.Headers["x-container-meta-foo"] == "foo");
			Assert.True(res.Reason == "foo");
			Assert.True(res.Status == 201);
		}
		[Test]
		[ExpectedException(typeof(ClientException))]
		public void test_head_container_fail()
    	{
			_headers.Add("request-type", "container-fail");
			_client.HeadContainer("", "", "", _headers, new Dictionary<string, string>());
		}

		[Test]
		public void test_post_container()
		{
			_headers.Add("request-type", "container");
			ContainerResponse res = _client.PostContainer("", "", "", _headers, new Dictionary<string, string>());
			Assert.True(res.Reason == "foo");
			Assert.True(res.Status == 201);
		}
		[Test]
		[ExpectedException(typeof(ClientException))]
		public void test_post_container_fail()
		{
			_headers.Add("request-type", "container-fail");
			_client.PostContainer("", "", "", _headers, new Dictionary<string, string>());
		}
		[Test]
		public void test_put_container()
		{
			_headers.Add("request-type", "container");
			ContainerResponse res = _client.PutContainer("", "", "", _headers, new Dictionary<string, string>());
			Assert.True(res.Reason == "foo");
			Assert.True(res.Status == 201);
		}
		[Test]
		[ExpectedException(typeof(ClientException))]
		public void test_put_container_fail()
		{
			_headers.Add("request-type", "container-fail");
			_client.PutContainer("" , "", "", _headers, new Dictionary<string, string>());
		}
		[Test]
		public void test_delete_container()
		{
		    _headers.Add("request-type", "container");
			ContainerResponse res = _client.DeleteContainer("", "", "", _headers, new Dictionary<string, string>());
			Assert.True(res.Reason == "foo");
			Assert.True(res.Status == 201);
		}
		[Test]
		[ExpectedException(typeof(ClientException))]
		public void test_delete_container_fail()
		{
			_headers.Add("request-type", "container-fail");
			_client.DeleteContainer("" , "", "", _headers, new Dictionary<string, string>());
		}
		[Test]
		public void test_get_object()
		{
			_headers.Add("request-type", "object");
			ObjectResponse res = _client.GetObject("", "", "", "", _headers, new Dictionary<string, string>());
			Assert.True(res.Headers.ContainsKey("content-length"));
			Assert.True(res.Headers["content-length"] == "1");
			Assert.True(res.Headers.ContainsKey("content-type"));
			Assert.True(res.Headers["content-type"] == "foo/foobar");
			Assert.True(res.Headers.ContainsKey("x-object-meta-foo"));
			Assert.True(res.Headers["x-object-meta-foo"] == "foo");
			Assert.True(res.Headers.ContainsKey("etag"));
			Assert.True(res.Headers["etag"] == "foo");
			Assert.True(res.Reason == "foo");
			Assert.True(res.Status == 200);
			Assert.True(new StreamReader(res.ObjectData).ReadToEnd() == "foo");
		}
		[Test]
		[ExpectedException(typeof(ClientException))]
		public void test_get_object_fail()
		{
			_headers.Add("request-type", "object-fail");
			_client.GetObject("", "", "", "", _headers, new Dictionary<string, string>());
		}
		[Test]
		public void test_head_object()
		{
			_headers.Add("request-type", "object");
			ObjectResponse res = _client.HeadObject("", "", "", "", _headers, new Dictionary<string, string>());
			Assert.True(res.Headers.ContainsKey("content-length"));
			Assert.True(res.Headers["content-length"] == "1");
			Assert.True(res.Headers.ContainsKey("content-type"));
			Assert.True(res.Headers["content-type"] == "foo/foobar");
			Assert.True(res.Headers.ContainsKey("x-object-meta-foo"));
			Assert.True(res.Headers["x-object-meta-foo"] == "foo");
			Assert.True(res.Headers.ContainsKey("etag"));
			Assert.True(res.Headers["etag"] == "foo");
			Assert.True(res.Reason == "foo");
			Assert.True(res.Status == 201);
		}
		[Test]
		[ExpectedException(typeof(ClientException))]
		public void test_head_object_fail()
    	{
			_headers.Add("request-type", "object-fail");
			_client.HeadObject("", "", "", "", _headers, new Dictionary<string, string>());
		}
		[Test]
		public void test_post_object()
		{
			_headers.Add("request-type", "object");
			ObjectResponse res = _client.PostObject("", "", "", "", _headers, new Dictionary<string, string>());
			Assert.True(res.Reason == "foo");
			Assert.True(res.Status == 201);
		}
		[Test]
		[ExpectedException(typeof(ClientException))]
		public void test_post_object_fail()
		{
			_headers.Add("request-type", "object-fail");
		    _client.PostObject("", "", "", "", _headers, new Dictionary<string, string>());
		}
		[Test]
		public void test_put_object()
		{
			_headers.Add("request-type", "object");
			var stream = new MemoryStream();
			ObjectResponse res = _client.PutObject("", "", "", "", stream, _headers, new Dictionary<string, string>());
			Assert.True(res.Reason == "foo");
			Assert.True(res.Status == 201);
		}
		[Test]
		[ExpectedException(typeof(ClientException))]
		public void test_put_object_fail()
		{
			_headers.Add("request-type", "object-fail");
			var stream = new MemoryStream();
			_client.PutObject("", "", "", "", stream, _headers, new Dictionary<string, string>());
		}
		[Test]
		public void test_delete_object()
		{
			_headers.Add("request-type", "object");
			ObjectResponse res = _client.DeleteObject("", "", "", "", _headers, new Dictionary<string, string>());
			Assert.True(res.Reason == "foo");
			Assert.True(res.Status == 201);
		}
		[Test]
		[ExpectedException(typeof(ClientException))]
		public void test_delete_object_fail()
		{
			_headers.Add("request-type", "object-fail");
			_client.DeleteObject("", "", "", "", _headers, new Dictionary<string, string>());
		}
	}
    public class FakeHttpRequestFactory : IHttpRequestFactory
    {
        public IHttpRequest GetHttpRequest(string method, string url, 
            Dictionary<string, string> headers=null, Dictionary<string, string> query=null, Tuple<long,long> byteRange=null)
        {
          return new FakeHttpRequest(method, headers);
        }
	}
    public class FakeHttpRequest : IHttpRequest
    {
        public FakeHttpRequest(string method, Dictionary<string, string> headers)
	    {
		    _headers = headers;
			_method = method;
		}
		private readonly Dictionary<string, string> _headers;
		private readonly string _method;
        public bool AllowWriteStreamBuffering{ set; get; }
		public bool SendChunked { set; get; }
		public long ContentLength { set; get; }
		public Stream GetRequestStream()
	    {
	        return new MemoryStream();
		}
		public IHttpResponse GetResponse()
		{
		    return new FakeHttpResponse(_method, _headers);
		}
    }
    public class FakeHttpResponse : IHttpResponse
    {
		private readonly int _status = -1;
		public int Status { get { return _status; } }
        private const string _reason = "foo";
        public string Reason { get { return _reason; } }
	    private readonly Stream _stream;
		public Stream ResponseStream { get {return _stream;} }
	    private readonly Dictionary<string, string> _headers = new Dictionary<string, string>();
		public Dictionary<string, string> Headers { get {return _headers;} }
		public void Close() {}
	    public FakeHttpResponse(string method, Dictionary<string, string> headers)
		{
			switch(method)
		    {
				case "GET":
				{
				    switch(headers["request-type"])
				    {
				        case "auth":
				        {
					        _status = 201;
					        _headers.Add("x-auth-token", "foo");
					        _headers.Add("x-storage-url", "https://foo.com");
					        break;
				        }
				        case "auth-fail":
				        {
					        throw new ClientException("I am a teapot", 418);
				        }
				        case "account":
				        {
					        _status = 200;
					        _headers.Add("x-account-meta-foo", "foo");
					        _headers.Add("x-account-object-count", "1");
					        _headers.Add("x-account-container-count", "1");
					        _headers.Add("x-account-bytes-used", "1");
					        _stream = _to_stream("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" + 
						                                   "<account name=\"foo\">\n" + 
						                                   "<container><name>foo</name><count>1</count><bytes>1</bytes></container>\n</account>");
				            break;
				        }
				        case "account-fail":
				        {
					        throw new ClientException("I am a teapot", 418);
				        }
				        case "container":
				        {
					        _status = 200;
					        _headers.Add("x-container-meta-foo", "foo");
					        _headers.Add("x-container-object-count", "1");
					        _headers.Add("x-container-bytes-used", "1");
					        _stream = _to_stream("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                                                           "<container name=\"foo\">\n" + 
						                                   "<object><name>foo</name>" +
						                                   "<hash>foo</hash>" +
						                                   "<bytes>1</bytes>" +
						                                   "<content_type>foo/foo</content_type>" +
						                                   "<last_modified>foo</last_modified>" +
						                                   "</object>" +
						                                   "</container>\n");
				            break;
				        }
				        case "container-fail":
				        {
					        throw new ClientException("I am a teapot", 418);
				        }
				        case "object":
				        {
					        _status = 200;
					        _headers.Add("x-object-meta-foo", "foo");
					        _headers.Add("content-length", "1");
					        _headers.Add("content-type", "foo/foobar");
					        _headers.Add("etag", "foo");
					        _stream = _to_stream("foo");
				            break;
				        }
				        case "object-fail":
				        {
					        throw new ClientException("I am a teapot", 418);
				        }
				    }
			        break;
			    }
			    case "POST":
			    {
				    switch(headers["request-type"])
				    {
				        case "account":
				        {
					        _status = 201;
					        break;
					    }
					    case "account-fail":
				        {
					        throw new ClientException("I am a teapot", 418);
				        }
				        case "container":
				        {
					        _status = 201;
				            break;
				        }
				        case "container-fail":
				        {
					        throw new ClientException("I am a teapot", 418);
				        }
				        case "object":
				        {
					        _status = 201;
				            break;
				        }
				        case "object-fail":
				        {
					        throw new ClientException("I am a teapot", 418);
				        }
				    }
				    break;
			    }
			    case "HEAD":
			    {
				    switch(headers["request-type"])
				    {
				        case "account":
				        {
					        _status = 201;
					        _headers.Add("x-account-object-count", "1");
					        _headers.Add("x-account-meta-foo", "foo");
					        _headers.Add("x-account-container-count", "1");
					        _headers.Add("x-account-bytes-used", "1");
				            break;
				        }
				        case "account-fail":
				        {
					        throw new ClientException("I am a teapot", 418);
				        }
				        case "container":
				        {
					        _status = 201;
					        _headers.Add("x-container-meta-foo", "foo");
					        _headers.Add("x-container-object-count", "1");
					        _headers.Add("x-container-bytes-used", "1");
				            break;
				        }
				        case "container-fail":
				        {
					        throw new ClientException("I am a teapot", 418);
				        }
				        case "object":
				        {
					        _status = 201;
					        _headers.Add("x-object-meta-foo", "foo");
					        _headers.Add("etag", "foo");
					        _headers.Add("content-length", "1");
					        _headers.Add("content-type", "foo/foobar");
				            break;
				        }
				        case "object-fail":
				        {
					        throw new ClientException("I am a teapot", 418);
				        }
				    }
			        break;
			    }
			    case "DELETE":
			    {
				    switch(headers["request-type"])
				    {
				        case "account":
				        {
					        _status = 201;
				            break;
				        }
				        case "account-fail":
				        {
					        throw new ClientException("I am a teapot", 418);
				        }
				        case "container":
				        {
					        _status = 201;
				            break;
				        }
				        case "container-fail":
				        {
					        throw new ClientException("I am a teapot", 418);
				        }
				        case "object":
				        {
					        _status = 201;
				            break;
				        }
				        case "object-fail":
				        {
					        throw new ClientException("I am a teapot", 418);
				        }
				    }
			        break;
			    }
			    case "PUT":
			    {
				    switch(headers["request-type"])
				    {
				        case "account":
				        {
					        _status = 201;
				            break;
				        }
				        case "account-fail":
				        {
					        throw new ClientException("I am a teapot", 418);
				        }
				        case "container":
				        {
					        _status = 201;
				            break;
				        }
				        case "container-fail":
				        {
					        throw new ClientException("I am a teapot", 418);
				        }
				        case "object":
				        {
					        _status = 201;
				            break;
				        }
				        case "object-fail":
				        {
					        throw new ClientException("I am a teapot", 418);
				        }
				    }
			        break;
			    }
			}
		}
		private Stream _to_stream(string to_stream)
		{
			return new MemoryStream(Encoding.UTF8.GetBytes(to_stream));
		}
    }
}