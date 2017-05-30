﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System.Collections;
using NUnit.Framework;

namespace NHibernate.Test.IdTest
{
	using System.Threading.Tasks;
	using System.Threading;
	[TestFixture]
	[Ignore("Not supported yet")]
	public class MultipleHiLoPerTableGeneratorTestAsync : TestCase
	{
		protected override string MappingsAssembly
		{
			get { return "NHibernate.Test"; }
		}

		protected override IList Mappings
		{
			get { return new string[] { "IdTest.Car.hbm.xml", "IdTest.Plane.hbm.xml", "IdTest.Radio.hbm.xml" }; }
		}

		public async Task DistinctIdAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			ISession s = OpenSession();
			ITransaction tx = s.BeginTransaction();
			const int testLength = 8;
			Car[] cars = new Car[testLength];
			Plane[] planes = new Plane[testLength];
			for (int i = 0; i < 8; i++)
			{
				cars[i] = new Car();
				cars[i].Color="Color" + i;
				planes[i] = new Plane();
				planes[i].NbrOfSeats=i;
				await (s.PersistAsync(cars[i], cancellationToken));
			}
			await (tx.CommitAsync(cancellationToken));
			s.Close();
			for (int i = 0; i < testLength; i++)
			{
				Assert.AreEqual(i + 1, cars[i].Id);
			}

			s = OpenSession();
			tx = s.BeginTransaction();
			await (s.DeleteAsync("from Car", cancellationToken));
			await (tx.CommitAsync(cancellationToken));
			s.Close();
		}

		public async Task RollingBackAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			ISession s = OpenSession();
			ITransaction tx = s.BeginTransaction();
			const int testLength = 3;
			long lastId = 0;
			Car car;
			for (int i = 0; i < testLength; i++)
			{
				car = new Car();
				car.Color="color " + i;
				await (s.SaveAsync(car, cancellationToken));
				lastId = car.Id;
			}
			tx.Rollback();
			s.Close();

			s = OpenSession();
			tx = s.BeginTransaction();
			car = new Car();
			car.Color="blue";
			await (s.SaveAsync(car, cancellationToken));
			await (s.FlushAsync(cancellationToken));
			await (tx.CommitAsync(cancellationToken));
			s.Close();

			Assert.AreEqual(lastId + 1, car.Id, "id generation was rolled back");

			s = OpenSession();
			tx = s.BeginTransaction();
			await (s.DeleteAsync("from Car", cancellationToken));
			await (tx.CommitAsync(cancellationToken));
			s.Close();
		}

		public async Task AllParamsAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			ISession s = OpenSession();
			ITransaction tx = s.BeginTransaction();
			Radio radio = new Radio();
			radio.Frequency="32 MHz";
			await (s.PersistAsync(radio, cancellationToken));
			Assert.AreEqual(1, radio.Id);
			radio = new Radio();
			radio.Frequency="32 MHz";
			await (s.PersistAsync(radio, cancellationToken));
			Assert.AreEqual(2, radio.Id);
			await (tx.CommitAsync(cancellationToken));
			s.Close();

			s = OpenSession();
			tx = s.BeginTransaction();
			await (s.DeleteAsync("from Radio", cancellationToken));
			await (tx.CommitAsync(cancellationToken));
			s.Close();			
		}
	}
}