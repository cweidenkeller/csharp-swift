using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using NUnit.Framework;
using Openstack.Swift;
namespace Openstack.Swift.Unit.Tests
{
	[TestFixture]
	public class TestClient
	{
		private SwiftClient _client = new SwiftClient(new FakeHttpRequestFactory());
		private Dictionary<string, string> _headers = new Dictionary<string, string>();
		[SetUp]
		public void setup()
		{
            this._headers = new Dictionary<string, string>();
		}
		[Test]
		public void test_get_auth()
		{
			this._headers.Add("request-type", "auth");
			AuthResponse res = this._client.GetAuth("", "", "", this._headers, new Dictionary<string, string>(), false);
			Assert.True(res.Headers.ContainsKey("x-auth-token"));
			Assert.True(res.Headers["x-auth-token"] == "foo");
			Assert.True(res.Headers.ContainsKey("x-storage-url"));
			Assert.True(res.Headers["x-storage-url"] == "https://foo.com");
			Assert.True(res.Reason == "foo");
			Assert.True(res.Status == 201);
			res = this._client.GetAuth("", "", "", this._headers, new Dictionary<string, string>(), true);
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
		    this._headers.Add("request-type", "auth-fail");
			this._client.GetAuth("", "", "", this._headers, new Dictionary<string, string>(), false);
		}
		[Test]
		public void test_get_account()
		{
			this._headers.Add("request-type", "account");
			AccountResponse res = this._client.GetAccount("", "", this._headers, new Dictionary<string, string>(), false);
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
			this._headers.Add("request-type", "account-fail");
			this._client.GetAccount("", "", this._headers, new Dictionary<string, string>(), false);
		}
		[Test]
		public void test_head_account()
		{
			this._headers.Add("request-type", "account");
			AccountResponse res = this._client.HeadAccount("", "", this._headers, new Dictionary<string, string>());
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
		    this._headers.Add("request-type", "account-fail");
			this._client.HeadAccount("", "", this._headers, new Dictionary<string, string>());
		}
		[Test]
		public void test_post_account()
		{
			this._headers.Add("request-type", "account");
			AccountResponse res = this._client.PostAccount("", "", this._headers, new Dictionary<string, string>());
			Assert.True(res.Reason == "foo");
			Assert.True(res.Status == 201);
		}
		[Test]
		[ExpectedException(typeof(ClientException))]
		public void test_post_account_fail()
		{
			this._headers.Add("request-type", "account-fail");
			this._client.PostAccount("", "", this._headers, new Dictionary<string, string>());
		}
		[Test]
		public void test_get_container()
		{
			this._headers.Add("request-type", "container");
			ContainerResponse res = this._client.GetContainer("", "", "", this._headers, new Dictionary<string, string>(), false);
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
			this._headers.Add("request-type", "container-fail");
			this._client.GetContainer("", "", "", this._headers, new Dictionary<string, string>(), false);
		}
		[Test]
		public void test_head_container()
		{
			this._headers.Add("request-type", "container");
			ContainerResponse res = this._client.HeadContainer("", "", "", this._headers, new Dictionary<string, string>());
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
			this._headers.Add("request-type", "container-fail");
			this._client.HeadContainer("", "", "", this._headers, new Dictionary<string, string>());
		}

		[Test]
		public void test_post_container()
		{
			this._headers.Add("request-type", "container");
			ContainerResponse res = this._client.PostContainer("", "", "", this._headers, new Dictionary<string, string>());
			Assert.True(res.Reason == "foo");
			Assert.True(res.Status == 201);
		}
		[Test]
		[ExpectedException(typeof(ClientException))]
		public void test_post_container_fail()
		{
			this._headers.Add("request-type", "container-fail");
			this._client.PostContainer("", "", "", this._headers, new Dictionary<string, string>());
		}
		[Test]
		public void test_put_container()
		{
			this._headers.Add("request-type", "container");
			ContainerResponse res = this._client.PutContainer("", "", "", this._headers, new Dictionary<string, string>());
			Assert.True(res.Reason == "foo");
			Assert.True(res.Status == 201);
		}
		[Test]
		[ExpectedException(typeof(ClientException))]
		public void test_put_container_fail()
		{
			this._headers.Add("request-type", "container-fail");
			this._client.PutContainer("" , "", "", this._headers, new Dictionary<string, string>());
		}
		[Test]
		public void test_delete_container()
		{
		    this._headers.Add("request-type", "container");
			ContainerResponse res = this._client.DeleteContainer("", "", "", this._headers, new Dictionary<string, string>());
			Assert.True(res.Reason == "foo");
			Assert.True(res.Status == 201);
		}
		[Test]
		[ExpectedException(typeof(ClientException))]
		public void test_delete_container_fail()
		{
			this._headers.Add("request-type", "container-fail");
			this._client.DeleteContainer("" , "", "", this._headers, new Dictionary<string, string>());
		}
		[Test]
		public void test_get_object()
		{
			this._headers.Add("request-type", "object");
			ObjectResponse res = this._client.GetObject("", "", "", "", this._headers, new Dictionary<string, string>());
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
			this._headers.Add("request-type", "object-fail");
			this._client.GetObject("", "", "", "", this._headers, new Dictionary<string, string>());
		}
		[Test]
		public void test_head_object()
		{
			this._headers.Add("request-type", "object");
			ObjectResponse res = this._client.HeadObject("", "", "", "", this._headers, new Dictionary<string, string>());
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
			this._headers.Add("request-type", "object-fail");
			this._client.HeadObject("", "", "", "", this._headers, new Dictionary<string, string>());
		}
		[Test]
		public void test_post_object()
		{
			this._headers.Add("request-type", "object");
			ObjectResponse res = this._client.PostObject("", "", "", "", this._headers, new Dictionary<string, string>());
			Assert.True(res.Reason == "foo");
			Assert.True(res.Status == 201);
		}
		[Test]
		[ExpectedException(typeof(ClientException))]
		public void test_post_object_fail()
		{
			this._headers.Add("request-type", "object-fail");
		    this._client.PostObject("", "", "", "", this._headers, new Dictionary<string, string>());
		}
		[Test]
		public void test_put_object()
		{
			this._headers.Add("request-type", "object");
			MemoryStream stream = new MemoryStream();
			ObjectResponse res = this._client.PutObject("", "", "", "", stream, this._headers, new Dictionary<string, string>());
			Assert.True(res.Reason == "foo");
			Assert.True(res.Status == 201);
		}
		[Test]
		[ExpectedException(typeof(ClientException))]
		public void test_put_object_fail()
		{
			this._headers.Add("request-type", "object-fail");
			MemoryStream stream = new MemoryStream();
			this._client.PutObject("", "", "", "", stream, this._headers, new Dictionary<string, string>());
		}
		[Test]
		public void test_delete_object()
		{
			this._headers.Add("request-type", "object");
			ObjectResponse res = this._client.DeleteObject("", "", "", "", this._headers, new Dictionary<string, string>());
			Assert.True(res.Reason == "foo");
			Assert.True(res.Status == 201);
		}
		[Test]
		[ExpectedException(typeof(ClientException))]
		public void test_delete_object_fail()
		{
			this._headers.Add("request-type", "object-fail");
			this._client.DeleteObject("", "", "", "", this._headers, new Dictionary<string, string>());
		}
	}
    public class FakeHttpRequestFactory : IHttpRequestFactory
    {
        public IHttpRequest GetHttpRequest(string method, string url, Dictionary<string, string> headers, Dictionary<string, string> query)
        {
          return new FakeHttpRequest(method, headers);
        }
	}
    public class FakeHttpRequest : IHttpRequest
    {
        public FakeHttpRequest(string method, Dictionary<string, string> headers)
	    {
		    this._headers = headers;
			this._method = method;
		}
		private Dictionary<string, string> _headers;
		private string _method;
        public bool AllowWriteStreamBuffering{ set; get; }
		public bool SendChunked { set; get; }
		public long ContentLength { set; get; }
		public Stream GetRequestStream()
	    {
	        return new MemoryStream();
		}
		public IHttpResponse GetResponse()
		{
		    return new FakeHttpResponse(this._method, this._headers);
		}
    }
    public class FakeHttpResponse : IHttpResponse
    {
		private int _status = -1;
		public int Status { get { return this._status; } }
		private string _reason = "foo";
		public string Reason { get { return this._reason; } }
	    private Stream _stream;
		public Stream ResponseStream { get {return this._stream;} }
	    private Dictionary<string, string> _headers = new Dictionary<string, string>();
		public Dictionary<string, string> Headers { get {return this._headers;} }
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
					        this._status = 201;
					        this._headers.Add("x-auth-token", "foo");
					        this._headers.Add("x-storage-url", "https://foo.com");
					        break;
				        }
				        case "auth-fail":
				        {
					        throw new ClientException("I am a teapot", 418);
				        }
				        case "account":
				        {
					        this._status = 200;
					        this._headers.Add("x-account-meta-foo", "foo");
					        this._headers.Add("x-account-object-count", "1");
					        this._headers.Add("x-account-container-count", "1");
					        this._headers.Add("x-account-bytes-used", "1");
					        this._stream = this._to_stream("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" + 
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
					        this._status = 200;
					        this._headers.Add("x-container-meta-foo", "foo");
					        this._headers.Add("x-container-object-count", "1");
					        this._headers.Add("x-container-bytes-used", "1");
					        this._stream = this._to_stream("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
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
					        this._status = 200;
					        this._headers.Add("x-object-meta-foo", "foo");
					        this._headers.Add("content-length", "1");
					        this._headers.Add("content-type", "foo/foobar");
					        this._headers.Add("etag", "foo");
					        this._stream = this._to_stream("foo");
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
					        this._status = 201;
					        break;
					    }
					    case "account-fail":
				        {
					        throw new ClientException("I am a teapot", 418);
				        }
				        case "container":
				        {
					        this._status = 201;
				            break;
				        }
				        case "container-fail":
				        {
					        throw new ClientException("I am a teapot", 418);
				        }
				        case "object":
				        {
					        this._status = 201;
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
					        this._status = 201;
					        this._headers.Add("x-account-object-count", "1");
					        this._headers.Add("x-account-meta-foo", "foo");
					        this._headers.Add("x-account-container-count", "1");
					        this._headers.Add("x-account-bytes-used", "1");
				            break;
				        }
				        case "account-fail":
				        {
					        throw new ClientException("I am a teapot", 418);
				        }
				        case "container":
				        {
					        this._status = 201;
					        this._headers.Add("x-container-meta-foo", "foo");
					        this._headers.Add("x-container-object-count", "1");
					        this._headers.Add("x-container-bytes-used", "1");
				            break;
				        }
				        case "container-fail":
				        {
					        throw new ClientException("I am a teapot", 418);
				        }
				        case "object":
				        {
					        this._status = 201;
					        this._headers.Add("x-object-meta-foo", "foo");
					        this._headers.Add("etag", "foo");
					        this._headers.Add("content-length", "1");
					        this._headers.Add("content-type", "foo/foobar");
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
					        this._status = 201;
				            break;
				        }
				        case "account-fail":
				        {
					        throw new ClientException("I am a teapot", 418);
				        }
				        case "container":
				        {
					        this._status = 201;
				            break;
				        }
				        case "container-fail":
				        {
					        throw new ClientException("I am a teapot", 418);
				        }
				        case "object":
				        {
					        this._status = 201;
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
					        this._status = 201;
				            break;
				        }
				        case "account-fail":
				        {
					        throw new ClientException("I am a teapot", 418);
				        }
				        case "container":
				        {
					        this._status = 201;
				            break;
				        }
				        case "container-fail":
				        {
					        throw new ClientException("I am a teapot", 418);
				        }
				        case "object":
				        {
					        this._status = 201;
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