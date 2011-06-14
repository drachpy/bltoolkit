﻿using System.Linq;

using BLToolkit.Data.DataProvider;

using NUnit.Framework;

namespace Data.Linq
{
	using Model;

	[TestFixture]
	public class JoinTest : TestBase
	{
		[Test]
		public void InnerJoin1()
		{
			TestJohn(db =>
				from p1 in db.Person
					join p2 in db.Person on p1.ID equals p2.ID
				where p1.ID == 1
				select new Person { ID = p1.ID, FirstName = p2.FirstName });
		}

		[Test]
		public void InnerJoin2()
		{
			TestJohn(db =>
				from p1 in db.Person
					join p2 in db.Person on new { p1.ID, p1.FirstName } equals new { p2.ID, p2.FirstName }
				where p1.ID == 1
				select new Person { ID = p1.ID, FirstName = p2.FirstName });
		}

		[Test]
		public void InnerJoin3()
		{
			TestJohn(db =>
				from p1 in db.Person
					join p2 in
						from p2 in db.Person join p3 in db.Person on new { p2.ID, p2.LastName } equals new { p3.ID, p3.LastName } select new { p2, p3 }
					on new { p1.ID, p1.FirstName } equals new { p2.p2.ID, p2.p2.FirstName }
				where p1.ID == 1
				select new Person { ID = p1.ID, FirstName = p2.p2.FirstName, LastName = p2.p3.LastName });
		}

		[Test]
		public void InnerJoin4()
		{
			TestJohn(db =>
				from p1 in db.Person
					join p2 in db.Person on new { p1.ID, p1.FirstName } equals new { p2.ID, p2.FirstName }
						join p3 in db.Person on new { p2.ID, p2.LastName } equals new { p3.ID, p3.LastName }
				where p1.ID == 1
				select new Person { ID = p1.ID, FirstName = p2.FirstName, LastName = p3.LastName });
		}

		[Test]
		public void InnerJoin5()
		{
			TestJohn(db =>
				from p1 in db.Person
				join p2 in db.Person on new { p1.ID, p1.FirstName } equals new { p2.ID, p2.FirstName }
				join p3 in db.Person on new { p1.ID, p2.LastName  } equals new { p3.ID, p3.LastName  }
				where p1.ID == 1
				select new Person { ID = p1.ID, FirstName = p2.FirstName, LastName = p3.LastName });
		}

		[Test]
		public void InnerJoin6()
		{
			TestJohn(db =>
				from p1 in db.Person
					join p2 in from p3 in db.Person select new { ID = p3.ID + 1, p3.FirstName } on p1.ID equals p2.ID - 1
				where p1.ID == 1
				select new Person { ID = p1.ID, FirstName = p2.FirstName });
		}

		[Test]
		public void InnerJoin7()
		{
			var expected =
				from t in
					from ch in Child
						join p in Parent on ch.ParentID equals p.ParentID
					select ch.ParentID + p.ParentID
				where t > 2
				select t;

			ForEachProvider(db => AreEqual(expected,
				from t in
					from ch in db.Child
						join p in db.Parent on ch.ParentID equals p.ParentID
					select ch.ParentID + p.ParentID
				where t > 2
				select t));
		}

		[Test]
		public void InnerJoin8()
		{
			ForEachProvider(db => AreEqual(
				from t in
					from ch in Child
						join p in Parent on ch.ParentID equals p.ParentID
					select new { ID = ch.ParentID + p.ParentID }
				where t.ID > 2
				select t,
				from t in
					from ch in db.Child
						join p in db.Parent on ch.ParentID equals p.ParentID
					select new { ID = ch.ParentID + p.ParentID }
				where t.ID > 2
				select t));
		}

		[Test]
		public void InnerJoin9()
		{
			ForEachProvider(new[] { ProviderName.Access }, db => AreEqual(
				from g in GrandChild
				join p in Parent4 on g.Child.ParentID equals p.ParentID
				where g.ParentID < 10 && p.Value1 == TypeValue.Value3
				select g,
				from g in db.GrandChild
				join p in db.Parent4 on g.Child.ParentID equals p.ParentID
				where g.ParentID < 10 && p.Value1 == TypeValue.Value3
				select g));
		}

		[Test]
		public void InnerJoin10()
		{
			ForEachProvider(db => AreEqual(
				from p in    Parent
				join g in    GrandChild on p.ParentID equals g.ParentID into q
				from q1 in q
				select new { p.ParentID, q1.GrandChildID },
				from p in db.Parent
				join g in db.GrandChild on p.ParentID equals g.ParentID into q
				from q1 in q
				select new { p.ParentID, q1.GrandChildID }));
		}

		[Test]
		public void GroupJoin1()
		{
			ForEachProvider(db => AreEqual(
				from p in Parent
					join ch in Child on p.ParentID equals ch.ParentID into lj1
				where p.ParentID == 1
				select p,
				from p in db.Parent
					join ch in db.Child on p.ParentID equals ch.ParentID into lj1
				where p.ParentID == 1
				select p));
		}

		[Test]
		public void GroupJoin2()
		{
			ForEachProvider(db =>
			{
				var q = 
					from p in db.Parent
						join c in db.Child on p.ParentID equals c.ParentID into lj
					where p.ParentID == 1
					select new { p, lj };

				var list = q.ToList();

				Assert.AreEqual(1, list.Count);
				Assert.AreEqual(1, list[0].p.ParentID);
				Assert.AreEqual(1, list[0].lj.Count());

				var ch = list[0].lj.ToList();

				Assert.AreEqual( 1, ch[0].ParentID);
				Assert.AreEqual(11, ch[0].ChildID);
			});
		}

		[Test]
		public void GroupJoin3()
		{
			ForEachProvider(db =>
			{
				var q = db.Parent
					.GroupJoin(
						db.Child,
						p => p.ParentID,
						ch => ch.ParentID,
						(p, lj1) => new { p, lj1 = new { lj1 } }
					)
					.Where (t => t.p.ParentID == 2)
					.Select(t => new { t.p, t.lj1 });

				var list = q.ToList();

				Assert.AreEqual(2, list.Count);
				Assert.AreEqual(2, list[0].p.ParentID);
				Assert.AreEqual(2, list[0].lj1.lj1.Count());
			});
		}

		[Test]
		public void GroupJoin4()
		{
			ForEachProvider(db =>
			{
				var q = 
					from p in db.Parent
						join ch in
							from c in db.Child select new { c.ParentID, c.ChildID }
						on p.ParentID equals ch.ParentID into lj1
					where p.ParentID == 3
					select new { p, lj1 };

				var list = q.ToList();

				Assert.AreEqual(3, list.Count);
				Assert.AreEqual(3, list[0].p.ParentID);
				Assert.AreEqual(3, list[0].lj1.Count());
			});
		}

		[Test]
		public void GroupJoin5()
		{
			ForEachProvider(db => AreEqual(
				from p in Parent
					join ch in Child on p.ParentID equals ch.ParentID into lj1
				where p.ParentID == 1
				select lj1.First(),
				from p in db.Parent
					join ch in db.Child on p.ParentID equals ch.ParentID into lj1
				where p.ParentID == 1
				select lj1.First()));
		}

		[Test]
		public void GroupJoin51()
		{
			var expected =
			(
				from p in Parent
					join ch in Child on p.ParentID equals ch.ParentID into lj1
				where p.ParentID == 1
				select new { p1 = lj1, p2 = lj1.First() }
			).ToList();

			ForEachProvider(db =>
			{
				var result =
				(
					from p in db.Parent
						join ch in db.Child on p.ParentID equals ch.ParentID into lj1
					where p.ParentID == 1
					select new { p1 = lj1, p2 = lj1.First() }
				).ToList();

				Assert.AreEqual(expected.Count, result.Count);
				AreEqual(expected[0].p1, result[0].p1);
			});
		}

		[Test]
		public void GroupJoin52()
		{
			ForEachProvider(db => AreEqual(
				from p in Parent
					join ch in Child on p.ParentID equals ch.ParentID into lj1
				where p.ParentID == 1
				select lj1.First().ParentID,
				from p in db.Parent
					join ch in db.Child on p.ParentID equals ch.ParentID into lj1
				where p.ParentID == 1
				select lj1.First().ParentID));
		}

		[Test]
		public void GroupJoin53()
		{
			ForEachProvider(db => AreEqual(
				from p in Parent
					join ch in Child on p.ParentID equals ch.ParentID into lj1
				where p.ParentID == 1
				select lj1.Select(_ => _.ParentID).First(),
				from p in db.Parent
					join ch in db.Child on p.ParentID equals ch.ParentID into lj1
				where p.ParentID == 1
				select lj1.Select(_ => _.ParentID).First()));
		}

		[Test]
		public void GroupJoin54()
		{
			ForEachProvider(db => AreEqual(
				from p in Parent
					join ch in Child on p.ParentID equals ch.ParentID into lj1
				where p.ParentID == 1
				select new { p1 = lj1.Count(), p2 = lj1.First() },
				from p in db.Parent
					join ch in db.Child on p.ParentID equals ch.ParentID into lj1
				where p.ParentID == 1
				select new { p1 = lj1.Count(), p2 = lj1.First() }));
		}

		[Test]
		public void GroupJoin6()
		{
			var n = 1;

			ForEachProvider(db =>
			{
				var q = 
					from p in db.Parent
						join c in db.Child on p.ParentID + n equals c.ParentID into lj
					where p.ParentID == 1
					select new { p, lj };

				var list = q.ToList();

				Assert.AreEqual(2, list.Count);
				Assert.AreEqual(1, list[0].p.ParentID);
				Assert.AreEqual(2, list[0].lj.Count());

				var ch = list[0].lj.ToList();

				Assert.AreEqual( 2, ch[0].ParentID);
				Assert.AreEqual(21, ch[0].ChildID);
			});
		}

		[Test]
		public void GroupJoin7()
		{
			var n = 1;

			ForEachProvider(
				new[] { ProviderName.Firebird },
				db =>
				{
					var q = 
						from p in db.Parent
							join c in db.Child on new { id = p.ParentID } equals new { id = c.ParentID - n } into j
						where p.ParentID == 1
						select new { p, j };

					var list = q.ToList();

					Assert.AreEqual(2, list.Count);
					Assert.AreEqual(1, list[0].p.ParentID);
					Assert.AreEqual(2, list[0].j.Count());

					var ch = list[0].j.ToList();

					Assert.AreEqual( 2, ch[0].ParentID);
					Assert.AreEqual(21, ch[0].ChildID);
				});
		}

		[Test]
		public void LeftJoin1()
		{
			var expected =
				from p in Parent
					join ch in Child on p.ParentID equals ch.ParentID into lj1
					from ch in lj1.DefaultIfEmpty()
				where p.ParentID >= 4
				select new { p, ch };

			ForEachProvider(db => AreEqual(expected,
				from p in db.Parent
					join ch in db.Child on p.ParentID equals ch.ParentID into lj1
					from ch in lj1.DefaultIfEmpty()
				where p.ParentID >= 4
				select new { p, ch }));
		}

		[Test]
		public void LeftJoin2()
		{
			ForEachProvider(db => AreEqual(
				from p in Parent
					join ch in Child on p.ParentID equals ch.ParentID into lj1
					from ch in lj1.DefaultIfEmpty()
				select new { p, ch },
				from p in db.Parent
					join ch in db.Child on p.ParentID equals ch.ParentID into lj1
					from ch in lj1.DefaultIfEmpty()
				select new { p, ch }));
		}

		[Test]
		public void LeftJoin3()
		{
			ForEachProvider(db => AreEqual(
				from c in    Child select c.Parent,
				from c in db.Child select c.Parent));
		}

		[Test]
		public void SubQueryJoin()
		{
			var expected =
				from p in Parent
					join ch in 
						from c in Child
						where c.ParentID > 0
						select new { c.ParentID, c.ChildID }
					on p.ParentID equals ch.ParentID into lj1
					from ch in lj1.DefaultIfEmpty()
				select p;

			ForEachProvider(db => AreEqual(expected,
				from p in db.Parent
					join ch in 
						from c in db.Child
						where c.ParentID > 0
						select new { c.ParentID, c.ChildID }
					on p.ParentID equals ch.ParentID into lj1
					from ch in lj1.DefaultIfEmpty()
				select p));
		}

		[Test]
		public void ReferenceJoin1()
		{
			ForEachProvider(new[] { ProviderName.Access }, db => AreEqual(
				from c in    Child join g in    GrandChild on c equals g.Child select new { c.ParentID, g.GrandChildID },
				from c in db.Child join g in db.GrandChild on c equals g.Child select new { c.ParentID, g.GrandChildID }));
		}

		[Test]
		public void ReferenceJoin2()
		{
			ForEachProvider(new[] { ProviderName.Access }, db => AreEqual(
				from g in    GrandChild
					join c in    Child on g.Child equals c
				select new { c.ParentID, g.GrandChildID },
				from g in db.GrandChild
					join c in db.Child on g.Child equals c
				select new { c.ParentID, g.GrandChildID }));
		}

		[Test]
		public void JoinByAnonymousTest()
		{
			ForEachProvider(new[] { ProviderName.Access }, db => AreEqual(
				from p in    Parent
				join c in    Child on new { Parent = p, p.ParentID } equals new { c.Parent, c.ParentID }
				select new { p.ParentID, c.ChildID },
				from p in db.Parent
				join c in db.Child on new { Parent = p, p.ParentID } equals new { c.Parent, c.ParentID }
				select new { p.ParentID, c.ChildID }));
		}

		[Test]
		public void FourTableJoin()
		{
			ForEachProvider(db => AreEqual(
				from p in Parent
				join c1 in Child      on p.ParentID  equals c1.ParentID
				join c2 in GrandChild on c1.ParentID equals c2.ParentID
				join c3 in GrandChild on c2.ParentID equals c3.ParentID
				select new { p, c1Key = c1.ChildID, c2Key = c2.GrandChildID, c3Key = c3.GrandChildID },
				from p in db.Parent
				join c1 in db.Child      on p.ParentID  equals c1.ParentID
				join c2 in db.GrandChild on c1.ParentID equals c2.ParentID
				join c3 in db.GrandChild on c2.ParentID equals c3.ParentID
				select new { p, c1Key = c1.ChildID, c2Key = c2.GrandChildID, c3Key = c3.GrandChildID }));
		}

		[Test]
		public void ProjectionTest1()
		{
			ForEachProvider(db => AreEqual(
				from p1 in Person
				join p2 in Person on p1.ID equals p2.ID
				select new { ID1 = new { Value = p1.ID }, FirstName2 = p2.FirstName, } into p1
				select p1.ID1.Value,
				from p1 in db.Person
				join p2 in db.Person on p1.ID equals p2.ID
				select new { ID1 = new { Value = p1.ID }, FirstName2 = p2.FirstName, } into p1
				select p1.ID1.Value));
		}

		[Test]
		public void LeftJoinTest()
		{
			// Reproduces the problem described here: http://rsdn.ru/forum/prj.rfd/4221837.flat.aspx
			ForEachProvider(
				//Providers.Select(p => p.Name).Except(new[] { ProviderName.SQLite }).ToArray(),
				db =>
				{
					var q = 
						from p1 in db.Person
						join p2 in db.Person on p1.ID equals p2.ID into g
						from p2 in g.DefaultIfEmpty() // yes I know the join will always succeed and it'll never be null, but just for test's sake :)
						select new { p1, p2 };

					var list = q.ToList(); // NotImplementedException? :(
					Assert.That(list, Is.Not.Empty);
				});
		}

		[Test]
		public void LeftJoinTest2()
		{
			// THIS TEST MUST BE RUN IN RELEASE CONFIGURATION (BECAUSE IT PASSES UNDER DEBUG CONFIGURATION)
			// Reproduces the problem described here: http://rsdn.ru/forum/prj.rfd/4221837.flat.aspx

			ForEachProvider(
				Providers.Select(p => p.Name).Except(new[] { ProviderName.SQLite }).ToArray(),
				db =>
				{
					var q =
						from p1 in db.Patient
						join p2 in db.Patient on p1.Diagnosis equals p2.Diagnosis into g
						from p2 in g.DefaultIfEmpty() // yes I know the join will always succeed and it'll never be null, but just for test's sake :)
						join p3 in db.Person on p2.PersonID equals p3.ID
						select new { p1, p2, p3 };

					var arr = q.ToArray(); // NotImplementedException? :(
					Assert.That(arr, Is.Not.Empty);
				});
		}

	}
}
