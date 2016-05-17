#if NET_4_5
using System;
using System.Drawing;
using System.Reflection;
using NUnit.Framework;
using NHibernate.Type;
using NHibernate.SqlTypes;
using System.Threading.Tasks;

namespace NHibernate.Test.NHSpecificTest.NH2484
{
	[System.CodeDom.Compiler.GeneratedCode("AsyncGenerator", "1.0.0")]
	public partial class Fixture : BugTestCase
	{
		[Test]
		public async Task TestPersistenceOfClassWithUnknownSerializableTypeAsync()
		{
			Assembly assembly = Assembly.Load(MappingsAssembly);
			var stream = assembly.GetManifestResourceStream("NHibernate.Test.NHSpecificTest.NH2484.food-photo.jpg");
			var image = Bitmap.FromStream(stream);
			var model = new ClassWithImage()
			{Image = image};
			var imageSize = model.Image.Size;
			int id = -1;
			using (ISession session = OpenSession())
			{
				session.SaveOrUpdate(model);
				await (session.FlushAsync());
				id = model.Id;
				Assert.That(id, Is.GreaterThan(-1));
			}

			using (ISession session = OpenSession())
			{
				model = await (session.GetAsync<ClassWithImage>(id));
				Assert.That(model.Image.Size, Is.EqualTo(imageSize)); // Ensure type is not truncated
			}

			using (ISession session = OpenSession())
			{
				session.CreateQuery("delete from ClassWithImage").ExecuteUpdate();
				await (session.FlushAsync());
			}

			stream.Dispose();
		}

		[Test]
		public async Task TestPersistenceOfClassWithSerializableTypeAsync()
		{
			Assembly assembly = Assembly.Load(MappingsAssembly);
			var stream = assembly.GetManifestResourceStream("NHibernate.Test.NHSpecificTest.NH2484.food-photo.jpg");
			var image = Bitmap.FromStream(stream);
			var model = new ClassWithSerializableType()
			{Image = image};
			var imageSize = ((Image)model.Image).Size;
			int id = -1;
			using (ISession session = OpenSession())
			{
				session.SaveOrUpdate(model);
				await (session.FlushAsync());
				id = model.Id;
				Assert.That(id, Is.GreaterThan(-1));
			}

			using (ISession session = OpenSession())
			{
				model = await (session.GetAsync<ClassWithSerializableType>(id));
				Assert.That(((Image)model.Image).Size, Is.EqualTo(imageSize)); // Ensure type is not truncated
			}

			using (ISession session = OpenSession())
			{
				session.CreateQuery("delete from ClassWithSerializableType").ExecuteUpdate();
				await (session.FlushAsync());
			}

			stream.Dispose();
		}
	}
}
#endif
