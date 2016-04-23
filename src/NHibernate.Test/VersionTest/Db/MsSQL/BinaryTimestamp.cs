using System.Data;
using System.Threading.Tasks;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using NHibernate.Util;

namespace NHibernate.Test.VersionTest.Db.MsSQL
{
	public class BinaryTimestamp : IUserVersionType
	{
		#region IUserVersionType Members

		public object Next(object current, ISessionImplementor session)
		{
			return current;
		}

		public object Seed(ISessionImplementor session)
		{
			return new byte[8];
		}

		public object Assemble(object cached, object owner)
		{
			return DeepCopy(cached);
		}

		public object DeepCopy(object value)
		{
			return value;
		}

		public object Disassemble(object value)
		{
			return DeepCopy(value);
		}

		public int GetHashCode(object x)
		{
			return x.GetHashCode();
		}

		public bool IsMutable
		{
			get { return false; }
		}

		public Task<object> NullSafeGet(IDataReader rs, string[] names, object owner)
		{
			return System.Threading.Tasks.Task.FromResult(rs.GetValue(rs.GetOrdinal(names[0])));
		}

		public System.Threading.Tasks.Task NullSafeSet(IDbCommand cmd, object value, int index)
		{
			NHibernateUtil.Binary.NullSafeSet(cmd, value, index);
			return TaskHelper.CompletedTask;
		}

		public object Replace(object original, object target, object owner)
		{
			return original;
		}

		public System.Type ReturnedType
		{
			get { return typeof (byte[]); }
		}

		public SqlType[] SqlTypes
		{
			get { return new[] {new SqlType(DbType.Binary, 8)}; }
		}

		public int Compare(object x, object y)
		{
			var xbytes = (byte[]) x;
			var ybytes = (byte[]) y;
			return CompareValues(xbytes, ybytes);
		}

		bool IUserType.Equals(object x, object y)
		{
			return (x == y);
		}

		#endregion

		private static int CompareValues(byte[] x, byte[] y)
		{
			if (x.Length < y.Length)
			{
				return -1;
			}
			if (x.Length > y.Length)
			{
				return 1;
			}
			for (int i = 0; i < x.Length; i++)
			{
				if (x[i] < y[i])
				{
					return -1;
				}
				if (x[i] > y[i])
				{
					return 1;
				}
			}
			return 0;
		}

		public static bool Equals(byte[] x, byte[] y)
		{
			return CompareValues(x, y) == 0;
		}
	}
}