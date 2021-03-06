﻿/*
Copyright 2016 ParticleNET

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

	http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
 */
using System;
using System.Linq;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using Particle;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;

namespace ParticleSDKTests
{
	[TestFixture]
	public class ParticleDeviceTests
	{
#if DEBUG
		[Test]
		public void ParseExceptionTest()
		{
			var p = new ParticleDeviceMock(new JObject());

			var obj = JObject.Parse(@"{'id':'3a', 'name':null}");
			p.ParseObjectMock(obj);
			Assert.AreEqual("3a", p.Id);
			Assert.AreEqual(null, p.Name);

			var json = @"{'id': '356a',
	'name': 'Work',
	'last_app': 'cheese',
	'last_ip_address': '192.168.0.1',
	'last_heard': '2015-05-25T01:15:36.034Z',
	'product_id': 0,
	'connected': true,
	'cellular': false,
	'status': 'normal',
	'variables':{
		temp: 'double',
		temp2: 'int',
		temp3: 'string'
	},
	'NETExceptionTest': 0,
	'functions':[
		'led',
		'led2'
	]}";


			obj = JObject.Parse(json);
			var ex = Assert.Throws<ParticleParseException>(() => p.ParseObjectMock(obj));
			Assert.AreEqual("Error parsing.", ex.Message);
			Assert.IsFalse(String.IsNullOrWhiteSpace(ex.SourceJson));
			Assert.IsNotNull(ex.InnerException);
			Assert.IsInstanceOf<JsonException>(ex.InnerException);
		}
#endif

		[Test]
		public void ParseTest()
		{
			var p = new ParticleDeviceMock(new JObject());
			var message = Assert.Throws<ArgumentNullException>(() => { p.ParseObjectMock(null); });
			Assert.AreEqual("obj", message.ParamName);

			var obj = JObject.Parse(@"{'id':'3a', 'name':null}");
			p.ParseObjectMock(obj);
			Assert.AreEqual("3a", p.Id);
			Assert.AreEqual(null, p.Name);

			obj = JObject.Parse(@"{'id': '356a',
	'name': 'Work',
	'last_app': 'cheese',
	'last_ip_address': '192.168.0.1',
	'last_heard': '2015-05-25T01:15:36.034Z',
	'product_id': 0,
	'connected': true,
	'cellular': false,
	'status': 'normal',
	'variables':{
		temp: 'double',
		temp2: 'int',
		temp3: 'string'
	},
	'functions':[
		'led',
		'led2'
	]}");
			p.ParseObjectMock(obj);
			Assert.AreEqual("356a", p.Id);
			Assert.AreEqual("Work", p.Name);
			Assert.AreEqual("cheese", p.LastApp);
			Assert.AreEqual("192.168.0.1", p.LastIPAddress);
			Assert.AreEqual(JToken.Parse("'2015-05-25T01:15:36.034Z'").Value<DateTime>().ToLocalTime(), p.LastHeard);
			Assert.AreEqual(ParticleDeviceType.Core, p.DeviceType);
			Assert.IsTrue(p.Connected);
			Assert.IsNull(p.PlatformId);
			Assert.IsFalse(p.Cellular);
			Assert.AreEqual("normal", p.Status);
			Assert.IsNull(p.LastICCID);
			Assert.IsNull(p.IMEI);
			Assert.IsNull(p.CurrentBuildTarget);

			Assert.AreEqual(3, p.Variables.Count);
			var variable = p.Variables[0];
			Assert.AreEqual("temp", variable.Name);
			Assert.AreEqual(VariableType.Double, variable.Type);
			variable = p.Variables[1];
			Assert.AreEqual("temp2", variable.Name);
			Assert.AreEqual(VariableType.Int, variable.Type);
			variable = p.Variables[2];
			Assert.AreEqual("temp3", variable.Name);
			Assert.AreEqual(VariableType.String, variable.Type);

			var functions = p.Functions;
			Assert.AreEqual(2, functions.Count);
			Assert.AreEqual("led", functions[0]);
			Assert.AreEqual("led2", functions[1]);
		}

		[Test]
		public void ParseElectronTest()
		{
			var p = new ParticleDeviceMock(new JObject());
			var message = Assert.Throws<ArgumentNullException>(() => { p.ParseObjectMock(null); });
			Assert.AreEqual("obj", message.ParamName);

			var obj = JObject.Parse(@"{'id':'3a', 'name':null}");
			p.ParseObjectMock(obj);
			Assert.AreEqual("3a", p.Id);
			Assert.AreEqual(null, p.Name);

			obj = JObject.Parse(@"{'id': '356a6',
	'name': 'CobbleFriend',
	'last_app': 'cheese',
	'last_ip_address': '161.20.133.22:45478',
	'last_heard': '2015-05-25T01:15:36.034Z',
	'product_id': 10,
	'connected': true,
	'platform_id': 10,
	'cellular': true,
	'status': 'normal',
	'last_iccid': '1000023400005678900',
	'imei': '312345678933111',
	'current_build_target': '0.4.8',
	'variables':{
		temp: 'double',
		temp2: 'int',
		temp3: 'string'
	},
	'functions':[
		'led',
		'led2'
	]}");
			p.ParseObjectMock(obj);
			Assert.AreEqual("356a6", p.Id);
			Assert.AreEqual("CobbleFriend", p.Name);
			Assert.AreEqual("cheese", p.LastApp);
			Assert.AreEqual("161.20.133.22:45478", p.LastIPAddress);
			Assert.AreEqual(JToken.Parse("'2015-05-25T01:15:36.034Z'").Value<DateTime>().ToLocalTime(), p.LastHeard);
			Assert.AreEqual(ParticleDeviceType.Electron, p.DeviceType);
			Assert.AreEqual(10, p.PlatformId);
			Assert.IsTrue(p.Connected);
			Assert.IsTrue(p.Cellular);
			Assert.AreEqual("normal", p.Status);
			Assert.AreEqual("1000023400005678900", p.LastICCID);
			Assert.AreEqual("312345678933111", p.IMEI);
			Assert.AreEqual("0.4.8", p.CurrentBuildTarget);

			Assert.AreEqual(3, p.Variables.Count);
			var variable = p.Variables[0];
			Assert.AreEqual("temp", variable.Name);
			Assert.AreEqual(VariableType.Double, variable.Type);
			variable = p.Variables[1];
			Assert.AreEqual("temp2", variable.Name);
			Assert.AreEqual(VariableType.Int, variable.Type);
			variable = p.Variables[2];
			Assert.AreEqual("temp3", variable.Name);
			Assert.AreEqual(VariableType.String, variable.Type);

			var functions = p.Functions;
			Assert.AreEqual(2, functions.Count);
			Assert.AreEqual("led", functions[0]);
			Assert.AreEqual("led2", functions[1]);
		}

		[Test]
		public async Task RefreshAsyncTest()
		{
			ParticleCloudMock cloud = new ParticleCloudMock();
			cloud.RequestCallBack = (a, b, c) =>
			{
				Assert.AreEqual("GET", a);
				Assert.AreEqual("devices/3", b);
				return new RequestResponse
				{
					StatusCode = System.Net.HttpStatusCode.OK,
					Response = JToken.Parse(@"{'id': '3',
	'name': 'Work',
	'last_app': 'cheese',
	'last_ip_address': '192.168.0.1',
	'last_heard': '2015-05-25T01:15:36.034Z',
	'product_id': 0,
	'connected': true,
	'variables':{
		temp: 'double',
		temp2: 'int',
		temp3: 'string'
	},
	'functions':[
		'led',
		'led2'
	]}")
				};
			};

			var p = new ParticleDeviceMock(cloud, JObject.Parse("{'id':'3', 'name': 'test'}"));
			var result = await p.RefreshAsync();
			Assert.IsTrue(result.Success);
			Assert.AreEqual("3", p.Id);
			Assert.AreEqual("Work", p.Name);
			Assert.AreEqual("cheese", p.LastApp);
			Assert.AreEqual("192.168.0.1", p.LastIPAddress);
			Assert.AreEqual(JToken.Parse("'2015-05-25T01:15:36.034Z'").Value<DateTime>().ToLocalTime(), p.LastHeard);
			Assert.AreEqual(ParticleDeviceType.Core, p.DeviceType);
			Assert.IsTrue(p.Connected);

			Assert.AreEqual(3, p.Variables.Count);
			var variable = p.Variables[0];
			Assert.AreEqual("temp", variable.Name);
			Assert.AreEqual(VariableType.Double, variable.Type);
			variable = p.Variables[1];
			Assert.AreEqual("temp2", variable.Name);
			Assert.AreEqual(VariableType.Int, variable.Type);
			variable = p.Variables[2];
			Assert.AreEqual("temp3", variable.Name);
			Assert.AreEqual(VariableType.String, variable.Type);

			var functions = p.Functions;
			Assert.AreEqual(2, functions.Count);
			Assert.AreEqual("led", functions[0]);
			Assert.AreEqual("led2", functions[1]);
		}

		[Test]
		public async Task RefreshAsyncHttpRequestExceptionTest()
		{
			using (ParticleCloudMock cloud = new ParticleCloudMock())
			{
				var ex = new HttpRequestException("Unable to resolve dns");
				cloud.RequestCallBack = (a, b, c) =>
				{
					throw ex;
				};

				var p = new ParticleDeviceMock(cloud, JObject.Parse("{'id':'3', 'name': 'test'}"));
				var result = await p.RefreshAsync();
				Assert.IsNotNull(result);
				Assert.IsFalse(result.Success);
				Assert.AreEqual(ex.Message, result.Error);
				Assert.AreEqual(ex, result.Exception);
			}
		}

		[Test]
		public async Task GetVariableValueAsyncTest()
		{
			ParticleCloudMock cloud = new ParticleCloudMock();
			cloud.RequestCallBack = (a, b, c) =>
			{
				Assert.AreEqual("GET", a);
				Assert.AreEqual("devices/3/temp", b);
				return new RequestResponse
				{
					StatusCode = System.Net.HttpStatusCode.OK,
					Response = JToken.Parse(@"{'name': 'temp',
  'result': 300,
  'coreInfo': {
						'name': 'weatherman',
						'deviceID': '0123456789abcdef01234567',
						'connected': true,
						'last_handshake_at': '2015-07-17T22:28:40.907Z',
						'last_app': ''
					}}")
				};
			};

			var p = new ParticleDeviceMock(cloud, JObject.Parse("{'id':'3', 'name': 'test', 'variables':{'temp':'int'}}"));

			var ex = Assert.Throws<ArgumentNullException>(() => { p.GetVariableValueAsync((String)null).GetAwaiter().GetResult(); });
			Assert.AreEqual("variable", ex.ParamName);
			ex = Assert.Throws<ArgumentNullException>(() => { p.GetVariableValueAsync((Variable)null).GetAwaiter().GetResult(); });
			Assert.AreEqual("variable", ex.ParamName);

			var results = await p.GetVariableValueAsync("temp");
			Assert.IsTrue(results.Success);
			Assert.IsNotNull(results.Data);
			var variable = results.Data;
			Assert.AreEqual("temp", variable.Name);
			Assert.AreEqual("300", variable.Value);

			cloud.RequestCallBack = (a, b, c) =>
			{
				Assert.AreEqual("GET", a);
				Assert.AreEqual("devices/3/temp", b);
				return new RequestResponse
				{
					StatusCode = System.Net.HttpStatusCode.OK,
					Response = JToken.Parse(@"{'name': 'temp',
  'result': 23,
  'coreInfo': {
						'name': 'weatherman',
						'deviceID': '0123456789abcdef01234567',
						'connected': true,
						'last_handshake_at': '2015-07-17T22:28:40.907Z',
						'last_app': ''
					}}")
				};
			};

			results = await p.GetVariableValueAsync(variable);
			Assert.IsTrue(results.Success);
			Assert.IsNotNull(results.Data);
			variable = results.Data;
			Assert.AreEqual("temp", variable.Name);
			Assert.AreEqual("23", variable.Value);
		}


		[Test]
		public async Task GetVariableValueAsyncHttpRequestExceptionTest()
		{
			using (ParticleCloudMock cloud = new ParticleCloudMock())
			{
				var ex = new HttpRequestException("Unable to resolve");
				cloud.RequestCallBack = (a, b, c) =>
				{
					throw ex;
				};

				var p = new ParticleDeviceMock(cloud, JObject.Parse("{'id':'3', 'name': 'test', 'variables':{'temp':'int'}}"));
				Assert.IsNotNull(p);
				var vv = await p.GetVariableValueAsync("Test");
				Assert.IsNotNull(vv);
				Assert.IsFalse(vv.Success);
				Assert.AreEqual(ex.Message, vv.Error);
				Assert.AreEqual(ex, vv.Exception);
			}
		}

		[Test]
		public async Task CallFunctionAsyncTest()
		{
			using (ParticleCloudMock cloud = new ParticleCloudMock())
			{
				cloud.RequestCallBack = (a, b, c) =>
				{
					Assert.AreEqual("POST", a);
					Assert.AreEqual("devices/3/led", b);
					Assert.AreEqual(1, c.Count());
					var first = c.FirstOrDefault();
					Assert.IsNotNull(first);
					Assert.AreEqual("arg", first.Key);
					Assert.AreEqual("on", first.Value);
					return new RequestResponse
					{
						StatusCode = System.Net.HttpStatusCode.OK,
						Response = JToken.Parse(@"{
  'id': '3',
  'name': 'led',
  'last_app': '',
  'connected': true,
  'return_value': 1
}")
					};
				};

				var p = new ParticleDeviceMock(cloud, JObject.Parse("{'id':'3', 'name': 'test', 'functions':['led']}"));
				var exc = Assert.Throws<ArgumentNullException>(() => { p.CallFunctionAsync(null, "").GetAwaiter().GetResult(); });
				Assert.AreEqual("functionName", exc.ParamName);
				var result = await p.CallFunctionAsync("led", "on");
				Assert.IsTrue(result.Success);
				Assert.AreEqual(1, result.Data);
			}
		}

		[Test]
		public async Task CallFunctionAsyncHttpRequestExceptionTest()
		{
			using (ParticleCloudMock cloud = new ParticleCloudMock())
			{
				var ex = new HttpRequestException("Unable to Resolve");
				cloud.RequestCallBack = (a, b, c) =>
				{
					throw ex;
				};
				var p = new ParticleDeviceMock(cloud, JObject.Parse("{'id':'3', 'name': 'test', 'functions':['led']}"));
				Assert.IsNotNull(p);
				var cf = await p.CallFunctionAsync("LED", "cheese");
				Assert.IsNotNull(cf);
				Assert.AreEqual(ex.Message, cf.Error);
				Assert.AreEqual(ex, cf.Exception);
			}
		}

		[Test]
		public async Task UnclaimAsyncTest()
		{
			using (ParticleCloudMock cloud = new ParticleCloudMock())
			{
				cloud.RequestCallBack = (a, b, c) =>
				{
					Assert.AreEqual("DELETE", a);
					Assert.AreEqual("devices/3", b);
					return new RequestResponse
					{
						StatusCode = System.Net.HttpStatusCode.Forbidden,
						Response = JToken.Parse(@"{
		  'error': 'Permission Denied',
		  'info': 'I didn\'t recognize that device name or ID, try opening https://api.particle.io/v1/devices?access_token=...'
		}")
					};
				};

				var p = new ParticleDeviceMock(cloud, JObject.Parse("{'id':'3', 'name': 'test'}"));

				var result = await p.UnclaimAsync();
				Assert.IsNotNull(result);
				Assert.IsFalse(result.Success);
				Assert.AreEqual("Permission Denied", result.Error);
				Assert.AreEqual("I didn\'t recognize that device name or ID, try opening https://api.particle.io/v1/devices?access_token=...", result.Message);

				cloud.RequestCallBack = (a, b, c) =>
				{
					return new RequestResponse
					{
						StatusCode = System.Net.HttpStatusCode.OK,
						Response = JToken.Parse(@"{
	  'ok': true
	}")
					};
				};

				result = await p.UnclaimAsync();
				Assert.IsNotNull(result);
				Assert.IsTrue(result.Success);
			}
		}


		[Test]
		public async Task UnclaimAsyncHttpRequestExceptionTest()
		{
			using (ParticleCloudMock cloud = new ParticleCloudMock())
			{
				var ex = new HttpRequestException("Unable to resolve");
				cloud.RequestCallBack = (a, b, c) =>
				{
					throw ex;
				};

				var p = new ParticleDeviceMock(cloud, JObject.Parse("{'id':'3', 'name': 'test'}"));

				var result = await p.UnclaimAsync();
				Assert.IsNotNull(result);
				Assert.IsFalse(result.Success);
				Assert.AreEqual(ex.Message, result.Error);
				Assert.AreEqual(ex, result.Exception);
			}
		}

		[Test]
		public async Task RenameAsyncTest()
		{
			using (ParticleCloudMock cloud = new ParticleCloudMock())
			{
				cloud.RequestCallBack = (a, b, c) =>
				{
					Assert.AreEqual("PUT", a);
					Assert.AreEqual("devices/3", b);
					Assert.AreEqual(1, c.Length);
					var p1 = c[0];
					Assert.AreEqual("name", p1.Key);
					Assert.AreEqual("newTest", p1.Value);
					return new RequestResponse
					{
						StatusCode = System.Net.HttpStatusCode.OK,
						Response = JToken.Parse(@"{
		  'error': 'Nothing to do?'
		}")
					};
				};

				var p = new ParticleDeviceMock(cloud, JObject.Parse("{'id':'3', 'name': 'test'}"));

				var exc = Assert.Throws<ArgumentNullException>(() => { p.RenameAsync(null).GetAwaiter().GetResult(); });
				Assert.AreEqual("newName", exc.ParamName);

				var result = await p.RenameAsync("newTest");
				Assert.IsNotNull(result);
				Assert.IsFalse(result.Success);
				Assert.AreEqual("Nothing to do?", result.Error);

				cloud.RequestCallBack = (a, b, c) =>
				{
					Assert.AreEqual("PUT", a);
					Assert.AreEqual("devices/3", b);
					Assert.AreEqual(1, c.Length);
					var p1 = c[0];
					Assert.AreEqual("name", p1.Key);
					Assert.AreEqual("newTest", p1.Value);
					return new RequestResponse
					{
						StatusCode = System.Net.HttpStatusCode.OK,
						Response = JToken.Parse(@"{
name: 'newTest',
id: '1234'
		}")
					};
				};

				result = await p.RenameAsync("newTest");
				Assert.IsNotNull(result);
				Assert.IsTrue(result.Success);
			}
		}

		[Test]
		public async Task RenameAsyncHttpRequestExceptionTest()
		{
			using (ParticleCloudMock cloud = new ParticleCloudMock())
			{
				var ex = new HttpRequestException("Unable to resolve");
				cloud.RequestCallBack = (a, b, c) =>
				{
					throw ex;
				};

				var p = new ParticleDeviceMock(cloud, JObject.Parse("{'id':'3', 'name': 'test'}"));
				var result = await p.RenameAsync("cheese");
				Assert.IsNotNull(result);
				Assert.IsFalse(result.Success);
				Assert.AreEqual(ex.Message, result.Error);
				Assert.AreEqual(ex, result.Exception);
			}
		}

		[Test]
		public async Task FlashKnownAppAsyncTest()
		{
			using (ParticleCloudMock cloud = new ParticleCloudMock())
			{
				cloud.RequestCallBack = (a, b, c) =>
				{
					Assert.AreEqual("PUT", a);
					Assert.AreEqual("devices/3", b);
					Assert.AreEqual(1, c.Length);
					var p1 = c[0];
					Assert.AreEqual("app", p1.Key);
					Assert.AreEqual("newTest", p1.Value);
					return new RequestResponse
					{
						StatusCode = System.Net.HttpStatusCode.OK,
						Response = JToken.Parse(@"{
			  'ok': false,
			  'code': 500,
			  'errors': [
				'Can\'t flash unknown app newTest'
			  ]
					}")
					};
				};

				var p = new ParticleDeviceMock(cloud, JObject.Parse("{'id':'3', 'name': 'test'}"));

				var exc = Assert.Throws<ArgumentNullException>(() => { p.FlashKnownAppAsync(null).GetAwaiter().GetResult(); });
				Assert.AreEqual("appName", exc.ParamName);

				var result = await p.FlashKnownAppAsync("newTest");
				Assert.IsNotNull(result);
				Assert.IsFalse(result.Success);
				Assert.AreEqual("Can't flash unknown app newTest", result.Error);

				cloud.RequestCallBack = (a, b, c) =>
				{
					Assert.AreEqual("PUT", a);
					Assert.AreEqual("devices/3", b);
					Assert.AreEqual(1, c.Length);
					var p1 = c[0];
					Assert.AreEqual("app", p1.Key);
					Assert.AreEqual("tinker", p1.Value);
					return new RequestResponse
					{
						StatusCode = System.Net.HttpStatusCode.OK,
						Response = JToken.Parse(@"{
  'id': '3',
  'status': 'Update started'
}")
					};
				};

				result = await p.FlashKnownAppAsync("tinker");
				Assert.IsNotNull(result);
				Assert.IsTrue(result.Success);
				Assert.AreEqual("Update started", result.Message);
			}
		}

		[Test]
		public async Task FlashExampleAppAsyncTest()
		{
			using (ParticleCloudMock cloud = new ParticleCloudMock())
			{
				cloud.RequestCallBack = (a, b, c) =>
				{
					Assert.AreEqual("PUT", a);
					Assert.AreEqual("devices/3", b);
					Assert.AreEqual(1, c.Length);
					var p1 = c[0];
					Assert.AreEqual("app_example_id", p1.Key);
					Assert.AreEqual("56214d636666d9ece3000001", p1.Value);
					return new RequestResponse
					{
						StatusCode = System.Net.HttpStatusCode.OK,
						Response = JToken.Parse(@"{
			  'ok': false,
			  'code': 500,
			  'errors': [
				'Can\'t flash unknown app newTest'
			  ]
					}")
					};
				};

				var p = new ParticleDeviceMock(cloud, JObject.Parse("{'id':'3', 'name': 'test'}"));

				var exc = Assert.Throws<ArgumentNullException>(() => { p.FlashExampleAppAsync(null).GetAwaiter().GetResult(); });
				Assert.AreEqual("exampleId", exc.ParamName);

				var result = await p.FlashExampleAppAsync("56214d636666d9ece3000001");
				Assert.IsNotNull(result);
				Assert.IsFalse(result.Success);
				Assert.AreEqual("Can't flash unknown app newTest", result.Error);

				cloud.RequestCallBack = (a, b, c) =>
				{
					Assert.AreEqual("PUT", a);
					Assert.AreEqual("devices/3", b);
					Assert.AreEqual(1, c.Length);
					var p1 = c[0];
					Assert.AreEqual("app_example_id", p1.Key);
					Assert.AreEqual("56214d636666d9ece3000006", p1.Value);
					return new RequestResponse
					{
						StatusCode = System.Net.HttpStatusCode.OK,
						Response = JToken.Parse(@"{
  'id': '3',
  'status': 'Update started'
}")
					};
				};

				result = await p.FlashExampleAppAsync("56214d636666d9ece3000006");
				Assert.IsNotNull(result);
				Assert.IsTrue(result.Success);
				Assert.AreEqual("Update started", result.Message);
			}
		}

		[Test]
		public async Task FlashKnownAppAsyncHttpRequestExceptionTest()
		{
			using (ParticleCloudMock cloud = new ParticleCloudMock())
			{
				var ex = new HttpRequestException("Unable to resolve");
				cloud.RequestCallBack = (a, b, c) =>
				{
					throw ex;
				};

				var p = new ParticleDeviceMock(cloud, JObject.Parse("{'id':'3', 'name': 'test'}"));
				var result = await p.FlashKnownAppAsync("tinker");
				Assert.IsNotNull(result);
				Assert.IsFalse(result.Success);
				Assert.AreEqual(ex.Message, result.Error);
				Assert.AreEqual(ex, result.Exception);
			}
		}

		[Test]
		public void LastHeardDateTest()
		{
			using (var cloud = new ParticleCloud())
			{
				var expected = DateTime.Now;
				var p = new ParticleDeviceMock(cloud, JObject.Parse($"{{'id':'3', 'name': 'test','last_heard': '{expected.ToUniversalTime():o}'}}"));

				Assert.AreEqual(expected, p.LastHeard);
			}
		}
	}
}
