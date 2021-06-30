using Iis.Utility.Csv;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Iis.UnitTests.Csv
{
    public class CsvManagerTests
    {
        [Fact]
        public void CsvTest()
        {
            var items = GetTestData();
            var csvManager = new CsvManager(items);
            var actual = csvManager.GetCsv();
            var expected =  @"""Aaa"",""Bbb"",""Bbb__1"",""Bbb__2"",""Ccc"",""Ccc__1"",""Ddd"""+Environment.NewLine+
                            @"""Aaa1"",,,,""Ccc11"",""Ccc12"""+Environment.NewLine+
                            @"""Aaa2"",""Bbb21"",""Bbb22"",""Bbb23"""+Environment.NewLine+
                            @",,,,""Ccc31"",,""Ddd""""3"""""""+Environment.NewLine;

            Assert.Equal(expected, actual);
        }

        private List<CsvDataItem> GetTestData()
        {
            var list = new List<CsvDataItem>();
            list.Add(new CsvDataItem("1", "Aaa", "Aaa1"));
            list.Add(new CsvDataItem("1", "Ccc", "Ccc11"));
            list.Add(new CsvDataItem("1", "Ccc", "Ccc12"));
            list.Add(new CsvDataItem("2", "Aaa", "Aaa2"));
            list.Add(new CsvDataItem("2", "Bbb", "Bbb21"));
            list.Add(new CsvDataItem("2", "Bbb", "Bbb22"));
            list.Add(new CsvDataItem("2", "Bbb", "Bbb23"));
            list.Add(new CsvDataItem("3", "Ccc", "Ccc31"));
            list.Add(new CsvDataItem("3", "Ddd", "Ddd\"3\""));

            return list;
        }
    }
}
