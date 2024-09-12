CSV Reader
==========

The [CsvReader](https://www.nuget.org/packages/LumenWorksCsvReader2/) library is an extended version of Sébastien Lorion's [fast CSV Reader](http://www.codeproject.com/Articles/9258/A-Fast-CSV-Reader) project 
and provides fast parsing and reading of CSV files

[![NuGet](https://img.shields.io/nuget/v/LumenWorksCsvReader2?style=flat-square&label=nuget)](https://www.nuget.org/packages/LumenWorksCsvReader2/) [![Build status](https://ci.appveyor.com/api/projects/status/q4a0jwsice979tr4/branch/master?svg=true)](https://ci.appveyor.com/project/drcolombo/csvreader/branch/master)

To this end it is a straight drop-in replacement for the existing NuGet package [LumenWork.Framework.IO](https://www.nuget.org/packages/LumenWorks.Framework.IO/) and [LumenWorksCsvReader](https://www.nuget.org/packages/LumenWorksCsvReader2/), but with additional
capabilities; the other rationale for the project is that the code is not available elsewhere in a public source repository, making it difficult to extend/contribute to.

Welcome to contributions from anyone.

You can see the version history [here](RELEASE_NOTES.md).

## Build the project
* Install [Fake](https://fake.build/fake-gettingstarted.html)
* In the command line run *dotnet fake build*

## Library License

The library is available under the [MIT License](http://en.wikipedia.org/wiki/MIT_License), for more information see the [License file][1] in the GitHub repository.

 [1]: https://github.com/phatcher/CsvReader/blob/master/License.md

## Getting Started
A good starting point is to look at Sébastien's [article](http://www.codeproject.com/Articles/9258/A-Fast-CSV-Reader) on Code Project.

A basic use of the reader something like this...
```csharp
    using System.IO;
    using LumenWorks.Framework.IO.Csv;

    void ReadCsv()
    {
        // open the file "data.csv" which is a CSV file with headers
        using (var csv = new CachedCsvReader(new StreamReader("data.csv"), true))
        {
            // Field headers will automatically be used as column names
            myDataGrid.DataSource = csv;
        }
    }
```
Having said that, there are some extensions built into this version of the library that it is worth mentioning.

## Additional Features

### Columns
One addition is the addition of a Column list which holds the names and types of the data in the CSV file. If there are no headers present, we default the column names to Column1, Column2 etc; this can be overridden by setting the DefaultColumnHeader property e.g.
```csharp
    using System.IO;
    using LumenWorks.Framework.IO.Csv;

    void ReadCsv()
    {
        // open the file "data.csv" which is a CSV file with headers
        using (var csv = new CachedCsvReader(new StreamReader("data.csv"), false))
        {
            csv.DefaultColumnHeader = "Fred"

            // Field headers will now be Fred1, Fred2, etc
            myDataGrid.DataSource = csv;
        }
    }
```

You can specify the columns yourself if there are none, and also specify the expected type; this is especially important when using against SqlBulkCopy which we will come back to later.
```csharp
    using System.IO;
    using LumenWorks.Framework.IO.Csv;

    void ReadCsv()
    {
        // open the file "data.csv" which is a CSV file with headers
        using (var csv = new CachedCsvReader(new StreamReader("data.csv"), false))
        {
            csv.Columns.Add(new Column { Name = "PriceDate", Type = typeof(DateTime) });
            csv.Columns.Add(new Column { Name = "OpenPrice", Type = typeof(decimal) });
            csv.Columns.Add(new Column { Name = "HighPrice", Type = typeof(decimal) });
            csv.Columns.Add(new Column { Name = "LowPrice", Type = typeof(decimal) });
            csv.Columns.Add(new Column { Name = "ClosePrice", Type = typeof(decimal) });
            csv.Columns.Add(new Column { Name = "Volume", Type = typeof(int) });

            // Field headers will now be picked from the Columns collection
            myDataGrid.DataSource = csv;
        }
    }
```

### SQL Bulk Copy
One use of CSV Reader is to have a nice .NET way of using SQL Bulk Copy (SBC) rather than bcp for bulk loading of data into SQL Server.

A couple of issues arise when using SBC
	1. SBC wants the data presented as the correct type rather than as string
	2. You need to map between the table destination columns and the CSV if the order does not match *exactly*
	
Below is a example using the Columns collection to set up the correct metadata for SBC
```csharp
	public void Import(string fileName, string connectionString)
	{
		using (var reader = new CsvReader(new StreamReader(fileName), false))
		{
			reader.Columns = new List<LumenWorks.Framework.IO.Csv.Column>
			{
				new LumenWorks.Framework.IO.Csv.Column { Name = "PriceDate", Type = typeof(DateTime) },
				new LumenWorks.Framework.IO.Csv.Column { Name = "OpenPrice", Type = typeof(decimal) },
				new LumenWorks.Framework.IO.Csv.Column { Name = "HighPrice", Type = typeof(decimal) },
				new LumenWorks.Framework.IO.Csv.Column { Name = "LowPrice", Type = typeof(decimal) },
				new LumenWorks.Framework.IO.Csv.Column { Name = "ClosePrice", Type = typeof(decimal) },
				new LumenWorks.Framework.IO.Csv.Column { Name = "Volume", Type = typeof(int) },
				new LumenWorks.Framework.IO.Csv.Column { Name = "IsActive", Type = typeof(bool) },
			};

			// With the help of CustomBooleanReplacer you can define a mapping between string values in the CSV file and boolean values
			// In this example, 'Y' and 'Yes' will be treated as true; 'N' and 'No' - as false value.
			reader.CustomBooleanReplacer = new Dictionary<string, bool>
			{
				{"Y", true},
				{"N", false},
				{"Yes", true},
				{"No", false},
			};

			// Now use SQL Bulk Copy to move the data
			using (var sbc = new SqlBulkCopy(connectionString))
			{
				sbc.DestinationTableName = "dbo.DailyPrice";
				sbc.BatchSize = 1000;

				sbc.AddColumnMapping("PriceDate", "PriceDate");
				sbc.AddColumnMapping("OpenPrice", "OpenPrice");
				sbc.AddColumnMapping("HighPrice", "HighPrice");
				sbc.AddColumnMapping("LowPrice", "LowPrice");
				sbc.AddColumnMapping("ClosePrice", "ClosePrice");
				sbc.AddColumnMapping("Volume", "Volume");
				sbc.AddColumnMapping("IsActive", "IsActive");

				sbc.WriteToServer(reader);
			}
		}
	}
```
The method AddColumnMapping is an extension I wrote to simplify adding mappings to SBC
```csharp
	public static class SqlBulkCopyExtensions
	{
		public static SqlBulkCopyColumnMapping AddColumnMapping(this SqlBulkCopy sbc, int sourceColumnOrdinal, int targetColumnOrdinal)
		{
			var map = new SqlBulkCopyColumnMapping(sourceColumnOrdinal, targetColumnOrdinal);
			sbc.ColumnMappings.Add(map);

			return map;
		}

		public static SqlBulkCopyColumnMapping AddColumnMapping(this SqlBulkCopy sbc, string sourceColumn, string targetColumn)
		{
			var map = new SqlBulkCopyColumnMapping(sourceColumn, targetColumn);
			sbc.ColumnMappings.Add(map);

			return map;
		}
	}
```	
One other issue recently arose where we wanted to use SBC but some of the data was not in the file itself, but metadata that needed to be included on every row. The solution was to amend the CSV reader and Columns collection to allow default values to be provided that are not in the data.

The additional columns should be added at the end of the Columns collection to avoid interfering with the parsing, see the amended example below...
```csharp
	public void Import(string fileName, string connectionString)
	{
		using (var reader = new CsvReader(new StreamReader(fileName), false))
		{
			reader.Columns = new List<LumenWorks.Framework.IO.Csv.Column>
			{
				...
				new LumenWorks.Framework.IO.Csv.Column { Name = "Volume", Type = typeof(int) },
				// NB Fake column so bulk import works
				new LumenWorks.Framework.IO.Csv.Column { Name = "Ticker", Type = typeof(string) },
			};

			// Fix up the column defaults with the values we need
			reader.UseColumnDefaults = true;
			reader.Columns[reader.GetFieldIndex("Ticker")] = Path.GetFileNameWithoutExtension(fileName);

			// Now use SQL Bulk Copy to move the data
			using (var sbc = new SqlBulkCopy(connectionString))
			{
				...
				sbc.AddColumnMapping("Ticker", "Ticker");

				sbc.WriteToServer(reader);
			}
		}
	}
```

#### VirtualColumns
It may happen that your database table where you would like to import a CSV contains more or different columns than your CSV file.
As SqlBulkCopy requires to define all column mappings from the target table, you can use the VirtualColumns functionality:
```csharp
    csv.VirtualColumns.Add(new Column { Name = "SourceTypeId", Type = typeof(int), DefaultValue = "1", NumberStyles = NumberStyles.Integer });
    csv.VirtualColumns.Add(new Column { Name = "DataBatchId", Type = typeof(int), DefaultValue = dataBatchId.ToString(), NumberStyles = NumberStyles.Integer });
```
In this case you define 2 additional columns that do not exist in the source CSV file, but exist in the target table. Also you can set the DefaultValue that will be bulk-copied to the target table together with the CSV file content. Do not forget to include the defined virtual columns to the SqlBulkCopy column mapping!

#### ExcludeFilter
In case if your CSV file is big enough and you do not want to import a whole file but some set of data, you can set the ExcludeFilter action:
```csharp
csv.ExcludeFilter = () => ((csv["Fmly"] ?? "") + (csv["Group"] ?? "") + (csv["Type"] ?? "")).ToUpperInvariant() == "EQDEQUIT";
```
In this case all rows that fit the defined criteria will not be imported to the database.

#### MapDataToDto<T>
Calling this method returns you an IEnumerable<T> where T is the type of an entity/DTO you want to map your CSV file.
Before calling this method you should define Columns passing names and data type of all columns within CSV file.
```
    var expected = new List<SampleData3>
    {
        new SampleData3("John", "Doe", "120 jefferson st.", "Riverside", "NJ", 8075, true, null),
        new SampleData3("Jack", "McGinnis", "220 hobo Av.", "Phila", "PA", 9119, false, null),
        new SampleData3("John \"Da Man\"", "Repici", "120 Jefferson St.", "Riverside", "NJ", 8075, false, null),
        new SampleData3("Stephen", "Tyler", "7452 Terrace \"At the Plaza\" road", "SomeTown", "SD", 91234, false, null),
        new SampleData3(null, "Blankman", null, "SomeTown", "SD", 298, false, null),
        new SampleData3("Joan \"the bone\", Anne", "Jet", "9th, at Terrace plc", "Desert City", "CO", 123, false, null),
    };
	/// using propertyToColumnMapping parameter you can map column names from CSV file to property names of your entity/DTO
    var propertyToColumnMapping = new Dictionary<string, string>
    {
        { "FirstName", "First Name" },
        { "LastName", "Last Name" },
        { "ZipCode", "Zip Code" }
    };
	using (CsvReader csv = new CsvReader(new StringReader(CsvReaderSampleData.SampleData1), true))
	{
		/// you should define Columns before calling MapDataToDto<T>!
        csv.Columns = new ColumnCollection
        {
            {"First Name", typeof(string)},
            {"Last Name", typeof(string)},
            {"Address", typeof(string)},
            {"City", typeof(string)},
            {"State", typeof(string)},
            {"Zip Code", typeof(int)},
            {"IsActive", typeof(bool)},
        };
        csv.CustomBooleanReplacer = new Dictionary<string, bool> { { "Y", true }, { "N", false } };
        var result = csv.MapDataToDto<SampleData3>(propertyToColumnMapping).ToList();
        result.Should().BeEquivalentTo(expected);
	}
```

## Performance
To give an idea of performance, this took a native sample app using an ORM from 2m 27s to 1.37s using SBC and the full import took just over 11m to import 9.8m records.

One of the main reasons for using this library is its excellent performance on reading/parsing raw data, here's a recent run of the benchmark (which is in the source)

|Test|.NET 4.8|.NET 6|.NET 7|.NET 8|
|---|---|---|---|---|
Test pass #1 - All fields|
CsvReader - No cache   |62.7333|80.1117|63.8059|37.0477
CachedCsvReader - Run 1|35.5160|41.7880|38.2042|39.8874
CachedCsvReader - Run 2|61426.7765|73999.3273|117302.0528|91973.2441
TextFieldParser        |9.8574|13.9199|14.6264|16.4390
Regex                  |10.2663|17.9696|18.1544|20.7676
||||||
Test pass #1 - Field #72 (middle)|
CsvReader - No cache   |67.7263|95.0044|86.0328|87.4732
CachedCsvReader - Run 1|29.2253|36.9564|42.5468|44.7421
CachedCsvReader - Run 2|792792.7928|646108.6637|512820.5128|639534.8837
TextFieldParser        |9.9626|13.1838|14.5921|18.2339
Regex                  |22.5060|37.9651|46.1343|50.7253
||||||
Test pass #2 - All fields|
CsvReader - No cache   |75.8756|88.6553|93.0768|109.0782
CachedCsvReader - Run 1|28.8864|38.4666|45.3834|40.4266
CachedCsvReader - Run 2|948275.8621|781527.5311|852713.1783|461699.8951
TextFieldParser        |9.4858|13.8782|15.3555|17.4682
Regex                  |9.6566|18.4976|20.6475|22.6479
||||||
Test pass #2 - Field #72 (middle)|
CsvReader - No cache   |72.6275|98.7495|107.8248|111.1179
CachedCsvReader - Run 1|28.4391|35.7626|36.8091|52.7800
CachedCsvReader - Run 2|830188.6792|765217.3913|827067.6692|995475.1131
TextFieldParser        |8.6734|14.6872|15.4038|18.2108
Regex                  |22.1135|44.0567|46.4395|50.7668
||||||
Test pass #3 - All fields|
CsvReader - No cache   |74.3428|90.2397|92.8137|111.8334
CachedCsvReader - Run 1|30.5301|35.6446|43.8796|49.9862
CachedCsvReader - Run 2|817843.8662|737018.4255|766550.5226|820895.5224
TextFieldParser        |9.3366|14.4030|15.0148|17.9641
Regex                  |10.1904|19.1660|20.0524|21.7854
||||||
Test pass #3 - Field #72 (middle)|
CsvReader - No cache   |76.5840|104.5209|105.9584|113.8155
CachedCsvReader - Run 1|35.5272|38.0744|43.4385|37.0724
CachedCsvReader - Run 2|932203.3898|766550.5226|634005.7637|748299.3197
TextFieldParser        |9.7928|14.4643|13.6437|17.7131
Regex                  |22.5506|44.6435|45.5831|49.3559



|Average of all test passes|.NET 4.8|.NET Core 3.1|.NET 5|.NET 6|
|---|---|---|---|---|---|
CsvReader - No cache   |716.483|928.803|915.854|950.610
CachedCsvReader - Run 1|313.540|377.821|417.103|441.491
CachedCsvReader - Run 2|7.304.552.278|6.284.036.436|6.184.099.499|6.263.129.964
TextFieldParser        |95.181|140.894|147.727|176.715
Regex                  |162.139|303.831|328.352|360.082

As you can see, an average performance slightly increases from full .NET Framework 4.8 to .NET Core 8.
![Performance Chart](AveragePerformanceChart.png)
This was run on a Core i5-8400 (6 cores), 32Gb RAM and 2Tb SSD.
