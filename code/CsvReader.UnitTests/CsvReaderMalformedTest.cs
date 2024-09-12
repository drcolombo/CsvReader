//	LumenWorks.Framework.Tests.Unit.IO.CSV.CsvReaderMalformedTest
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


// A special thanks goes to "shriop" at CodeProject for providing many of the standard and Unicode parsing tests.


using System.Collections.Generic;
using System.IO;
using System.Text;
using CsvReader.Events;
using CsvReader.Exceptions;
using FluentAssertions;
using NUnit.Framework;

namespace CsvReader.UnitTests
{
	[TestFixture()]
	public class CsvReaderMalformedTest
	{
		#region Utilities

		private void CheckMissingFieldUnquoted(long recordCount, int fieldCount, long badRecordIndex, int badFieldIndex, int bufferSize)
		{
			CheckMissingFieldUnquoted(recordCount, fieldCount, badRecordIndex, badFieldIndex, bufferSize, true, MissingFieldAction.ParseError);
			CheckMissingFieldUnquoted(recordCount, fieldCount, badRecordIndex, badFieldIndex, bufferSize, true, MissingFieldAction.ReplaceByEmpty);
			CheckMissingFieldUnquoted(recordCount, fieldCount, badRecordIndex, badFieldIndex, bufferSize, true, MissingFieldAction.ReplaceByNull);

			CheckMissingFieldUnquoted(recordCount, fieldCount, badRecordIndex, badFieldIndex, bufferSize, false, MissingFieldAction.ParseError);
			CheckMissingFieldUnquoted(recordCount, fieldCount, badRecordIndex, badFieldIndex, bufferSize, false, MissingFieldAction.ReplaceByEmpty);
			CheckMissingFieldUnquoted(recordCount, fieldCount, badRecordIndex, badFieldIndex, bufferSize, false, MissingFieldAction.ReplaceByNull);
		}

		private void CheckMissingFieldUnquoted(long recordCount, int fieldCount, long badRecordIndex, int badFieldIndex, int bufferSize, bool sequentialAccess, MissingFieldAction action)
		{
			// construct the csv data with template "00,01,02\n10,11,12\n...." and calculate expected error position

			long capacity = recordCount * (fieldCount * 2 + fieldCount - 1) + recordCount;
			Assert.That(capacity, Is.LessThanOrEqualTo(int.MaxValue));

			StringBuilder sb = new StringBuilder((int) capacity);
			int expectedErrorPosition = 0;

			for (long i = 0; i < recordCount; i++)
			{
				int realFieldCount;

				if (i == badRecordIndex)
					realFieldCount = badFieldIndex;
				else
					realFieldCount = fieldCount;

				for (int j = 0; j < realFieldCount; j++)
				{
					sb.Append(i);
					sb.Append(j);
					sb.Append(CsvReader.DefaultDelimiter);
				}

				sb.Length--;
				sb.Append('\n');

				if (i == badRecordIndex)
				{
					expectedErrorPosition = sb.Length % bufferSize;

					// when eof is true, buffer is cleared and position is reset to 0, so exception will have CurrentPosition = 0
					if (i == recordCount - 1)
						expectedErrorPosition = 0;
				}
			}

			// test csv

			using (CsvReader csv = new CsvReader(new StringReader(sb.ToString()), false, bufferSize))
            {
                csv.DefaultParseErrorAction = ParseErrorAction.ThrowException;
				csv.MissingFieldAction = action;
				Assert.That(csv.FieldCount, Is.EqualTo(fieldCount));

				while (csv.ReadNextRecord())
				{
					Assert.That(csv.FieldCount, Is.EqualTo(fieldCount));

					// if not sequential, directly test the missing field
					if (!sequentialAccess)
						CheckMissingFieldValueUnquoted(csv, badFieldIndex, badRecordIndex, badFieldIndex, expectedErrorPosition, sequentialAccess, action);

					for (int i = 0; i < csv.FieldCount; i++)
						CheckMissingFieldValueUnquoted(csv, i, badRecordIndex, badFieldIndex, expectedErrorPosition, sequentialAccess, action);
				}
			}
		}

		private void CheckMissingFieldValueUnquoted(CsvReader csv, int fieldIndex, long badRecordIndex, int badFieldIndex, int expectedErrorPosition, bool sequentialAccess, MissingFieldAction action)
		{
			const string Message = "RecordIndex={0}; FieldIndex={1}; Position={2}; Sequential={3}; Action={4}";

			// make sure s contains garbage as to not have false successes
			string s = "asdfasdfasdf";

			try
			{
				s = csv[fieldIndex];
			}
			catch (MissingFieldCsvException ex)
			{
                Assert.That(ex.CurrentRecordIndex, Is.EqualTo(badRecordIndex), string.Format(Message, ex.CurrentRecordIndex, ex.CurrentFieldIndex, ex.CurrentPosition, sequentialAccess, action));
				Assert.That(badFieldIndex, Is.LessThanOrEqualTo(fieldIndex), string.Format(Message, ex.CurrentRecordIndex, ex.CurrentFieldIndex, ex.CurrentPosition, sequentialAccess, action));
				Assert.That(ex.CurrentPosition, Is.EqualTo(expectedErrorPosition), string.Format(Message, ex.CurrentRecordIndex, ex.CurrentFieldIndex, ex.CurrentPosition, sequentialAccess, action));

				return;
			}

			if (csv.CurrentRecordIndex != badRecordIndex || fieldIndex < badFieldIndex)
				Assert.That(s, Is.EqualTo(csv.CurrentRecordIndex.ToString() + fieldIndex), string.Format(Message, csv.CurrentRecordIndex, fieldIndex, -1, sequentialAccess, action));
			else
			{
				switch (action)
				{
					case MissingFieldAction.ReplaceByEmpty:
						Assert.That(s, Is.EqualTo(string.Empty), string.Format(Message, csv.CurrentRecordIndex, fieldIndex, -1, sequentialAccess, action));
						break;

					case MissingFieldAction.ReplaceByNull:
						Assert.That(s, Is.Null, string.Format(Message, csv.CurrentRecordIndex, fieldIndex, -1, sequentialAccess, action));
						break;

					case MissingFieldAction.ParseError:
						Assert.Fail(string.Format("Failed to throw ParseError. - " + Message, csv.CurrentRecordIndex, fieldIndex, -1, sequentialAccess, action));
						break;

					default:
						Assert.Fail($"'{action}' is not handled by this test.");
						break;
				}
			}
		}

		#endregion

		[Test()]
		public void MissingFieldUnquotedTest1()
		{
			CheckMissingFieldUnquoted(4, 4, 2, 2, CsvReader.DefaultBufferSize);
			CheckMissingFieldUnquoted(4, 4, 2, 2, CsvReader.DefaultBufferSize);
		}

		[Test()]
		public void MissingFieldUnquotedTest2()
		{
			// With bufferSize = 16, faulty new line char is at the start of next buffer read
			CheckMissingFieldUnquoted(4, 4, 2, 3, 16);
		}

		[Test()]
		public void MissingFieldUnquotedTest3()
		{
			// test missing field when end of buffer has been reached
			CheckMissingFieldUnquoted(3, 4, 2, 3, 16);
		}

		[Test()]
		public void MissingFieldAllQuotedFields_Issue_12()
		{
			string sample =
				"\"A\",\"B\"\n" +
				"\"1\",\"2\"\n" +
				"\"3\"\n" +
				"\"5\",\"6\"";

			string[] buffer = new string[2];

			using (CsvReader csv = new CsvReader(new StringReader(sample), false))
            {
                csv.DefaultParseErrorAction = ParseErrorAction.ThrowException;
			    try
			    {
			        while (csv.ReadNextRecord())
			        {
			            csv.CopyCurrentRecordTo(buffer);
			        }
			    }
			    catch (MissingFieldCsvException)
			    {
			        Assert.Pass();
			    }
			}
            Assert.Fail();
		}

		[Test()]
		public void MissingFieldQuotedTest1()
		{
			const string Data = "a,b,c,d\n1,1,1,1\n2,\"2\"\n3,3,3,3";

			try
			{
				using (CsvReader csv = new CsvReader(new StringReader(Data), false))
				{
					while (csv.ReadNextRecord())
						for (int i = 0; i < csv.FieldCount; i++)
						{
							string s = csv[i];
						}
				}
			}
			catch (MissingFieldCsvException ex)
			{
			    if (ex.CurrentRecordIndex == 2 && ex.CurrentFieldIndex == 2 && ex.CurrentPosition == 22)
			    {
			        Assert.Pass();
			    }
			    else
			    {
			        Assert.Fail();
			    }
			}
		}

		[Test()]
		public void MissingFieldQuotedTest2()
		{
			const string Data = "a,b,c,d\n1,1,1,1\n2,\"2\",\n3,3,3,3";

			try
			{
				using (CsvReader csv = new CsvReader(new StringReader(Data), false, 11))
				{
					while (csv.ReadNextRecord())
						for (int i = 0; i < csv.FieldCount; i++)
						{
							string s = csv[i];
						}
				}
			}
			catch (MissingFieldCsvException ex)
			{
                if (ex.CurrentRecordIndex == 2 && ex.CurrentFieldIndex == 2 && ex.CurrentPosition == 1)
                {
                    Assert.Pass();
                }
                else
                {
                    Assert.Fail();
                }
            }
        }

		[Test()]
		public void MissingFieldQuotedTest3()
		{
			const string Data = "a,b,c,d\n1,1,1,1\n2,\"2\"\n\"3\",3,3,3";

			try
			{
				using (CsvReader csv = new CsvReader(new StringReader(Data), false))
				{
					while (csv.ReadNextRecord())
						for (int i = 0; i < csv.FieldCount; i++)
						{
							string s = csv[i];
						}
				}
			}
			catch (MissingFieldCsvException ex)
			{
			    if (ex.CurrentRecordIndex == 2 && ex.CurrentFieldIndex == 2 && ex.CurrentPosition == 22)
			    {
			        Assert.Pass();
			    }
			    else
			    {
			        Assert.Fail();
			    }
			}
		}

		[Test()]
		public void MissingFieldQuotedTest4()
		{
			const string Data = "a,b,c,d\n1,1,1,1\n2,\"2\",\n\"3\",3,3,3";

			try
			{
				using (CsvReader csv = new CsvReader(new StringReader(Data), false, 11))
				{
					while (csv.ReadNextRecord())
						for (int i = 0; i < csv.FieldCount; i++)
						{
							string s = csv[i];
						}
				}
			}
			catch (MissingFieldCsvException ex)
			{
                if (ex.CurrentRecordIndex == 2 && ex.CurrentFieldIndex == 2 && ex.CurrentPosition == 1)
                {
                    Assert.Pass();
                }
                else
                {
                    Assert.Fail();
                }
            }
        }

		[Test()]
		public void MissingDelimiterAfterQuotedFieldTest1()
		{
			const string Data = "\"111\",\"222\"\"333\"";

			try
			{
				using (CsvReader csv = new CsvReader(new StringReader(Data), false, ',', '"', '\\', '#', ValueTrimmingOptions.UnquotedOnly))
				{
					while (csv.ReadNextRecord())
						for (int i = 0; i < csv.FieldCount; i++)
						{
							string s = csv[i];
						}
				}
			}
			catch (MalformedCsvException ex)
			{
                if (ex.CurrentRecordIndex == 0 && ex.CurrentFieldIndex == 1 && ex.CurrentPosition == 11)
                {
                    Assert.Pass();
                }
                else
                {
                    Assert.Fail();
                }
            }
        }

		[Test()]
		public void MissingDelimiterAfterQuotedFieldTest2()
		{
			const string Data = "\"111\",\"222\",\"333\"\n\"111\",\"222\"\"333\"";

			try
			{
				using (CsvReader csv = new CsvReader(new StringReader(Data), false, ',', '"', '\\', '#', ValueTrimmingOptions.UnquotedOnly))
				{
					while (csv.ReadNextRecord())
						for (int i = 0; i < csv.FieldCount; i++)
						{
							string s = csv[i];
						}
				}
			}
			catch (MalformedCsvException ex)
			{
                if (ex.CurrentRecordIndex == 1 && ex.CurrentFieldIndex == 1 && ex.CurrentPosition == 29)
                {
                    Assert.Pass();
                }
                else
                {
                    Assert.Fail();
                }
            }
        }

		[Test()]
		public void MissingDelimiterAfterQuotedFieldTestRaiseEvent()
		{
			// arrange
			const string Data = "\"111\",\"222\",\"333\"\n\"111\",\"222\"\"333\"";
            var actualErrors = new List<ParseErrorEventArgs>();
            var expectedErrors = new List<ParseErrorEventArgs>
            {
                new ParseErrorEventArgs(
                    new MalformedCsvException(
						"\"111\",\"222\",\"333\"\n\"111\",\"222\"\"333\"",
						29, 1, 1), ParseErrorAction.RaiseEvent),
                new ParseErrorEventArgs(
                    new MalformedCsvException(
						string.Empty,
                        34, 1, 2), ParseErrorAction.RaiseEvent),
            };

			// act
			using (CsvReader csv = new CsvReader(new StringReader(Data), false, ',', '"', '\\', '#', ValueTrimmingOptions.UnquotedOnly))
            {
                csv.ParseError += (sender, args) =>
                {
                    actualErrors.Add(args);
                };
				while (csv.ReadNextRecord())
					for (int i = 0; i < csv.FieldCount; i++)
					{
						string s = csv[i];
					}
			}

			// assert
            actualErrors.Should().BeEquivalentTo(expectedErrors, options => options
                .Including(x => x.Error.RawData)
                .Including(x => x.Error.CurrentFieldIndex)
                .Including(x => x.Error.CurrentRecordIndex)
                .Including(x => x.Error.CurrentPosition)
                .Including(x => x.Error.Message)
            );
        }

		[Test()]
		public void MoreFieldsTest_AdvanceToNextLine()
		{
			const string Data = "ORIGIN,DESTINATION\nPHL,FLL,kjhkj kjhkjh,eg,fhgf\nNYC,LAX";

			using (CsvReader csv = new CsvReader(new System.IO.StringReader(Data), false))
			{
				csv.SupportsMultiline = false;
                csv.DefaultParseErrorAction = ParseErrorAction.AdvanceToNextLine;

				while (csv.ReadNextRecord())
				{
					for (int i = 0; i < csv.FieldCount; i++)
					{
						string s = csv[i];
					}
				}
			}
		}

        [Test()]
        public void MoreFieldsTest_RaiseEvent()
        {
            const string Data = "ORIGIN,DESTINATION\nPHL,FLL,kjhkj kjhkjh,eg,fhgf\nNYC,LAX";

            using (CsvReader csv = new CsvReader(new System.IO.StringReader(Data), false))
            {
                bool sawError = false;
                csv.SupportsMultiline = false;
                csv.DefaultParseErrorAction = ParseErrorAction.RaiseEvent;
                csv.ParseError += (obj, args) => sawError = true;
                while (csv.ReadNextRecord())
                {
                    for (int i = 0; i < csv.FieldCount; i++)
                    {
                        string s = csv[i];
                    }
                }

                Assert.That(sawError, Is.True);
            }
        }

        [Test]
        public void MoreFieldsTest_ThrowsException()
        {
            const string Data = "ORIGIN,DESTINATION\nPHL,FLL,kjhkj kjhkjh,eg,fhgf\nNYC,LAX";

            using (CsvReader csv = new CsvReader(new StringReader(Data), false))
            {
                bool sawError = false;
                csv.SupportsMultiline = false;
                csv.DefaultParseErrorAction = ParseErrorAction.ThrowException;
                try
                {
                    while (csv.ReadNextRecord())
                    {
                        for (int i = 0; i < csv.FieldCount; i++)
                        {
                            string s = csv[i];
                        }
                    }
                }
                catch (MalformedCsvException)
                {
                    Assert.Pass();
                }
            }
            Assert.Fail();
        }

		[Test()]
		public void MoreFieldsMultilineTest()
		{
			const string Data = "ORIGIN,DESTINATION\nPHL,FLL,kjhkj kjhkjh,eg,fhgf\nNYC,LAX";

			using (CsvReader csv = new CsvReader(new System.IO.StringReader(Data), false))
			{
				while (csv.ReadNextRecord())
				{
					for (int i = 0; i < csv.FieldCount; i++)
					{
						string s = csv[i];
					}
				}
			}
		}

		[Test]
		public void ParseErrorBeforeInitializeTest()
		{
			const string Data = "\"0022 - SKABELON\";\"\"Tandremstrammer\";\"\";\"0,00\";\"\"\n\"15907\";\"\"BOLT TIL 2-05-405\";\"\";\"42,50\";\"4027816159070\"\n\"19324\";\"FJEDER TIL 2-05-405\";\"\";\"14,50\";\"4027816193241\"";

			using (var csv = new CsvReader(new System.IO.StringReader(Data), false, ';'))
			{
				csv.DefaultParseErrorAction = ParseErrorAction.AdvanceToNextLine;

				Assert.That(csv.ReadNextRecord(), Is.True);

				Assert.That(csv[0], Is.EqualTo("19324"));
				Assert.That(csv[1], Is.EqualTo("FJEDER TIL 2-05-405"));
				Assert.That(csv[2], Is.EqualTo(""));
				Assert.That(csv[3], Is.EqualTo("14,50"));
				Assert.That(csv[4], Is.EqualTo("4027816193241"));

				Assert.That(csv.ReadNextRecord(), Is.False);
			}
		}

		[Test]
		public void LastFieldEmptyFollowedByMissingFieldsOnNextRecord()
		{
			const string Data = "a,b,c,d,e"
				+ "\na,b,c,d,"
				+ "\na,b,";

			using (var csv = new CsvReader(new StringReader(Data), false))
			{
				csv.MissingFieldAction = MissingFieldAction.ReplaceByNull;

				var record = new string[5];

				Assert.That(csv.ReadNextRecord(), Is.True);
				csv.CopyCurrentRecordTo(record);
				CollectionAssert.AreEqual(new string[] { "a", "b", "c", "d", "e" }, record);

				Assert.That(csv.ReadNextRecord(), Is.True);
				csv.CopyCurrentRecordTo(record);
				CollectionAssert.AreEqual(new string[] { "a", "b", "c", "d", "" }, record);

				Assert.That(csv.ReadNextRecord(), Is.True);
				csv.CopyCurrentRecordTo(record);
				CollectionAssert.AreEqual(new string[] { "a", "b", "", null, null }, record);

				Assert.That(csv.ReadNextRecord(), Is.False);
			}
		}
	}
}