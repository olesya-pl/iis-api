using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.ApiTests 
{
	public class Authentification : Environment
	{
        protected string _authToken = null;

		[SetUp]
		public void Authenticate()
		{
			var result = Login("olya", "hammer69", @"http://dev.contour.net:5000");
			Assert.IsTrue(result.state, "Authentication failed!");
			Assert.IsNotEmpty(result.token, "Token is not received");
			_authToken = result.token;
        }
	}
}
