//	LumenWorks.Framework.Tests.Unit.IO.CSV.CsvReaderSampleData
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
//	copies or substantial portions of the Software.
//
//	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
//	INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
//	PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE 
//	FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
//	ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using NUnit.Framework;

namespace CsvReader.UnitTests
{
	public class CsvReaderSampleData
	{
		#region Sample data

		public const int SampleData1RecordCount = 6;
		public const int SampleData1FieldCount = 7;

		public const string SampleData1Header0 = "First Name";
		public const string SampleData1Header1 = "Last Name";
		public const string SampleData1Header2 = "Address";
		public const string SampleData1Header3 = "City";
		public const string SampleData1Header4 = "State";
		public const string SampleData1Header5 = "Zip Code";
		public const string SampleData1Header6 = "IsActive";

		// <blank>
		// # This is a comment
		// "First Name", "Last Name", Address, City, State, "Zip Code"<tab>
		// John,Doe,120 jefferson st.,Riverside, NJ, 08075
		// Jack,McGinnis,220 hobo Av.,Phila<tab>, PA,09119
		// "John ""Da Man""",Repici,120 Jefferson St.,Riverside, NJ,08075
		// <blank>
		// # This is a comment
		// Stephen,Tyler,"7452 Terrace ""At the Plaza"" road",SomeTown,SD, 91234
		// ,Blankman,,SomeTown, SD, 00298
		// "Joan ""the bone"", Anne",Jet,"9th, at Terrace plc",Desert City,CO,00123

		public const string SampleData1 = @"
# This is a comment
""First Name"", ""Last Name"", Address, City, State, ""Zip Code"", IsActive	
John,Doe,120 jefferson st.,Riverside, NJ, 08075, Y
Jack,McGinnis,220 hobo Av.,Phila	, PA,09119, N
""John """"Da Man"""""",Repici,120 Jefferson St.,Riverside, NJ,08075, N/A

# This is a comment
Stephen,Tyler,""7452 Terrace """"At the Plaza"""" road"",SomeTown,SD, 91234,
,Blankman,,SomeTown, SD, 00298,
""Joan """"the bone"""", Anne"",Jet,""9th, at Terrace plc"",Desert City,CO,00123,";

		public const string SampleData2 = @"
# This is a comment
""First Name"", ""Last Name"", Address, City, State, ""Zip Code"", IsActive, Data
John,Doe,120 jefferson st.,Riverside, NJ, 08075, A, 00
Jack,McGinnis,220 hobo Av.,Phila	, PA,09119, B, 0x01
""John """"Da Man"""""",Repici,120 Jefferson St.,Riverside, NJ,08075, N/A,";

		public const string SampleTypedData1 = @"
System.Boolean,System.DateTime,System.Single,System.Double,System.Decimal,System.SByte,System.Int16,System.Int32,System.Int64,System.Byte,System.UInt16,System.UInt32,System.UInt64,System.Char,System.String,System.Guid,System.DBNull,System.DBNullWithNullValue
1,2001-01-01,1,1,1,1,1,1,1,1,1,1,1,a,abc,{11111111-1111-1111-1111-111111111111},,_<null>_
""true"",""2001-01-01"",""1"",""1"",""1"",""1"",""1"",""1"",""1"",""1"",""1"",""1"",""1"",""a"",""abc"",""{11111111-1111-1111-1111-111111111111}"","""",""_<NULL>_""";

		public const string DbNullWithNullValueHeader = "System.DBNullWithNullValue";

		public const string SampleNullValue = "_<null>_";

		#endregion

		#region Sample data utility methods

		public static void CheckSampleData1(CsvReader csv, bool readToEnd)
		{
			if (readToEnd)
			{
				Assert.That(csv.FieldCount, Is.EqualTo(CsvReaderSampleData.SampleData1FieldCount));

				if (csv.HasHeaders)
				{
					Assert.That(csv.GetFieldIndex(SampleData1Header0), Is.EqualTo(0));
					Assert.That(csv.GetFieldIndex(SampleData1Header1), Is.EqualTo(1));
					Assert.That(csv.GetFieldIndex(SampleData1Header2), Is.EqualTo(2));
					Assert.That(csv.GetFieldIndex(SampleData1Header3), Is.EqualTo(3));
					Assert.That(csv.GetFieldIndex(SampleData1Header4), Is.EqualTo(4));
					Assert.That(csv.GetFieldIndex(SampleData1Header5), Is.EqualTo(5));
					Assert.That(csv.GetFieldIndex(SampleData1Header6), Is.EqualTo(6));
				}
				
				Assert.That(csv.CurrentRecordIndex, Is.EqualTo(-1));

				int recordCount = 0;

				while (csv.ReadNextRecord())
				{
					CheckSampleData1(csv.CurrentRecordIndex, csv);
					recordCount++;
				}

				if (csv.HasHeaders)
					Assert.That(recordCount, Is.EqualTo(CsvReaderSampleData.SampleData1RecordCount));
				else
					Assert.That(recordCount, Is.EqualTo(CsvReaderSampleData.SampleData1RecordCount + 1));
			}
			else
				CheckSampleData1(csv.CurrentRecordIndex, csv);
		}

		public static void CheckSampleData1(long recordIndex, CsvReader csv)
		{
			string[] fields = new string[7];
			csv.CopyCurrentRecordTo(fields);

			CheckSampleData1(csv.HasHeaders, recordIndex, fields, 0);
		}

		public static void CheckSampleData1(bool hasHeaders, long recordIndex, string[] fields)
		{
			CheckSampleData1(hasHeaders, recordIndex, fields, 0);
		}

		public static void CheckSampleData1(bool hasHeaders, long recordIndex, string[] fields, int startIndex)
		{
			Assert.That(fields.Length - startIndex, Is.GreaterThanOrEqualTo(6));

			long index = recordIndex;

			if (hasHeaders)
				index++;

			switch (index)
			{
				case 0:
					Assert.That(fields[startIndex], Is.EqualTo(SampleData1Header0));
					Assert.That(fields[startIndex + 1], Is.EqualTo(SampleData1Header1));
					Assert.That(fields[startIndex + 2], Is.EqualTo(SampleData1Header2));
					Assert.That(fields[startIndex + 3], Is.EqualTo(SampleData1Header3));
					Assert.That(fields[startIndex + 4], Is.EqualTo(SampleData1Header4));
					Assert.That(fields[startIndex + 5], Is.EqualTo(SampleData1Header5));
					Assert.That(fields[startIndex + 6], Is.EqualTo(SampleData1Header6));
					break;

				case 1:
					Assert.That(fields[startIndex], Is.EqualTo("John"));
					Assert.That(fields[startIndex + 1], Is.EqualTo("Doe"));
					Assert.That(fields[startIndex + 2], Is.EqualTo("120 jefferson st."));
					Assert.That(fields[startIndex + 3], Is.EqualTo("Riverside"));
					Assert.That(fields[startIndex + 4], Is.EqualTo("NJ"));
					Assert.That(fields[startIndex + 5], Is.EqualTo("08075"));
					Assert.That(fields[startIndex + 6], Is.EqualTo("Y"));
					break;

				case 2:
					Assert.That(fields[startIndex], Is.EqualTo("Jack"));
					Assert.That(fields[startIndex + 1], Is.EqualTo("McGinnis"));
					Assert.That(fields[startIndex + 2], Is.EqualTo("220 hobo Av."));
					Assert.That(fields[startIndex + 3], Is.EqualTo("Phila"));
					Assert.That(fields[startIndex + 4], Is.EqualTo("PA"));
					Assert.That(fields[startIndex + 5], Is.EqualTo("09119"));
					Assert.That(fields[startIndex + 6], Is.EqualTo("N"));
					break;

				case 3:
					Assert.That(fields[startIndex], Is.EqualTo(@"John ""Da Man"""));
					Assert.That(fields[startIndex + 1], Is.EqualTo("Repici"));
					Assert.That(fields[startIndex + 2], Is.EqualTo("120 Jefferson St."));
					Assert.That(fields[startIndex + 3], Is.EqualTo("Riverside"));
					Assert.That(fields[startIndex + 4], Is.EqualTo("NJ"));
					Assert.That(fields[startIndex + 5], Is.EqualTo("08075"));
					Assert.That(fields[startIndex + 6], Is.EqualTo("N/A"));
					break;

				case 4:
					Assert.That(fields[startIndex], Is.EqualTo("Stephen"));
					Assert.That(fields[startIndex + 1], Is.EqualTo("Tyler"));
					Assert.That(fields[startIndex + 2], Is.EqualTo(@"7452 Terrace ""At the Plaza"" road"));
					Assert.That(fields[startIndex + 3], Is.EqualTo("SomeTown"));
                    Assert.That(fields[startIndex + 4], Is.EqualTo("SD"));
					Assert.That(fields[startIndex + 5], Is.EqualTo("91234"));
                    Assert.That(fields[startIndex + 6], Is.EqualTo(""));
					break;

				case 5:
					Assert.That(fields[startIndex], Is.EqualTo(""));
					Assert.That(fields[startIndex + 1], Is.EqualTo("Blankman"));
					Assert.That(fields[startIndex + 2], Is.EqualTo(""));
					Assert.That(fields[startIndex + 3], Is.EqualTo("SomeTown"));
					Assert.That(fields[startIndex + 4], Is.EqualTo("SD"));
					Assert.That(fields[startIndex + 5], Is.EqualTo("00298"));
                    Assert.That(fields[startIndex + 6], Is.EqualTo(""));
					break;

				case 6:
					Assert.That(fields[startIndex], Is.EqualTo(@"Joan ""the bone"", Anne"));
					Assert.That(fields[startIndex + 1], Is.EqualTo("Jet"));
					Assert.That(fields[startIndex + 2], Is.EqualTo("9th, at Terrace plc"));
					Assert.That(fields[startIndex + 3], Is.EqualTo("Desert City"));
					Assert.That(fields[startIndex + 4], Is.EqualTo("CO"));
					Assert.That(fields[startIndex + 5], Is.EqualTo("00123"));
                    Assert.That(fields[startIndex + 6], Is.EqualTo(""));
					break;

				default:
					throw new IndexOutOfRangeException(string.Format("Specified recordIndex is '{0}'. Possible range is [0, 5].", recordIndex));
			}
		}

		#endregion
	}
}
