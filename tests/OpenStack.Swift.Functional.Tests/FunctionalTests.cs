using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.IO;
using System.Threading;
using NUnit.Framework;
using OpenStack.Swift;
namespace OpenStack.Swift.Functional.Tests
{
    [TestFixture]
	public class TestClient
	{
		private string user_name = ConfigurationManager.AppSettings["FunctionalTestUserName"];
		private string api_key = ConfigurationManager.AppSettings["FunctionalTestApiKey"];
		private string auth_url = ConfigurationManager.AppSettings["FunctionalTestAuthUrl"];
		private string snet_pattern = "://snet-";
		private string prefix = ".csharp_swift";
		private string object_content = "unit";
		private const string auth_storage_url_header = "x-storage-url";
		private const string auth_storage_token_header = "x-storage-token";
		private const string auth_auth_token_header = "x-auth-token";
		private string[] auth_headers = {auth_storage_url_header, auth_storage_token_header, auth_auth_token_header};
		private const string container_name_key = "name";
		private const string container_bytes_used_key = "bytes";
		private const string container_object_count_key = "count";
		private string[] container_listing_keys = {container_name_key, container_bytes_used_key, container_object_count_key};
		private const string account_object_count_header = "x-account-object-count";
		private const string account_container_count_header = "x-account-container-count";
		private const string account_bytes_used_header = "x-account-bytes-used";
		private string[] account_headers = {account_object_count_header, account_container_count_header, account_bytes_used_header};
		private const string container_object_count_header = "x-container-object-count";
		private const string container_bytes_used_header = "x-container-bytes-used";
		private string[] container_headers = {container_object_count_header, container_bytes_used_header};
		private string folder_object_name = "folder/blah";
		private const string subdir_name_key = "subdir";
		private string[] subdir_listing_keys = {subdir_name_key};
		private string account_metadata_prefix = "x-account-meta-";
		private string container_metadata_prefix = "x-container-meta-";
		private string object_metadata_prefix = "x-object-meta-";
		private string metadata_key = "unit";
		private string metadata_value = "unit";
		private const string object_name_key = "name";
		private const string object_hash_key = "hash";
		private const string object_bytes_key = "bytes";
		private const string object_content_type_key = "content_type";
		private const string object_last_modified_key = "last_modified";
		private string[] object_listing_keys = {object_name_key, object_hash_key, object_bytes_key, object_bytes_key, object_content_type_key, object_last_modified_key};
		private const string object_last_modified_header = "last-modified";
		private const string object_etag_header = "etag";
		private const string object_content_length_header = "content-length";
		private const string object_content_type_header = "content-type";
		private string[] object_header_keys = {object_last_modified_header, object_etag_header, object_content_length_header, object_content_type_header};
		private string auth_token;
		private string storage_url;
		private Dictionary<string, string> teardown_query = new Dictionary<string, string>();
		private List<Dictionary<string, string>> created_objects = new List<Dictionary<string, string>>();
		private List<string> created_containers = new List<string>();
		private SwiftClient client = new SwiftClient();
		[SetUp]
		public void setup()
		{
			this.created_containers = new List<string>();
			this.created_objects = new List<Dictionary<string, string>>();
			this.client = new SwiftClient();
			AuthResponse res = client.GetAuth(this.auth_url, this.user_name, this.api_key, new Dictionary<string, string>(), new Dictionary<string, string>(), false);
			this.auth_token = res.Headers["x-auth-token"];
			this.storage_url = res.Headers["x-storage-url"];
		}
		[TearDown]
		public void teardown()
		{
			this.teardown_query["prefix"] = this.prefix;
		    try
			{
			    foreach (Dictionary<string, string> container_info in client.GetAccount(this.storage_url, this.auth_token, new Dictionary<string, string>(), this.teardown_query, false).Containers)
			    {
					foreach (Dictionary<string, string> object_info in client.GetContainer(this.storage_url, this.auth_token, container_info["name"], new Dictionary<string, string>(), new Dictionary<string, string>(), false).Objects)
					{
					    if(container_info.ContainsKey("name"))
						{
				            client.DeleteObject(this.storage_url, this.auth_token, container_info["name"], object_info["name"], new Dictionary<string, string>(), new Dictionary<string, string>());
						}
						else
						{
							client.DeleteObject(this.storage_url, this.auth_token, container_info["name"], object_info["subdir"], new Dictionary<string, string>(), new Dictionary<string, string>());
						}
					}
					client.DeleteContainer(this.storage_url, this.auth_token, container_info["name"], new Dictionary<string, string>(), new Dictionary<string, string>());
			    }
			}
			catch(ClientException)
			{
			}
			
		}
		[Test]
		public void test_get_auth()
		{
			AuthResponse res = client.GetAuth(this.auth_url, this.user_name, this.api_key, new Dictionary<string, string>(), new Dictionary<string, string>(), false);
			foreach (string header in this.auth_headers)
			{
			    Assert.IsTrue(res.Headers.ContainsKey(header), "Header: " + header);
			}
			Assert.IsTrue(res.Status < 300 || res.Status > 199);
			res = client.GetAuth(this.auth_url, this.user_name, this.api_key, new Dictionary<string, string>(), new Dictionary<string, string>(), true);
			foreach (string header in this.auth_headers)
			{
			    Assert.IsTrue(res.Headers.ContainsKey(header), "Header: " + header);
			}
			Assert.IsTrue(res.Status < 300 && res.Status > 199);
			int index = res.Headers["x-storage-url"].IndexOf(this.snet_pattern);
			//Make Sure snet- was added to the right part of the url
            Assert.IsTrue(index > 3 && index < 6);
			Assert.IsTrue(index != -1);
		}
		[Test]
		[ExpectedException(typeof(ClientException))]
		public void test_get_auth_fail()
		{
			client.GetAuth(this.auth_url, this.user_name, "sdf", new Dictionary<string, string>(), new Dictionary<string, string>(), false);
		}
		[Test]
		public void test_get_account()
		{
			this.created_containers.Add((this.prefix + Guid.NewGuid().ToString()));
			this.created_containers.Add((this.prefix + Guid.NewGuid().ToString()));
			foreach (string container_name in this.created_containers)
			{
				client.PutContainer(this.storage_url, this.auth_token, container_name, new Dictionary<string, string>(), new Dictionary<string, string>());
			}
			Dictionary<string, string> query = new Dictionary<string, string>();
			query["prefix"] = this.prefix;
			AccountResponse res = client.GetAccount(this.storage_url, this.auth_token, new Dictionary<string, string>(), query, true);
			foreach (Dictionary<string, string> container_dictionary in res.Containers)
			{
				foreach (string key in this.container_listing_keys)
				{
					Assert.IsTrue(container_dictionary.ContainsKey(key));
				}
			}
			foreach (string header in this.account_headers)
			{
			    Assert.IsTrue(res.Headers.ContainsKey(header));
		    }
		}
		[ExpectedException(typeof(ClientException))]
		[Test]
		public void test_get_account_fail()         
		{
			client.GetAccount(this.storage_url, "asdf", new Dictionary<string, string>(), new Dictionary<string, string>(), false);
		}
		[Test]
		public void test_head_account()
		{
		    AccountResponse res = client.HeadAccount(this.storage_url, this.auth_token, new Dictionary<string, string>(), new Dictionary<string, string>());
			Assert.IsTrue(res.Status > 199 && res.Status < 300);
			foreach (string header in this.account_headers)
			{
				Assert.IsTrue(res.Headers.ContainsKey(header));
			}
			res = client.HeadAccount(this.storage_url, this.auth_token, new Dictionary<string, string>(), new Dictionary<string, string>());
			Assert.IsTrue(res.Status > 199 && res.Status < 300);
			foreach (string header in this.account_headers)
			{
				Assert.IsTrue(res.Headers.ContainsKey(header));
			}
		}
		[Test]
		[ExpectedException(typeof(ClientException))]
		public void test_head_account_fail()
		{
			client.HeadAccount(this.storage_url, "asdf", new Dictionary<string, string>(), new Dictionary<string, string>());
		}
		[Test]
		public void test_post_account()
		{
			Dictionary<string, string> headers = new Dictionary<string, string>();
			headers.Add(this.account_metadata_prefix + this.metadata_key, this.metadata_value);
			client.PostAccount(this.storage_url, this.auth_token, headers, new Dictionary<string, string>());
			AccountResponse res = client.HeadAccount(this.storage_url, this.auth_token, new Dictionary<string, string>(), new Dictionary<string, string>());
			Assert.IsTrue(res.Headers.ContainsKey(this.account_metadata_prefix + this.metadata_key));
			Assert.IsTrue(res.Headers[this.account_metadata_prefix + this.metadata_key] == this.metadata_value);
			client.PostAccount(this.storage_url, this.auth_token, headers, new Dictionary<string, string>());
			res = client.HeadAccount(this.storage_url, this.auth_token, new Dictionary<string, string>(), new Dictionary<string, string>());
			Assert.IsTrue(res.Headers.ContainsKey(this.account_metadata_prefix + 
			                                      this.metadata_key));
			Assert.IsTrue(res.Headers[this.account_metadata_prefix + this.metadata_key] == this.metadata_value);
			headers[this.account_metadata_prefix + this.metadata_key] = "";
			client.PostAccount(this.storage_url, this.auth_token, headers, new Dictionary<string, string>());
			res = client.HeadAccount(this.storage_url, this.auth_token, new Dictionary<string, string>(), new Dictionary<string, string>());
			Assert.IsFalse(res.Headers.ContainsKey(this.account_metadata_prefix + this.metadata_key));
		}
		[Test]
		[ExpectedException(typeof(ClientException))]
		public void test_post_account_fail()
		{
			client.PostAccount(this.storage_url, "asdf", new Dictionary<string, string>(), new Dictionary<string, string>());
		}
		[Test]
		public void test_get_container()
		{
			string container_name = ((string)this.prefix + Guid.NewGuid().ToString());
			this.created_containers.Add(container_name);
		    Dictionary<string, string> obj_cont_pair = new Dictionary<string, string>();
			obj_cont_pair.Add("container", container_name);
			obj_cont_pair.Add("object", this.prefix + Guid.NewGuid().ToString());
			this.created_objects.Add(obj_cont_pair);
			obj_cont_pair = new Dictionary<string, string>();
			obj_cont_pair.Add("container", container_name);
			obj_cont_pair.Add("object", this.prefix + Guid.NewGuid().ToString());
			client.PutContainer(this.storage_url, this.auth_token, container_name, new Dictionary<string, string>(), new Dictionary<string, string>());
			this.created_objects.Add(obj_cont_pair);
			foreach (Dictionary<string, string> obj_dict in this.created_objects)
			{
				UTF8Encoding encoder = new UTF8Encoding();
				byte[] byte_array = encoder.GetBytes(this.object_content);
				MemoryStream stream = new MemoryStream(byte_array);
				client.PutObject(this.storage_url, this.auth_token, obj_dict["container"], obj_dict["object"], stream, new Dictionary<string, string>(), new Dictionary<string, string>());
			}
			Dictionary<string, string> query = new Dictionary<string, string>();
			query.Add("prefix", this.prefix);
			ContainerResponse res = client.GetContainer(this.storage_url, this.auth_token, container_name, new Dictionary<string, string>(), query, false);
			foreach (string header in this.container_headers)
			{
				Assert.IsTrue(res.Headers.ContainsKey(header), "Header: " + header);
			}
			foreach (Dictionary<string, string> object_hash in res.Objects)
			{
				if (object_hash.ContainsKey("subdir"))
				{
					foreach (string key in this.subdir_listing_keys)
					{
						Assert.IsTrue(object_hash.ContainsKey(key));
					}
				}
				else
				{
					foreach (string key in this.object_listing_keys)
					{
						Assert.IsTrue(object_hash.ContainsKey(key));
					}
				}
				
			}
			query.Add("limit", "1");
		    obj_cont_pair = new Dictionary<string, string>();
			obj_cont_pair.Add("container", container_name);
			obj_cont_pair.Add("object", this.folder_object_name);
			this.created_objects.Add(obj_cont_pair);
			UTF8Encoding coder = new UTF8Encoding();
			byte[] byte_array2 = coder.GetBytes(this.object_content);
		    MemoryStream stream2 = new MemoryStream(byte_array2);
		    client.PutObject(this.storage_url, this.auth_token, container_name, this.folder_object_name, stream2, new Dictionary<string, string>(), new Dictionary<string, string>());
			query = new Dictionary<string, string>();
			query.Add("delimiter", "/");
			foreach (string header in this.container_headers)
			{
				Assert.IsTrue(res.Headers.ContainsKey(header), "Header: " + header);
			}
			foreach (Dictionary<string, string> object_hash in res.Objects)
			{
				if (object_hash.ContainsKey("subdir"))
				{
					foreach (string key in this.subdir_listing_keys)
					{
						Assert.IsTrue(object_hash.ContainsKey(key));
					}
				}
				else
				{
					foreach (string key in this.object_listing_keys)
					{
						Assert.IsTrue(object_hash.ContainsKey(key));
					}
				}
				
			}
		}
		[Test]
		[ExpectedException(typeof(ClientException))]
		public void test_get_container_fail()
		{
			client.GetContainer(this.storage_url, "asdf", "asdf", new Dictionary<string, string>(), new Dictionary<string, string>(), false);
		}
		[Test]
		public void test_head_container()
		{
			string container_name = this.prefix + Guid.NewGuid().ToString();
			this.created_containers.Add(container_name);
			client.PutContainer(this.storage_url, this.auth_token, container_name, new Dictionary<string, string>(), new Dictionary<string, string>());
			ContainerResponse res = client.HeadContainer(this.storage_url, this.auth_token, container_name, new Dictionary<string, string>(), new Dictionary<string, string>());
			foreach (string header in this.container_headers)
			{
				Assert.IsTrue(res.Headers.ContainsKey(header),
				              "Header: " + header);
			}
		}
		[Test]
		[ExpectedException(typeof(ClientException))]
		public void test_head_container_fail()
		{
			client.HeadContainer(this.storage_url, "asdf", "asdf", new Dictionary<string, string>(), new Dictionary<string, string>());
		}

		[Test]
		public void test_post_container()
		{   
			Dictionary<string, string> headers = new Dictionary<string, string>();
			headers.Add(this.container_metadata_prefix + this.metadata_key, this.metadata_value);
			string container_name = this.prefix + Guid.NewGuid().ToString();
			this.created_containers.Add(container_name);
			client.PutContainer(this.storage_url, this.auth_token, container_name, new Dictionary<string, string>(), new Dictionary<string, string>());
		    client.PostContainer(this.storage_url, this.auth_token, container_name, headers, new Dictionary<string, string>());
			ContainerResponse res = client.HeadContainer(this.storage_url, this.auth_token, container_name, new Dictionary<string, string>(), new Dictionary<string, string>());
		    Assert.IsTrue(res.Headers.ContainsKey(this.container_metadata_prefix + this.metadata_key));
			Assert.IsTrue(res.Headers[this.container_metadata_prefix + this.metadata_key] == this.metadata_value);
			foreach (string header in this.container_headers)
			{
				Assert.IsTrue(res.Headers.ContainsKey(header));
			}
			headers[this.container_metadata_prefix + this.metadata_key] = "";
			client.PostContainer(this.storage_url, this.auth_token, container_name, headers, new Dictionary<string, string>());
			res = client.HeadContainer(this.storage_url, this.auth_token, container_name, new Dictionary<string, string>(), new Dictionary<string, string>());
			Assert.IsFalse(res.Headers.ContainsKey(this.container_metadata_prefix + this.metadata_key));
		}
		[Test]
		[ExpectedException(typeof(ClientException))]
		public void test_post_container_fail()
		{
			client.PostContainer(this.storage_url, "asdf", "asdf", new Dictionary<string, string>(), new Dictionary<string, string>());
		}
		[Test]
		public void test_put_container()
		{
            Dictionary<string, string> query = new Dictionary<string, string>();
            query.Add("prefix", this.prefix);
			Dictionary<string, string> headers = new Dictionary<string, string>();
			string container_name = this.prefix + Guid.NewGuid().ToString();
			this.created_containers.Add(container_name);
			client.PutContainer(this.storage_url, this.auth_token, container_name, headers, new Dictionary<string, string>());
			AccountResponse res = client.GetAccount(this.storage_url, this.auth_token, new Dictionary<string, string>(), query, false);
			Assert.IsTrue(res.Containers.Count > 0 && res.Containers.Count < 2, res.Containers.Count.ToString());
		}
		[Test]
		[ExpectedException(typeof(ClientException))]
		public void test_put_container_fail()
		{
			client.PutContainer(this.storage_url, "asdf", "asdf", new Dictionary<string, string>(), new Dictionary<string, string>());
		}
		[Test]
		public void test_delete_container()
		{
			string container_name = this.prefix + Guid.NewGuid().ToString();
			Dictionary<string, string> query = new Dictionary<string, string>();
			query.Add("prefix", this.prefix);
			client.PutContainer(this.storage_url, this.auth_token, container_name, new Dictionary<string, string>(), new Dictionary<string, string>());
			AccountResponse res = client.GetAccount(this.storage_url, this.auth_token, new Dictionary<string, string>(), query, false);
			Assert.IsTrue(res.Containers.Count > 0 && res.Containers.Count < 2);
			client.DeleteContainer(this.storage_url, this.auth_token, container_name, new Dictionary<string, string>(), new Dictionary<string, string>());
			res = client.GetAccount(this.storage_url, this.auth_token, new Dictionary<string, string>(), query, false);
            Assert.IsTrue(res.Containers.Count == 0);
		}
		[Test]
		[ExpectedException(typeof(ClientException))]
		public void test_delete_container_fail()
		{
			client.DeleteContainer(this.storage_url, "asdf", "asdf", new Dictionary<string, string>(), new Dictionary<string, string>());
		}
		[Test]
		public void test_get_object()
		{
			string name = this.prefix + Guid.NewGuid().ToString();
			this.created_containers.Add(name);
			Dictionary<string, string> obj_cont_pair = new Dictionary<string, string>();
			obj_cont_pair.Add("container", name);
			obj_cont_pair.Add("object", name);
			client.PutContainer(this.storage_url, this.auth_token, name, new Dictionary<string, string>(), new Dictionary<string, string>());
			UTF8Encoding encoder = new UTF8Encoding();
			byte[] byte_array = encoder.GetBytes(this.object_content);
			MemoryStream stream = new MemoryStream(byte_array);
			client.PutObject(this.storage_url, this.auth_token, name, name, stream, new Dictionary<string, string>(), new Dictionary<string, string>());
			ObjectResponse res = client.GetObject(this.storage_url, this.auth_token, name, name, new Dictionary<string, string>(), new Dictionary<string, string>());
			byte_array = new byte[this.object_content.Length];
			res.ObjectData.Read(byte_array, 0, this.object_content.Length);
			string content = System.Text.Encoding.UTF8.GetString(byte_array);
			Assert.IsTrue(content == this.object_content);
			foreach (string header in this.object_header_keys)
			{
				Assert.IsTrue(res.Headers.ContainsKey(header));
			}
		}
		[Test]
		[ExpectedException(typeof(ClientException))]
		public void test_get_object_fail()
		{
			client.GetObject(this.storage_url, "asdf", "asdf", "asdf", new Dictionary<string, string>(), new Dictionary<string, string>());
		}
		[Test]
		public void test_head_object()
		{
			string name = this.prefix + Guid.NewGuid().ToString();
			this.created_containers.Add(name);
			Dictionary<string, string> obj_cont_pair = new Dictionary<string, string>();
			obj_cont_pair.Add("container", name);
			obj_cont_pair.Add("object", name);
			client.PutContainer(this.storage_url, this.auth_token, name, new Dictionary<string, string>(), new Dictionary<string, string>());
			this.created_objects.Add(obj_cont_pair);
			UTF8Encoding encoder = new UTF8Encoding();
			byte[] byte_array = encoder.GetBytes(this.object_content);
			MemoryStream stream = new MemoryStream(byte_array);
			client.PutObject(this.storage_url, this.auth_token, name, name, stream, new Dictionary<string, string>(), new Dictionary<string, string>());
			ObjectResponse res = client.HeadObject(this.storage_url, this.auth_token, name, name, new Dictionary<string, string>(), new Dictionary<string, string>());
			foreach (string header in this.object_header_keys)
			{
				Assert.IsTrue(res.Headers.ContainsKey(header));
			}
		}
		[Test]
		[ExpectedException(typeof(ClientException))]
		public void test_head_object_fail()
		{
			client.HeadObject(this.storage_url, "asdf", "asdf", "asdf", new Dictionary<string, string>(), new Dictionary<string, string>());
		}
		[Test]
		public void test_post_object()
		{
			Dictionary<string, string> headers = new Dictionary<string, string>();
			headers.Add(this.object_metadata_prefix + this.metadata_key, this.metadata_value);
			string name = this.prefix + Guid.NewGuid().ToString();
			this.created_containers.Add(name);
			Dictionary<string, string> obj_cont_pair = new Dictionary<string, string>();
			obj_cont_pair = new Dictionary<string, string>();
			obj_cont_pair.Add("container", name);
			obj_cont_pair.Add("object", name);
			this.created_objects.Add(obj_cont_pair);
			client.PutContainer(this.storage_url, this.auth_token, name, new Dictionary<string, string>(), new Dictionary<string, string>());
			UTF8Encoding encoder = new UTF8Encoding();
			byte[] byte_array = encoder.GetBytes(this.object_content);
			MemoryStream stream = new MemoryStream(byte_array);
			client.PutObject(this.storage_url, this.auth_token, name, name, stream, new Dictionary<string, string>(), new Dictionary<string, string>());
			client.PostObject(this.storage_url, this.auth_token, name, name, headers, new Dictionary<string, string>());
			ObjectResponse res = client.HeadObject(this.storage_url, this.auth_token, name, name, new Dictionary<string, string>(), new Dictionary<string, string>());
			Assert.IsTrue(res.Headers.ContainsKey(this.object_metadata_prefix + this.metadata_key));
			Assert.IsTrue(res.Headers[this.object_metadata_prefix + this.metadata_key] == this.metadata_value);
			client.PostObject(this.storage_url, this.auth_token, name, name, new Dictionary<string, string>(), new Dictionary<string, string>());
			res = client.HeadObject(this.storage_url, this.auth_token, name, name, new Dictionary<string, string>(), new Dictionary<string, string>());
			Assert.IsFalse(res.Headers.ContainsKey(this.object_metadata_prefix + this.metadata_key));
		}
		[Test]
		[ExpectedException(typeof(ClientException))]
		public void test_post_object_fail()
		{
		    client.PostObject(this.storage_url, "asdf", "asdf", "asdf", new Dictionary<string, string>(), new Dictionary<string, string>());
		}
		[Test]
		public void test_put_object()
		{
			string name = this.prefix + Guid.NewGuid().ToString();
			this.created_containers.Add(name);
			Dictionary<string, string> obj_cont_pair = new Dictionary<string, string>();
			obj_cont_pair.Add("container", name);
			obj_cont_pair.Add("object", name);
			client.PutContainer(this.storage_url, this.auth_token, name, new Dictionary<string, string>(), new Dictionary<string, string>());
			this.created_objects.Add(obj_cont_pair);
			UTF8Encoding encoder = new UTF8Encoding();
			byte[] byte_array = encoder.GetBytes(this.object_content);
			MemoryStream stream = new MemoryStream(byte_array);
			client.PutObject(this.storage_url, this.auth_token, name, name, stream, new Dictionary<string, string>(), new Dictionary<string, string>());
			ObjectResponse res = client.GetObject(this.storage_url, this.auth_token, name, name, new Dictionary<string, string>(), new Dictionary<string, string>());
			byte_array = new byte[this.object_content.Length];
			res.ObjectData.Read(byte_array, 0, this.object_content.Length);
			string content = System.Text.Encoding.UTF8.GetString(byte_array);
			Assert.IsTrue(content == this.object_content);
			foreach (string header in this.object_header_keys)
			{
				Assert.IsTrue(res.Headers.ContainsKey(header));
			}
		}
		[Test]
		[ExpectedException(typeof(ClientException))]
		public void test_put_object_fail()
		{
			MemoryStream stream = new MemoryStream();
			client.PutObject(this.storage_url, "asdf", "asdf", "asdf", stream, new Dictionary<string, string>(), new Dictionary<string, string>());
		}
		[Test]
		public void test_delete_object()
		{
			string name = this.prefix + Guid.NewGuid().ToString();
			this.created_containers.Add(name);
			Dictionary<string, string> obj_cont_pair = new Dictionary<string, string>();
			obj_cont_pair.Add("container", name);
			obj_cont_pair.Add("object", name);
			client.PutContainer(this.storage_url, this.auth_token, name, new Dictionary<string, string>(), new Dictionary<string, string>());
			this.created_objects.Add(obj_cont_pair);
			UTF8Encoding encoder = new UTF8Encoding();
			byte[] byte_array = encoder.GetBytes(this.object_content);
			MemoryStream stream = new MemoryStream(byte_array);
			client.PutObject(this.storage_url, this.auth_token, name, name, stream, new Dictionary<string, string>(), new Dictionary<string, string>());
			ContainerResponse res = client.GetContainer(this.storage_url, this.auth_token, name, new Dictionary<string, string>(), new Dictionary<string, string>(), false);
			Assert.IsTrue(res.Objects.Count == 1);
			client.DeleteObject(this.storage_url, this.auth_token, name, name, new Dictionary<string, string>(), new Dictionary<string, string>());
			res = client.GetContainer(this.storage_url, this.auth_token, name, new Dictionary<string, string>(), new Dictionary<string, string>(), false);
			Assert.IsTrue(res.Objects.Count == 0);
			
		}
		[Test]
		[ExpectedException(typeof(ClientException))]
		public void test_delete_object_fail()
		{
			client.DeleteObject(this.storage_url, "asdf", "asdf", "asdf", new Dictionary<string, string>(), new Dictionary<string, string>());
		}
	}
}
