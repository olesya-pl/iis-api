using NUnit.Framework;
using System.Threading.Tasks;

namespace Iis.ApiTests
{
	public class Tests : BaseAPITest
	{
		//[SetUp]
		//public void Setup()
		//{
		//}

		[Test]
		public async Task Test1()
		{
			var result = await Login("olya", "hammer69", @"http://dev.contour.net:5000");

			//Assert.IsTrue(result, )
		}
	}
}