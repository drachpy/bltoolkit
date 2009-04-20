﻿using System;
using System.Text;

namespace BLToolkit.Data.Sql.SqlProvider
{
	using DataProvider;

	public class InformixSqlProvider : BasicSqlProvider
	{
		public InformixSqlProvider(DataProviderBase dataProvider) : base(dataProvider)
		{
		}

		protected override void BuildSelectClause(StringBuilder sb)
		{
			if (SqlBuilder.From.Tables.Count == 0)
			{
				AppendIndent(sb);
				sb.Append("SELECT FIRST 1").AppendLine();
				BuildColumns(sb);
				AppendIndent(sb);
				sb.Append("FROM SYSTABLES").AppendLine();
			}
			else
				base.BuildSelectClause(sb);
		}

		protected override void BuildBinaryExpression(StringBuilder sb, SqlBinaryExpression expr)
		{
			switch (expr.Operation[0])
			{
				case '%': BuildFunction(sb, "MOD",    expr);    break;
				case '&': BuildFunction(sb, "BITAND", expr);    break;
				case '|': BuildFunction(sb, "BITOR",  expr);    break;
				case '^': BuildFunction(sb, "BITXOR", expr);    break;
				default : base.BuildBinaryExpression(sb, expr); break;
			}
		}

		protected override int GetPrecedence(ISqlExpression expr)
		{
			if (expr is SqlBinaryExpression)
			{
				switch (((SqlBinaryExpression)expr).Operation[0])
				{
					case '%':
					case '&':
					case '|':
					case '^': return Precedence.Primary;
				}
			}

			return base.GetPrecedence(expr);
		}
	}
}
