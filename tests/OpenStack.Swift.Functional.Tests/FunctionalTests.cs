using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Text;
using System.IO;
using NUnit.Framework;

namespace OpenStack.Swift.Functional.Tests
{
    [TestFixture]
	public class TestClient
	{
		private readonly string user_name = ConfigurationManager.AppSettings["FunctionalTestUserName"];
		private readonly string api_key = ConfigurationManager.AppSettings["FunctionalTestApiKey"];
		private readonly string auth_url = ConfigurationManager.AppSettings["FunctionalTestAuthUrl"];
        private const string snet_pattern = "://snet-";
        private const string prefix = ".csharp_swift";
        private const string object_content = "unit";
        private const string auth_storage_url_header = "x-storage-url";
		private const string auth_storage_token_header = "x-storage-token";
		private const string auth_auth_token_header = "x-auth-token";
		private readonly string[] auth_headers = {auth_storage_url_header, auth_storage_token_header, auth_auth_token_header};
		private const string container_name_key = "name";
		private const string container_bytes_used_key = "bytes";
		private const string container_object_count_key = "count";
		private readonly string[] container_listing_keys = {container_name_key, container_bytes_used_key, container_object_count_key};
		private const string account_object_count_header = "x-account-object-count";
		private const string account_container_count_header = "x-account-container-count";
		private const string account_bytes_used_header = "x-account-bytes-used";
		private readonly string[] account_headers = {account_object_count_header, account_container_count_header, account_bytes_used_header};
		private const string container_object_count_header = "x-container-object-count";
		private const string container_bytes_used_header = "x-container-bytes-used";
		private readonly string[] container_headers = {container_object_count_header, container_bytes_used_header};
        private const string folder_object_name = "folder/blah";
        private const string subdir_name_key = "subdir";
		private readonly string[] subdir_listing_keys = {subdir_name_key};
        private const string account_metadata_prefix = "x-account-meta-";
        private const string container_metadata_prefix = "x-container-meta-";
        private const string object_metadata_prefix = "x-object-meta-";
        private const string metadata_key = "unit";
        private const string metadata_value = "unit";
        private const string object_name_key = "name";
		private const string object_hash_key = "hash";
		private const string object_bytes_key = "bytes";
		private const string object_content_type_key = "content_type";
		private const string object_last_modified_key = "last_modified";
		private readonly string[] object_listing_keys = {object_name_key, object_hash_key, object_bytes_key, object_bytes_key, object_content_type_key, object_last_modified_key};
		private const string object_last_modified_header = "last-modified";
		private const string object_etag_header = "etag";
		private const string object_content_length_header = "content-length";
		private const string object_content_type_header = "content-type";
		private readonly string[] object_header_keys = {object_last_modified_header, object_etag_header, object_content_length_header, object_content_type_header};
		private string auth_token;
		private string storage_url;
		private readonly Dictionary<string, string> teardown_query = new Dictionary<string, string>();
		private List<Dictionary<string, string>> created_objects = new List<Dictionary<string, string>>();
		private List<string> created_containers = new List<string>();
		private SwiftClient client = new SwiftClient();
		[SetUp]
		public void setup()
		{
			created_containers = new List<string>();
			created_objects = new List<Dictionary<string, string>>();
			client = new SwiftClient();
			var res = client.GetAuth(auth_url, user_name, api_key, new Dictionary<string, string>(), new Dictionary<string, string>(), false);
			auth_token = res.Headers["x-auth-token"];
			storage_url = res.Headers["x-storage-url"];
		}
		[TearDown]
		public void teardown()
		{
			teardown_query["prefix"] = prefix;
		    try
			{
			    foreach (var container_info in client.GetAccount(storage_url, auth_token, new Dictionary<string, string>(), teardown_query, false).Containers)
			    {
					foreach (var object_info in client.GetContainer(storage_url, auth_token, container_info["name"], new Dictionary<string, string>(), new Dictionary<string, string>(), false).Objects)
					{
					    client.DeleteObject(storage_url, auth_token, container_info["name"],
					                        container_info.ContainsKey("name") ? object_info["name"] : object_info["subdir"],
					                        new Dictionary<string, string>(), new Dictionary<string, string>());
					}
					client.DeleteContainer(storage_url, auth_token, container_info["name"], new Dictionary<string, string>(), new Dictionary<string, string>());
			    }
			}
			catch(ClientException)
			{
			}
			
		}
		[Test]
		public void test_get_auth()
		{
			var res = client.GetAuth(auth_url, user_name, api_key, new Dictionary<string, string>(), new Dictionary<string, string>(), false);
			foreach (var header in auth_headers)
			{
			    Assert.IsTrue(res.Headers.ContainsKey(header), "Header: " + header);
			}
			Assert.IsTrue(res.Status < 300 || res.Status > 199);
			res = client.GetAuth(auth_url, user_name, api_key, new Dictionary<string, string>(), new Dictionary<string, string>(), true);
			foreach (var header in auth_headers)
			{
			    Assert.IsTrue(res.Headers.ContainsKey(header), "Header: " + header);
			}
			Assert.IsTrue(res.Status < 300 && res.Status > 199);
			var index = res.Headers["x-storage-url"].IndexOf(snet_pattern, StringComparison.Ordinal);
			//Make Sure snet- was added to the right part of the url
            Assert.IsTrue(index > 3 && index < 6);
			Assert.IsTrue(index != -1);
		}
		[Test]
		[ExpectedException(typeof(ClientException))]
		public void test_get_auth_fail()
		{
			client.GetAuth(auth_url, user_name, "sdf", new Dictionary<string, string>(), new Dictionary<string, string>(), false);
		}
		[Test]
		public void test_get_account()
		{
			created_containers.Add((prefix + Guid.NewGuid().ToString()));
			created_containers.Add((prefix + Guid.NewGuid().ToString()));
			foreach (string container_name in created_containers)
			{
				client.PutContainer(storage_url, auth_token, container_name, new Dictionary<string, string>(), new Dictionary<string, string>());
			}
			var query = new Dictionary<string, string>();
			query["prefix"] = prefix;
			var res = client.GetAccount(storage_url, auth_token, new Dictionary<string, string>(), query, true);
			foreach (var container_dictionary in res.Containers)
			{
				foreach (var key in container_listing_keys)
				{
					Assert.IsTrue(container_dictionary.ContainsKey(key));
				}
			}
			foreach (var header in account_headers)
			{
			    Assert.IsTrue(res.Headers.ContainsKey(header));
		    }
		}
		[ExpectedException(typeof(ClientException))]
		[Test]
		public void test_get_account_fail()         
		{
			client.GetAccount(storage_url, "asdf", new Dictionary<string, string>(), new Dictionary<string, string>(), false);
		}
		[Test]
		public void test_head_account()
		{
		    AccountResponse res = client.HeadAccount(storage_url, auth_token, new Dictionary<string, string>(), new Dictionary<string, string>());
			Assert.IsTrue(res.Status > 199 && res.Status < 300);
			foreach (string header in account_headers)
			{
				Assert.IsTrue(res.Headers.ContainsKey(header));
			}
			res = client.HeadAccount(storage_url, auth_token, new Dictionary<string, string>(), new Dictionary<string, string>());
			Assert.IsTrue(res.Status > 199 && res.Status < 300);
			foreach (string header in account_headers)
			{
				Assert.IsTrue(res.Headers.ContainsKey(header));
			}
		}
		[Test]
		[ExpectedException(typeof(ClientException))]
		public void test_head_account_fail()
		{
			client.HeadAccount(storage_url, "asdf", new Dictionary<string, string>(), new Dictionary<string, string>());
		}
		[Test]
		public void test_post_account()
		{
			var headers = new Dictionary<string, string> {{account_metadata_prefix + metadata_key, metadata_value}};
		    client.PostAccount(storage_url, auth_token, headers, new Dictionary<string, string>());
			var res = client.HeadAccount(storage_url, auth_token, new Dictionary<string, string>(), new Dictionary<string, string>());
			Assert.IsTrue(res.Headers.ContainsKey(account_metadata_prefix + metadata_key));
			Assert.IsTrue(res.Headers[account_metadata_prefix + metadata_key] == metadata_value);
			client.PostAccount(storage_url, auth_token, headers, new Dictionary<string, string>());
			res = client.HeadAccount(storage_url, auth_token, new Dictionary<string, string>(), new Dictionary<string, string>());
			Assert.IsTrue(res.Headers.ContainsKey(account_metadata_prefix + 
			                                      metadata_key));
			Assert.IsTrue(res.Headers[account_metadata_prefix + metadata_key] == metadata_value);
			headers[account_metadata_prefix + metadata_key] = "";
			client.PostAccount(storage_url, auth_token, headers, new Dictionary<string, string>());
			res = client.HeadAccount(storage_url, auth_token, new Dictionary<string, string>(), new Dictionary<string, string>());
			Assert.IsFalse(res.Headers.ContainsKey(account_metadata_prefix + metadata_key));
		}
		[Test]
		[ExpectedException(typeof(ClientException))]
		public void test_post_account_fail()
		{
			client.PostAccount(storage_url, "asdf", new Dictionary<string, string>(), new Dictionary<string, string>());
		}
		[Test]
		public void test_get_container()
		{
			string container_name = (prefix + Guid.NewGuid().ToString());
			created_containers.Add(container_name);
		    var obj_cont_pair = new Dictionary<string, string>
		                            {{"container", container_name}, {"object", prefix + Guid.NewGuid().ToString()}};
		    created_objects.Add(obj_cont_pair);
			obj_cont_pair = new Dictionary<string, string>
			                    {{"container", container_name}, {"object", prefix + Guid.NewGuid().ToString()}};
		    client.PutContainer(storage_url, auth_token, container_name, new Dictionary<string, string>(), new Dictionary<string, string>());
			created_objects.Add(obj_cont_pair);
			foreach (var obj_dict in created_objects)
			{
				var encoder = new UTF8Encoding();
				byte[] byte_array = encoder.GetBytes(object_content);
				var stream = new MemoryStream(byte_array);
				client.PutObject(storage_url, auth_token, obj_dict["container"], obj_dict["object"], stream, new Dictionary<string, string>(), new Dictionary<string, string>());
			}
			var query = new Dictionary<string, string> {{"prefix", prefix}};
		    var res = client.GetContainer(storage_url, auth_token, container_name, new Dictionary<string, string>(), query, false);
			foreach (var header in container_headers)
			{
				Assert.IsTrue(res.Headers.ContainsKey(header), "Header: " + header);
			}
			foreach (var object_hash in res.Objects)
			{
				if (object_hash.ContainsKey("subdir"))
				{
					foreach (string key in subdir_listing_keys)
					{
						Assert.IsTrue(object_hash.ContainsKey(key));
					}
				}
				else
				{
					foreach (var key in object_listing_keys)
					{
						Assert.IsTrue(object_hash.ContainsKey(key));
					}
				}
				
			}
			query.Add("limit", "1");
		    obj_cont_pair = new Dictionary<string, string> {{"container", container_name}, {"object", folder_object_name}};
		    created_objects.Add(obj_cont_pair);
			var coder = new UTF8Encoding();
			var byte_array2 = coder.GetBytes(object_content);
		    var stream2 = new MemoryStream(byte_array2);
		    client.PutObject(storage_url, auth_token, container_name, folder_object_name, stream2, new Dictionary<string, string>(), new Dictionary<string, string>());
			foreach (var header in container_headers)
			{
				Assert.IsTrue(res.Headers.ContainsKey(header), "Header: " + header);
			}
			foreach (var object_hash in res.Objects)
			{
				if (object_hash.ContainsKey("subdir"))
				{
					foreach (var key in subdir_listing_keys)
					{
						Assert.IsTrue(object_hash.ContainsKey(key));
					}
				}
				else
				{
					foreach (string key in object_listing_keys)
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
			client.GetContainer(storage_url, "asdf", "asdf", new Dictionary<string, string>(), new Dictionary<string, string>(), false);
		}
		[Test]
		public void test_head_container()
		{
			var container_name = prefix + Guid.NewGuid().ToString();
			created_containers.Add(container_name);
			client.PutContainer(storage_url, auth_token, container_name, new Dictionary<string, string>(), new Dictionary<string, string>());
			var res = client.HeadContainer(storage_url, auth_token, container_name, new Dictionary<string, string>(), new Dictionary<string, string>());
			foreach (var header in container_headers)
			{
				Assert.IsTrue(res.Headers.ContainsKey(header),
				              "Header: " + header);
			}
		}
		[Test]
		[ExpectedException(typeof(ClientException))]
		public void test_head_container_fail()
		{
			client.HeadContainer(storage_url, "asdf", "asdf", new Dictionary<string, string>(), new Dictionary<string, string>());
		}

		[Test]
		public void test_post_container()
		{   
			var headers = new Dictionary<string, string> {{container_metadata_prefix + metadata_key, metadata_value}};
		    var container_name = prefix + Guid.NewGuid().ToString();
			created_containers.Add(container_name);
			client.PutContainer(storage_url, auth_token, container_name, new Dictionary<string, string>(), new Dictionary<string, string>());
		    client.PostContainer(storage_url, auth_token, container_name, headers, new Dictionary<string, string>());
			var res = client.HeadContainer(storage_url, auth_token, container_name, new Dictionary<string, string>(), new Dictionary<string, string>());
		    Assert.IsTrue(res.Headers.ContainsKey(container_metadata_prefix + metadata_key));
			Assert.IsTrue(res.Headers[container_metadata_prefix + metadata_key] == metadata_value);
			foreach (var header in container_headers)
			{
				Assert.IsTrue(res.Headers.ContainsKey(header));
			}
			headers[container_metadata_prefix + metadata_key] = "";
			client.PostContainer(storage_url, auth_token, container_name, headers, new Dictionary<string, string>());
			res = client.HeadContainer(storage_url, auth_token, container_name, new Dictionary<string, string>(), new Dictionary<string, string>());
			Assert.IsFalse(res.Headers.ContainsKey(container_metadata_prefix + metadata_key));
		}
		[Test]
		[ExpectedException(typeof(ClientException))]
		public void test_post_container_fail()
		{
			client.PostContainer(storage_url, "asdf", "asdf", new Dictionary<string, string>(), new Dictionary<string, string>());
		}
		[Test]
		public void test_put_container()
		{
            var query = new Dictionary<string, string> {{"prefix", prefix}};
		    var headers = new Dictionary<string, string>();
			var container_name = prefix + Guid.NewGuid().ToString();
			created_containers.Add(container_name);
			client.PutContainer(storage_url, auth_token, container_name, headers, new Dictionary<string, string>());
			var res = client.GetAccount(storage_url, auth_token, new Dictionary<string, string>(), query, false);
			Assert.IsTrue(res.Containers.Count > 0 && res.Containers.Count < 2, res.Containers.Count.ToString(CultureInfo.InvariantCulture));
		}
		[Test]
		[ExpectedException(typeof(ClientException))]
		public void test_put_container_fail()
		{
			client.PutContainer(storage_url, "asdf", "asdf", new Dictionary<string, string>(), new Dictionary<string, string>());
		}
		[Test]
		public void test_delete_container()
		{
			string container_name = prefix + Guid.NewGuid().ToString();
			var query = new Dictionary<string, string> {{"prefix", prefix}};
		    client.PutContainer(storage_url, auth_token, container_name, new Dictionary<string, string>(), new Dictionary<string, string>());
			var res = client.GetAccount(storage_url, auth_token, new Dictionary<string, string>(), query, false);
			Assert.IsTrue(res.Containers.Count > 0 && res.Containers.Count < 2);
			client.DeleteContainer(storage_url, auth_token, container_name, new Dictionary<string, string>(), new Dictionary<string, string>());
			res = client.GetAccount(storage_url, auth_token, new Dictionary<string, string>(), query, false);
            Assert.IsTrue(res.Containers.Count == 0);
		}
		[Test]
		[ExpectedException(typeof(ClientException))]
		public void test_delete_container_fail()
		{
			client.DeleteContainer(storage_url, "asdf", "asdf", new Dictionary<string, string>(), new Dictionary<string, string>());
		}
		[Test]
		public void test_get_object()
		{
			string name = prefix + Guid.NewGuid().ToString();
			created_containers.Add(name);
		    client.PutContainer(storage_url, auth_token, name, new Dictionary<string, string>(), new Dictionary<string, string>());
			var encoder = new UTF8Encoding();
			byte[] byte_array = encoder.GetBytes(object_content);
			var stream = new MemoryStream(byte_array);
			client.PutObject(storage_url, auth_token, name, name, stream, new Dictionary<string, string>(), new Dictionary<string, string>());
			var res = client.GetObject(storage_url, auth_token, name, name, new Dictionary<string, string>(), new Dictionary<string, string>());
			byte_array = new byte[object_content.Length];
			res.ObjectData.Read(byte_array, 0, object_content.Length);
			var content = Encoding.UTF8.GetString(byte_array);
			Assert.IsTrue(content == object_content);
			foreach (var header in object_header_keys)
			{
				Assert.IsTrue(res.Headers.ContainsKey(header));
			}
		}
		[Test]
		[ExpectedException(typeof(ClientException))]
		public void test_get_object_fail()
		{
			client.GetObject(storage_url, "asdf", "asdf", "asdf", new Dictionary<string, string>(), new Dictionary<string, string>());
		}
		[Test]
		public void test_head_object()
		{
			string name = prefix + Guid.NewGuid().ToString();
			created_containers.Add(name);
			var obj_cont_pair = new Dictionary<string, string> {{"container", name}, {"object", name}};
		    client.PutContainer(storage_url, auth_token, name, new Dictionary<string, string>(), new Dictionary<string, string>());
			created_objects.Add(obj_cont_pair);
			var encoder = new UTF8Encoding();
			byte[] byte_array = encoder.GetBytes(object_content);
			var stream = new MemoryStream(byte_array);
			client.PutObject(storage_url, auth_token, name, name, stream, new Dictionary<string, string>(), new Dictionary<string, string>());
			var res = client.HeadObject(storage_url, auth_token, name, name, new Dictionary<string, string>(), new Dictionary<string, string>());
			foreach (string header in object_header_keys)
			{
				Assert.IsTrue(res.Headers.ContainsKey(header));
			}
		}
		[Test]
		[ExpectedException(typeof(ClientException))]
		public void test_head_object_fail()
		{
			client.HeadObject(storage_url, "asdf", "asdf", "asdf", new Dictionary<string, string>(), new Dictionary<string, string>());
		}
		[Test]
		public void test_post_object()
		{
			var headers = new Dictionary<string, string> {{object_metadata_prefix + metadata_key, metadata_value}};
		    var name = prefix + Guid.NewGuid().ToString();
			created_containers.Add(name);
			var obj_cont_pair = new Dictionary<string, string> {{"container", name}, {"object", name}};
		    created_objects.Add(obj_cont_pair);
			client.PutContainer(storage_url, auth_token, name, new Dictionary<string, string>(), new Dictionary<string, string>());
			var encoder = new UTF8Encoding();
			byte[] byte_array = encoder.GetBytes(object_content);
			var stream = new MemoryStream(byte_array);
			client.PutObject(storage_url, auth_token, name, name, stream, new Dictionary<string, string>(), new Dictionary<string, string>());
			client.PostObject(storage_url, auth_token, name, name, headers, new Dictionary<string, string>());
			var res = client.HeadObject(storage_url, auth_token, name, name, new Dictionary<string, string>(), new Dictionary<string, string>());
			Assert.IsTrue(res.Headers.ContainsKey(object_metadata_prefix + metadata_key));
			Assert.IsTrue(res.Headers[object_metadata_prefix + metadata_key] == metadata_value);
			client.PostObject(storage_url, auth_token, name, name, new Dictionary<string, string>(), new Dictionary<string, string>());
			res = client.HeadObject(storage_url, auth_token, name, name, new Dictionary<string, string>(), new Dictionary<string, string>());
			Assert.IsFalse(res.Headers.ContainsKey(object_metadata_prefix + metadata_key));
		}
		[Test]
		[ExpectedException(typeof(ClientException))]
		public void test_post_object_fail()
		{
		    client.PostObject(storage_url, "asdf", "asdf", "asdf", new Dictionary<string, string>(), new Dictionary<string, string>());
		}
		[Test]
		public void test_put_object()
		{
			var name = prefix + Guid.NewGuid();
			created_containers.Add(name);
			var obj_cont_pair = new Dictionary<string, string> {{"container", name}, {"object", name}};
		    client.PutContainer(storage_url, auth_token, name, new Dictionary<string, string>(), new Dictionary<string, string>());
			created_objects.Add(obj_cont_pair);
			var encoder = new UTF8Encoding();
			var byte_array = encoder.GetBytes(object_content);
		    var byte_array_length_before_put = byte_array.Length;
			var stream = new MemoryStream(byte_array);
			client.PutObject(storage_url, auth_token, name, name, stream, new Dictionary<string, string>(), new Dictionary<string, string>());

			var res = client.GetObject(storage_url, auth_token, name, name, new Dictionary<string, string>(), new Dictionary<string, string>());
            byte_array = ReadFully(res.ObjectData);
            long byte_array_length_after_get = byte_array.Length;
            
            Assert.That(byte_array_length_after_get, Is.EqualTo(byte_array_length_before_put));

			res.ObjectData.Read(byte_array, 0, object_content.Length);
			var content = Encoding.UTF8.GetString(byte_array);
			Assert.That(content, Is.EqualTo(object_content));
			foreach (var header in object_header_keys)
			{
				Assert.IsTrue(res.Headers.ContainsKey(header));
			}
		}

        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

		[Test]
		[ExpectedException(typeof(ClientException))]
		public void test_put_object_fail()
		{
			client.PutObject(storage_url, "asdf", "asdf", "asdf", new MemoryStream(), new Dictionary<string, string>(), new Dictionary<string, string>());
		}
		[Test]
		public void test_delete_object()
		{
			var name = prefix + Guid.NewGuid().ToString();
			created_containers.Add(name);
			var obj_cont_pair = new Dictionary<string, string> {{"container", name}, {"object", name}};
		    client.PutContainer(storage_url, auth_token, name, new Dictionary<string, string>(), new Dictionary<string, string>());
			created_objects.Add(obj_cont_pair);
			var encoder = new UTF8Encoding();
			byte[] byte_array = encoder.GetBytes(object_content);
			var stream = new MemoryStream(byte_array);
			client.PutObject(storage_url, auth_token, name, name, stream, new Dictionary<string, string>(), new Dictionary<string, string>());
			var res = client.GetContainer(storage_url, auth_token, name, new Dictionary<string, string>(), new Dictionary<string, string>(), false);
			Assert.IsTrue(res.Objects.Count == 1);
			client.DeleteObject(storage_url, auth_token, name, name, new Dictionary<string, string>(), new Dictionary<string, string>());
			res = client.GetContainer(storage_url, auth_token, name, new Dictionary<string, string>(), new Dictionary<string, string>(), false);
			Assert.IsTrue(res.Objects.Count == 0);
			
		}
		[Test]
		[ExpectedException(typeof(ClientException))]
		public void test_delete_object_fail()
		{
			client.DeleteObject(storage_url, "asdf", "asdf", "asdf", new Dictionary<string, string>(), new Dictionary<string, string>());
		}
	}
}
