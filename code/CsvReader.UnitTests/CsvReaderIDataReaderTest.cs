//	LumenWorks.Framework.Tests.Unit.IO.CSV.CsvReaderIDataReaderTest
//	Copyright (c) 2005 Sébastien Lorion
//
//	MIT license (http://en.wikipedia.org/wiki/MIT_License)
//
//	Permission is hereby granted, free of charge, to any person obtaining a copy
//	of this software and associated documentation files (the "Software"), to deal
//	in the Software without restriction, including without limitation the rights 
//	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
//	of the Software, and to permit persons to whom the Software is furnished to do so, 
//	subject to the following conditions:
//
//	The above copyright notice and this permission notice shall be included in all 
//	copies or substantial portions of the Software.http://scottchacon.com/2011/08/31/github-flow.html
//
//	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
//	INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
//	PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE 
//	FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
//	ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace CsvReader.UnitTests
{
	[TestFixture()]
	public class CsvReaderIDataReaderTest
	{
		#region IDataReader interface

		[Test()]
		public void CloseTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleData1), true))
			{
				IDataReader reader = csv;

				csv.ReadNextRecord();

				reader.Close();

				Assert.IsTrue(reader.IsClosed);
				Assert.IsTrue(csv.IsDisposed);
			}
		}

		[Test()]
		public void GetSchemaTableWithHeadersTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleData1), true))
			{
				IDataReader reader = csv;

				DataTable schema = reader.GetSchemaTable();

			    // ReSharper disable once PossibleNullReferenceException
				Assert.AreEqual(CsvReaderSampleData.SampleData1FieldCount, schema.Rows.Count);

				foreach (DataColumn column in schema.Columns)
				{
					Assert.IsTrue(column.ReadOnly);
				}

				for (int index = 0; index < schema.Rows.Count; index++)
				{
					DataRow column = schema.Rows[index];

					Assert.AreEqual(int.MaxValue, column["ColumnSize"]);
					Assert.AreEqual(DBNull.Value, column["NumericPrecision"]);
					Assert.AreEqual(DBNull.Value, column["NumericScale"]);
					Assert.AreEqual(false, column["IsUnique"]);
					Assert.AreEqual(false, column["IsKey"]);
					Assert.AreEqual(string.Empty, column["BaseServerName"]);
					Assert.AreEqual(string.Empty, column["BaseCatalogName"]);
					Assert.AreEqual(string.Empty, column["BaseSchemaName"]);
					Assert.AreEqual(string.Empty, column["BaseTableName"]);
					Assert.AreEqual(typeof(string), column["DataType"]);
					Assert.AreEqual(true, column["AllowDBNull"]);
					Assert.AreEqual((int) DbType.String, column["ProviderType"]);
					Assert.AreEqual(false, column["IsAliased"]);
					Assert.AreEqual(false, column["IsExpression"]);
					Assert.AreEqual(false, column["IsAutoIncrement"]);
					Assert.AreEqual(false, column["IsRowVersion"]);
					Assert.AreEqual(false, column["IsHidden"]);
					Assert.AreEqual(false, column["IsLong"]);
					Assert.AreEqual(true, column["IsReadOnly"]);

					Assert.AreEqual(index, column["ColumnOrdinal"]);

					switch (index)
					{
						case 0:
							Assert.AreEqual(CsvReaderSampleData.SampleData1Header0, column["ColumnName"]);
							Assert.AreEqual(CsvReaderSampleData.SampleData1Header0, column["BaseColumnName"]);
							break;
						case 1:
							Assert.AreEqual(CsvReaderSampleData.SampleData1Header1, column["ColumnName"]);
							Assert.AreEqual(CsvReaderSampleData.SampleData1Header1, column["BaseColumnName"]);
							break;
						case 2:
							Assert.AreEqual(CsvReaderSampleData.SampleData1Header2, column["ColumnName"]);
							Assert.AreEqual(CsvReaderSampleData.SampleData1Header2, column["BaseColumnName"]);
							break;
						case 3:
							Assert.AreEqual(CsvReaderSampleData.SampleData1Header3, column["ColumnName"]);
							Assert.AreEqual(CsvReaderSampleData.SampleData1Header3, column["BaseColumnName"]);
							break;
						case 4:
							Assert.AreEqual(CsvReaderSampleData.SampleData1Header4, column["ColumnName"]);
							Assert.AreEqual(CsvReaderSampleData.SampleData1Header4, column["BaseColumnName"]);
							break;
						case 5:
							Assert.AreEqual(CsvReaderSampleData.SampleData1Header5, column["ColumnName"]);
							Assert.AreEqual(CsvReaderSampleData.SampleData1Header5, column["BaseColumnName"]);
							break;
						case 6:
							Assert.AreEqual(CsvReaderSampleData.SampleData1Header6, column["ColumnName"]);
							Assert.AreEqual(CsvReaderSampleData.SampleData1Header6, column["BaseColumnName"]);
                            break;
						default:
							throw new IndexOutOfRangeException();
					}
				}
			}
		}

		[Test()]
		public void GetSchemaTableWithoutHeadersTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleData1), false))
			{
				IDataReader reader = csv;

				DataTable schema = reader.GetSchemaTable();

			    // ReSharper disable once PossibleNullReferenceException
				Assert.AreEqual(CsvReaderSampleData.SampleData1FieldCount, schema.Rows.Count);

				foreach (DataColumn column in schema.Columns)
				{
					Assert.IsTrue(column.ReadOnly);
				}

				for (int index = 0; index < schema.Rows.Count; index++)
				{
					DataRow column = schema.Rows[index];

					Assert.AreEqual(int.MaxValue, column["ColumnSize"]);
					Assert.AreEqual(DBNull.Value, column["NumericPrecision"]);
					Assert.AreEqual(DBNull.Value, column["NumericScale"]);
					Assert.AreEqual(false, column["IsUnique"]);
					Assert.AreEqual(false, column["IsKey"]);
					Assert.AreEqual(string.Empty, column["BaseServerName"]);
					Assert.AreEqual(string.Empty, column["BaseCatalogName"]);
					Assert.AreEqual(string.Empty, column["BaseSchemaName"]);
					Assert.AreEqual(string.Empty, column["BaseTableName"]);
					Assert.AreEqual(typeof(string), column["DataType"]);
					Assert.AreEqual(true, column["AllowDBNull"]);
					Assert.AreEqual((int) DbType.String, column["ProviderType"]);
					Assert.AreEqual(false, column["IsAliased"]);
					Assert.AreEqual(false, column["IsExpression"]);
					Assert.AreEqual(false, column["IsAutoIncrement"]);
					Assert.AreEqual(false, column["IsRowVersion"]);
					Assert.AreEqual(false, column["IsHidden"]);
					Assert.AreEqual(false, column["IsLong"]);
					Assert.AreEqual(true, column["IsReadOnly"]);

					Assert.AreEqual(index, column["ColumnOrdinal"]);

					Assert.AreEqual("Column" + index.ToString(CultureInfo.InvariantCulture), column["ColumnName"]);
					Assert.AreEqual("Column" + index.ToString(CultureInfo.InvariantCulture), column["BaseColumnName"]);
				}
			}
		}

		[Test()]
		public void GetSchemaTableReaderClosedTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleData1), true))
			{
				IDataReader reader = csv;
				csv.ReadNextRecord();
				reader.Close();

				reader.Invoking(x => x.GetSchemaTable()).Should().Throw<InvalidOperationException>();
			}
		}

		[Test()]
		public void NextResultTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleData1), true))
			{
				IDataReader reader = csv;
				Assert.IsFalse(reader.NextResult());

				csv.ReadNextRecord();
				Assert.IsFalse(reader.NextResult());
			}
		}

		[Test()]
		public void NextResultReaderClosedTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleData1), true))
			{
				IDataReader reader = csv;
				csv.ReadNextRecord();
				reader.Close();

				reader.Invoking(x => x.NextResult()).Should().Throw<InvalidOperationException>();
			}
		}

		[Test()]
		public void ReadTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleData1), true))
			{
				IDataReader reader = csv;

				for (int i = 0; i < CsvReaderSampleData.SampleData1RecordCount; i++)
					Assert.IsTrue(reader.Read());

				Assert.IsFalse(reader.Read());
			}
		}

		[Test()]
		public void ReadWithExcludeFilterTest()
		{
            var sampleData1 = new List<SampleData1>();
            var sampleData2 = new List<SampleData1>();
            using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleData1), true))
            {
                csv.CustomBooleanReplacer = new Dictionary<string, bool> { { "Y", true }, { "N", false } };
                csv.MissingFieldAction = MissingFieldAction.ReplaceByNull;
                IDataReader reader = csv;

                csv.Columns = new ColumnCollection
                {
                    {"First Name", typeof(string)},
                    {"Last Name", typeof(string)},
                    {"Address", typeof(string)},
                    {"City", typeof(string)},
                    {"State", typeof(string)},
                    {"Zip Code", typeof(string)},
                    {"IsActive", typeof(bool)},
                };

				while (reader.Read())
                {
                    var isActive = reader.GetValue(6);
                    sampleData1.Add(new SampleData1(reader[0].ToString(), reader[1].ToString(), reader[2].ToString(),
                        reader[3].ToString(), reader[4].ToString(), reader[5].ToString(),
                        isActive == DBNull.Value ? null : (bool?) isActive));
                }
			}
            using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleData1), true))
            {
                csv.CustomBooleanReplacer = new Dictionary<string, bool> { { "Y", true }, { "N", false } };
                csv.MissingFieldAction = MissingFieldAction.ReplaceByNull;

                IDataReader reader = csv;

                csv.Columns = new ColumnCollection
                {
                    {"First Name", typeof(string)},
                    {"Last Name", typeof(string)},
                    {"Address", typeof(string)},
                    {"City", typeof(string)},
                    {"State", typeof(string)},
                    {"Zip Code", typeof(string)},
                    {"IsActive", typeof(bool)},
                };
                csv.ExcludeFilter = () => csv["Zip Code"] == "00123";

				while (reader.Read())
                {
                    var isActive = reader.GetValue(6);
                    sampleData2.Add(new SampleData1(reader[0].ToString(), reader[1].ToString(), reader[2].ToString(),
                        reader[3].ToString(), reader[4].ToString(), reader[5].ToString(),
                        isActive == DBNull.Value ? null : (bool?)isActive));
                }
			}

            sampleData1.Should().HaveCount(6);
            sampleData1.Should().Contain(x => x.ZipCode == "00123");
            sampleData2.Should().HaveCount(5);
            sampleData2.Should().NotContain(x => x.ZipCode == "00123");
        }


		[Test()]
		public void VirtualColumnsTest()
		{
			var sampleData = new List<SampleData2>();
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleData1), true))
            {
                csv.VirtualColumns.Add(new Column {Name = "AmountOfChildren", DefaultValue = "1", Type = typeof(int), NumberStyles = NumberStyles.Integer});
                csv.VirtualColumns.Add(new Column {Name = "Sex", DefaultValue = Sex.M.ToString(), Type = typeof(Sex)});
				IDataReader reader = csv;

                reader.GetSchemaTable();
                csv.MissingFieldAction = MissingFieldAction.ReplaceByNull;
				while (reader.Read())
                {
                    var item = new SampleData2(reader[0].ToString(), reader[1].ToString(), reader[2].ToString(),
                        reader[3].ToString(), reader[4].ToString(), reader[5].ToString());

					item.AmountOfChildren = Convert.ToInt32(reader["AmountOfChildren"].ToString());
                    item.Sex = (Sex) Enum.Parse(typeof(Sex), reader["Sex"].ToString());
				    sampleData.Add(item);
                }
			}

			sampleData.Should().HaveCount(6);
			sampleData.Should().BeEquivalentTo(new List<SampleData2>
            {
				new SampleData2("John", "Doe", "120 jefferson st.", "Riverside", "NJ", "08075") { AmountOfChildren = 1, Sex = Sex.M },
				new SampleData2("Jack", "McGinnis", "220 hobo Av.", "Phila", "PA", "09119") { AmountOfChildren = 1, Sex = Sex.M },
				new SampleData2("John \"Da Man\"", "Repici", "120 Jefferson St.", "Riverside", "NJ", "08075") { AmountOfChildren = 1, Sex = Sex.M },
				new SampleData2("Stephen", "Tyler", "7452 Terrace \"At the Plaza\" road", "SomeTown", "SD", "91234") { AmountOfChildren = 1, Sex = Sex.M },
				new SampleData2("", "Blankman", "", "SomeTown", "SD", "00298") { AmountOfChildren = 1, Sex = Sex.M },
				new SampleData2("Joan \"the bone\", Anne", "Jet", "9th, at Terrace plc", "Desert City", "CO", "00123") { AmountOfChildren = 1, Sex = Sex.M },
            }, o => o.IncludingAllDeclaredProperties());
		}

		[Test()]
		public void ReadReaderClosedTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleData1), true))
			{
				IDataReader reader = csv;
				csv.ReadNextRecord();
				reader.Close();

				reader.Invoking(x => x.Read()).Should().Throw<InvalidOperationException>();
			}
		}

		[Test()]
		public void DepthTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleData1), true))
			{
				IDataReader reader = csv;
				Assert.AreEqual(0, reader.Depth);

				csv.ReadNextRecord();
				Assert.AreEqual(0, reader.Depth);
			}
		}

		[Test()]
		public void DepthReaderClosedTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleData1), true))
			{
				IDataReader reader = csv;
				csv.ReadNextRecord();
				reader.Close();

			    // ReSharper disable once UnusedVariable
				reader.Invoking(x => { var depth = x.Depth; }).Should().Throw<InvalidOperationException>();
			}
		}

		[Test()]
		public void IsClosedTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleData1), true))
			{
				IDataReader reader = csv;
				Assert.IsFalse(reader.IsClosed);

				csv.ReadNextRecord();
				Assert.IsFalse(reader.IsClosed);

				reader.Close();
				Assert.IsTrue(reader.IsClosed);
			}
		}

		[Test()]
		public void RecordsAffectedTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleData1), true))
			{
				IDataReader reader = csv;
				Assert.AreEqual(-1, reader.RecordsAffected);

				csv.ReadNextRecord();
				Assert.AreEqual(-1, reader.RecordsAffected);

				reader.Close();
				Assert.AreEqual(-1, reader.RecordsAffected);
			}
		}

		#endregion

		#region IDataRecord interface

		[Test()]
		public void GetBooleanTest1()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleTypedData1), true))
			{
				IDataReader reader = csv;

				Boolean value = true;
				while (reader.Read())
				{
					Assert.AreEqual(value, reader.GetBoolean(reader.GetOrdinal(typeof(Boolean).FullName)));
				}
			}
		}
		
		
		[Test]
		public void GetBooleanTest2()
		{
			// arrange
			const string data = "Data,HasData\r\n\"data\", false\r\ntest, Y";

			// act
			// assert
			using (var csv = new CsvReader(new StringReader(data), true, ','))
			{
				csv.CustomBooleanReplacer = new Dictionary<string, bool> {{"N", false}, {"Y", true}};
				csv.MissingFieldAction = MissingFieldAction.ReplaceByNull;
				IDataReader reader = csv;

				reader.Read().Should().BeTrue();
				reader.GetString(0).Should().Be("data");
				reader.GetBoolean(1).Should().BeFalse();

				reader.Read().Should().BeTrue();
				reader.GetString(0).Should().Be("test");
				reader.GetBoolean(1).Should().BeTrue();
				
				reader.Read().Should().BeFalse();
			}
		}

		[Test()]
		public void GetByteTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleTypedData1), true))
			{
				IDataReader reader = csv;

				Byte value = 1;
				while (reader.Read())
				{
					Assert.AreEqual(value, reader.GetByte(reader.GetOrdinal(typeof(Byte).FullName)));
				}
			}
		}

		[Test()]
		public void GetBytesTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleTypedData1), true))
			{
				IDataReader reader = csv;

				Char[] temp = "abc".ToCharArray();
				Byte[] value = new Byte[temp.Length];

				for (int i = 0; i < temp.Length; i++)
					value[i] = Convert.ToByte(temp[i]);

				while (reader.Read())
				{
					Byte[] csvValue = new Byte[value.Length];

					long count = reader.GetBytes(reader.GetOrdinal(typeof(String).FullName), 0, csvValue, 0, value.Length);

					Assert.AreEqual(value.Length, count);
					Assert.AreEqual(value.Length, csvValue.Length);

					for (int i = 0; i < value.Length; i++)
						Assert.AreEqual(value[i], csvValue[i]);
				}
			}
		}

		[Test()]
		public void GetCharTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleTypedData1), true))
			{
				IDataReader reader = csv;

				Char value = 'a';
				while (reader.Read())
				{
					Assert.AreEqual(value, reader.GetChar(reader.GetOrdinal(typeof(Char).FullName)));
				}
			}
		}

		[Test()]
		public void GetCharsTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleTypedData1), true))
			{
				IDataReader reader = csv;

				Char[] value = "abc".ToCharArray();
				while (reader.Read())
				{
					Char[] csvValue = new Char[value.Length];

					long count = reader.GetChars(reader.GetOrdinal(typeof(String).FullName), 0, csvValue, 0, value.Length);

					Assert.AreEqual(value.Length, count);
					Assert.AreEqual(value.Length, csvValue.Length);

					for (int i = 0; i < value.Length; i++)
						Assert.AreEqual(value[i], csvValue[i]);
				}
			}
		}

		[Test()]
		public void GetDataTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleTypedData1), true))
			{
				IDataReader reader = csv;

				while (reader.Read())
				{
					Assert.AreSame(csv, reader.GetData(0));

					for (int i = 1; i < reader.FieldCount; i++)
						Assert.IsNull(reader.GetData(i));
				}
			}
		}

		[Test()]
		public void GetDataTypeNameTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleTypedData1), true))
			{
				IDataReader reader = csv;

				while (reader.Read())
				{
					for (int i = 0; i < reader.FieldCount; i++)
						Assert.AreEqual(typeof(string).FullName, reader.GetDataTypeName(i));
				}
			}
		}

		[Test()]
		public void GetDateTimeTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleTypedData1), true))
			{
				IDataReader reader = csv;

				DateTime value = new DateTime(2001, 1, 1);
				while (reader.Read())
				{
					Assert.AreEqual(value, reader.GetDateTime(reader.GetOrdinal(typeof(DateTime).FullName)));
				}
			}
		}

		[Test()]
		public void GetDecimalTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleTypedData1), true))
			{
				IDataReader reader = csv;

				Decimal value = 1;
				while (reader.Read())
				{
					Assert.AreEqual(value, reader.GetDecimal(reader.GetOrdinal(typeof(Decimal).FullName)));
				}
			}
		}

		[Test()]
		public void GetDoubleTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleTypedData1), true))
			{
				IDataReader reader = csv;

				Double value = 1;
				while (reader.Read())
				{
					Assert.AreEqual(value, reader.GetDouble(reader.GetOrdinal(typeof(Double).FullName)));
				}
			}
		}

		[Test()]
		public void GetFieldTypeTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleTypedData1), true))
			{
				IDataReader reader = csv;

				while (reader.Read())
				{
					for (int i = 0; i < reader.FieldCount; i++)
						Assert.AreEqual(typeof(string), reader.GetFieldType(i));
				}
			}
		}

		[Test()]
		public void GetFloatTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleTypedData1), true))
			{
				IDataReader reader = csv;

				Single value = 1;
				while (reader.Read())
				{
					Assert.AreEqual(value, reader.GetFloat(reader.GetOrdinal(typeof(Single).FullName)));
				}
			}
		}

		[Test()]
		public void GetGuidTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleTypedData1), true))
			{
				IDataReader reader = csv;

				Guid value = new Guid("{11111111-1111-1111-1111-111111111111}");
				while (reader.Read())
				{
					Assert.AreEqual(value, reader.GetGuid(reader.GetOrdinal(typeof(Guid).FullName)));
				}
			}
		}

		[Test()]
		public void GetInt16Test()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleTypedData1), true))
			{
				IDataReader reader = csv;

				Int16 value = 1;
				while (reader.Read())
				{
					Assert.AreEqual(value, reader.GetInt16(reader.GetOrdinal(typeof(Int16).FullName)));
				}
			}
		}

		[Test()]
		public void GetInt32Test()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleTypedData1), true))
			{
				IDataReader reader = csv;

				Int32 value = 1;
				while (reader.Read())
				{
					Assert.AreEqual(value, reader.GetInt32(reader.GetOrdinal(typeof(Int32).FullName)));
				}
			}
		}

		[Test()]
		public void GetInt64Test()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleTypedData1), true))
			{
				IDataReader reader = csv;

				Int64 value = 1;
				while (reader.Read())
				{
					Assert.AreEqual(value, reader.GetInt64(reader.GetOrdinal(typeof(Int64).FullName)));
				}
			}
		}

		[Test()]
		public void GetNameTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleData1), true))
			{
				IDataReader reader = csv;

				while (reader.Read())
				{
					Assert.AreEqual(CsvReaderSampleData.SampleData1Header0, reader.GetName(0));
					Assert.AreEqual(CsvReaderSampleData.SampleData1Header1, reader.GetName(1));
					Assert.AreEqual(CsvReaderSampleData.SampleData1Header2, reader.GetName(2));
					Assert.AreEqual(CsvReaderSampleData.SampleData1Header3, reader.GetName(3));
					Assert.AreEqual(CsvReaderSampleData.SampleData1Header4, reader.GetName(4));
					Assert.AreEqual(CsvReaderSampleData.SampleData1Header5, reader.GetName(5));
				}
			}
		}

		[Test()]
		public void GetOrdinalTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleData1), true))
			{
				IDataReader reader = csv;

				while (reader.Read())
				{
					Assert.AreEqual(0, reader.GetOrdinal(CsvReaderSampleData.SampleData1Header0));
					Assert.AreEqual(1, reader.GetOrdinal(CsvReaderSampleData.SampleData1Header1));
					Assert.AreEqual(2, reader.GetOrdinal(CsvReaderSampleData.SampleData1Header2));
					Assert.AreEqual(3, reader.GetOrdinal(CsvReaderSampleData.SampleData1Header3));
					Assert.AreEqual(4, reader.GetOrdinal(CsvReaderSampleData.SampleData1Header4));
					Assert.AreEqual(5, reader.GetOrdinal(CsvReaderSampleData.SampleData1Header5));
				}
			}
		}

		[Test()]
		public void GetStringTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleTypedData1), true))
			{
				IDataReader reader = csv;

				String value = "abc";
				while (reader.Read())
				{
					Assert.AreEqual(value, reader.GetString(reader.GetOrdinal(typeof(String).FullName)));
				}
			}
		}

		[Test()]
		public void GetValueTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleData1), true))
			{
				IDataReader reader = csv;

				string[] values = new string[CsvReaderSampleData.SampleData1FieldCount];

				while (reader.Read())
				{
					for (int i = 0; i < reader.FieldCount; i++)
					{
						object value = reader.GetValue(i);

						if (string.IsNullOrEmpty(csv[i]))
							Assert.AreEqual(DBNull.Value, value);

						values[i] = value.ToString();
					}

					CsvReaderSampleData.CheckSampleData1(csv.HasHeaders, csv.CurrentRecordIndex, values);
				}
			}
		}

		[Test()]
		public void GetValuesTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleData1), true))
			{
				IDataReader reader = csv;

				object[] objValues = new object[CsvReaderSampleData.SampleData1FieldCount];
				string[] values = new string[CsvReaderSampleData.SampleData1FieldCount];

				while (reader.Read())
				{
					Assert.AreEqual(CsvReaderSampleData.SampleData1FieldCount, reader.GetValues(objValues));

					for (int i = 0; i < reader.FieldCount; i++)
					{
						if (string.IsNullOrEmpty(csv[i]))
							Assert.AreEqual(DBNull.Value, objValues[i]);

						values[i] = objValues[i].ToString();
					}

					CsvReaderSampleData.CheckSampleData1(csv.HasHeaders, csv.CurrentRecordIndex, values);
				}
			}
		}

		[Test()]
		public void IsDBNullTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleTypedData1), true))
			{
				IDataReader reader = csv;

				while (reader.Read())
				{
					Assert.IsTrue(reader.IsDBNull(reader.GetOrdinal(typeof(DBNull).FullName)));
				}
			}
		}

		[Test()]
		public void IsDBNullWithNullValueTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleTypedData1), true, 
				CsvReader.DefaultDelimiter, CsvReader.DefaultQuote, CsvReader.DefaultEscape, CsvReader.DefaultComment,
				ValueTrimmingOptions.UnquotedOnly, CsvReaderSampleData.SampleNullValue))
			{
				IDataReader reader = csv;

				while (reader.Read())
				{
					Assert.IsTrue(reader.IsDBNull(reader.GetOrdinal(CsvReaderSampleData.DbNullWithNullValueHeader)));
					Assert.IsFalse(reader.IsDBNull(reader.GetOrdinal(typeof(DBNull).FullName)));
				}
			}
		}

		[Test()]
		public void FieldCountTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleData1), true))
			{
				IDataReader reader = csv;

				Assert.AreEqual(CsvReaderSampleData.SampleData1FieldCount, reader.FieldCount);
			}
		}

		[Test()]
		public void IndexerByFieldNameTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleData1), true))
			{
				IDataReader reader = csv;

				string[] values = new string[CsvReaderSampleData.SampleData1FieldCount];

				while (reader.Read())
				{
					values[0] = (string) reader[CsvReaderSampleData.SampleData1Header0];
					values[1] = (string) reader[CsvReaderSampleData.SampleData1Header1];
					values[2] = (string) reader[CsvReaderSampleData.SampleData1Header2];
					values[3] = (string) reader[CsvReaderSampleData.SampleData1Header3];
					values[4] = (string) reader[CsvReaderSampleData.SampleData1Header4];
					values[5] = (string) reader[CsvReaderSampleData.SampleData1Header5];
					values[6] = (string) reader[CsvReaderSampleData.SampleData1Header6];

					CsvReaderSampleData.CheckSampleData1(csv.HasHeaders, csv.CurrentRecordIndex, values);
				}
			}
		}

		[Test()]
		public void IndexerByFieldIndexTest()
		{
			using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleData1), true))
			{
				IDataReader reader = csv;

				string[] values = new string[CsvReaderSampleData.SampleData1FieldCount];

				while (reader.Read())
				{
					for (int i = 0; i < reader.FieldCount; i++)
						values[i] = (string) reader[i];

					CsvReaderSampleData.CheckSampleData1(csv.HasHeaders, csv.CurrentRecordIndex, values);
				}
			}
		}

		[Test]
		public void SqlBulkCopyTest()
		{
			// arrange
            var actual = new List<SampleData1>();
            var dbFile = Path.Combine(Directory.GetCurrentDirectory(), "csvtest.mdf");
			using (var connection = new SqlConnection($"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={dbFile};Integrated Security=True;Connect Timeout=30"))
            {
                connection.Open();
                var command = new SqlCommand("TRUNCATE TABLE [dbo].[SampleData1]", connection);
                command.ExecuteNonQuery();

				using (var csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleData1), true))
                {
                    var sqlBulkCopy = new SqlBulkCopy(connection);
                    sqlBulkCopy.DestinationTableName = "[dbo].[SampleData1]";
                    csv.SkipEmptyLines = true;
                    csv.CustomBooleanReplacer = new Dictionary<string, bool> {{"Y", true}, {"N", false}};
                    csv.MissingFieldAction = MissingFieldAction.ReplaceByNull;
                    IDataReader reader = csv;

                    csv.Columns = new ColumnCollection
                    {
                        {"First Name", typeof(string)},
                        {"Last Name", typeof(string)},
                        {"Address", typeof(string)},
                        {"City", typeof(string)},
                        {"State", typeof(string)},
                        {"Zip Code", typeof(string)},
                        {"IsActive", typeof(bool)},
                    };

					sqlBulkCopy.ColumnMappings.Add("First Name", "FirstName");
                    sqlBulkCopy.ColumnMappings.Add("Last Name", "LastName");
				    sqlBulkCopy.ColumnMappings.Add("Address", "Address");
				    sqlBulkCopy.ColumnMappings.Add("City", "City");
                    sqlBulkCopy.ColumnMappings.Add("State", "State");
                    sqlBulkCopy.ColumnMappings.Add("Zip Code", "ZipCode");
				    sqlBulkCopy.ColumnMappings.Add("IsActive", "IsActive");
				    sqlBulkCopy.WriteToServer(reader);
                }

                command = new SqlCommand("SELECT [FirstName], [LastName], [Address], [City], [State], [ZipCode], [IsActive] FROM [dbo].[SampleData1]", connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var firstName = reader.GetSqlString(reader.GetOrdinal("FirstName"));
                        var lastName = reader.GetSqlString(reader.GetOrdinal("LastName"));
                        var address = reader.GetSqlString(reader.GetOrdinal("Address"));
                        var city = reader.GetSqlString(reader.GetOrdinal("City"));
                        var state = reader.GetSqlString(reader.GetOrdinal("State"));
                        var zipCode = reader.GetSqlString(reader.GetOrdinal("ZipCode"));
                        var isActive = reader.GetSqlBoolean(reader.GetOrdinal("IsActive"));
                        var sampleData = new SampleData1(firstName.IsNull ? null : firstName.Value,
                            lastName.IsNull ? null : lastName.Value,
                            address.IsNull ? null : address.Value,
                            city.IsNull ? null : city.Value,
                            state.IsNull ? null : state.Value,
                            zipCode.IsNull ? null : zipCode.Value,
                            isActive.IsNull ? (bool?) null : isActive.Value);
						actual.Add(sampleData);
                    }
                }
            }

			// assert
            actual.Should().BeEquivalentTo(new List<SampleData1>
            {
                new SampleData1("John", "Doe", "120 jefferson st.", "Riverside", "NJ", "08075", true),
                new SampleData1("Jack", "McGinnis", "220 hobo Av.", "Phila", "PA", "09119", false),
                new SampleData1("John \"Da Man\"", "Repici", "120 Jefferson St.", "Riverside", "NJ", "08075", null),
                new SampleData1("Stephen", "Tyler", "7452 Terrace \"At the Plaza\" road", "SomeTown", "SD", "91234", null),
                new SampleData1(null, "Blankman", null, "SomeTown", "SD", "00298", null),
                new SampleData1("Joan \"the bone\", Anne", "Jet", "9th, at Terrace plc", "Desert City", "CO", "00123", null),
            }, o => o.IncludingAllDeclaredProperties());
		}

		#endregion

		private class SampleData1
        {
            public SampleData1(string firstName, string lastName, string address, string city, string state, string zipCode, bool? isActive)
            {
                FirstName = firstName;
                LastName = lastName;
                Address = address;
                City = city;
                State = state;
                ZipCode = zipCode;
                IsActive = isActive;
            }

            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Address { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string ZipCode { get; set; }
            public bool? IsActive { get; set; }
        }

        private class SampleData2
        {
            public SampleData2(string firstName, string lastName, string address, string city, string state, string zipCode)
            {
                FirstName = firstName;
                LastName = lastName;
                Address = address;
                City = city;
                State = state;
                ZipCode = zipCode;
            }

            public string FirstName { get; set; }
            public string LastName { get; set; }
            public int AmountOfChildren { get; set; }
            public string Address { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string ZipCode { get; set; }
			public Sex Sex { get; set; }
        }

        private enum Sex
        {
			M,
			W,
			D
        }
    }
}
