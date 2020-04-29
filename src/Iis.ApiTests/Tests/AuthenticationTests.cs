using NUnit.Framework;
using System.Threading.Tasks;

namespace Iis.ApiTests
{
	public class AuthenticationTests : Environment
	{
		// Login
		[Test]
		public void LoginSuccess()
		{
			var result =  Login("olya", "hammer69", @"http://dev.contour.net:5000");

			Assert.IsTrue(result.state, "Authentication failed!");
			Assert.IsNotEmpty(result.token, "Token is not received");
		}

		// Login
		[Test]
		public void LoginFailed()
		{
			var result = Login("olya1", "hammer69", @"http://dev.contour.net:5000");

			Assert.IsFalse(result.state, "Authentication failed!");
			Assert.AreEqual(result.exceptionMessage, "Wrong username or password");
		}
	}
}