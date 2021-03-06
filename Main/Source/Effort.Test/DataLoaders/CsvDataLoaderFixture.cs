﻿// --------------------------------------------------------------------------------------------
// <copyright file="CsvDataLoaderFixture.cs" company="Effort Team">
//     Copyright (C) Effort Team
//
//     Permission is hereby granted, free of charge, to any person obtaining a copy
//     of this software and associated documentation files (the "Software"), to deal
//     in the Software without restriction, including without limitation the rights
//     to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//     copies of the Software, and to permit persons to whom the Software is
//     furnished to do so, subject to the following conditions:
//
//     The above copyright notice and this permission notice shall be included in
//     all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//     IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//     AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//     OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//     THE SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------

namespace Effort.Test.DataLoaders
{
    using System;
    using System.Linq;
    using Effort.DataLoaders;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class CsvDataLoaderFixture
    {
        [Test]
        public void CsvDataLoader_EmptyStringConversion()
        {
            IValueConverter converter = new CsvValueConverter();

            object value = converter.ConvertValue("", typeof(string));

            value.Should().Be("");
        }

        [Test]
        public void CsvDataLoader_NullStringConversion()
        {
            IValueConverter converter = new CsvValueConverter();

            object value = converter.ConvertValue(null, typeof(string));

            value.Should().BeNull();
        }

        [Test]
        public void CsvDataLoader_LineFeedStringConversion()
        {
            IValueConverter converter = new CsvValueConverter();

            object value = converter.ConvertValue(@"\n", typeof(string));

            value.Should().Be("\n");
        }

        [Test]
        public void CsvDataLoader_CarrageReturnStringConversion()
        {
            IValueConverter converter = new CsvValueConverter();

            object value = converter.ConvertValue(@"\r", typeof(string));

            value.Should().Be("\r");
        }

        [Test]
        public void CsvDataLoader_EscapeStringConversion()
        {
            IValueConverter converter = new CsvValueConverter();

            object value = converter.ConvertValue(@"\\", typeof(string));

            value.Should().Be("\\");
        }

        [Test]
        [ExpectedException(typeof(FormatException))]
        public void CsvDataLoader_InvalidStringConversion()
        {
            IValueConverter converter = new CsvValueConverter();

           converter.ConvertValue(@"\k", typeof(string));
        }

        [Test]
        public void CsvDataLoader_TimespanConversion()
        {
            IValueConverter converter = new CsvValueConverter();

            object value = converter.ConvertValue("3:00:00", typeof(TimeSpan));

            value.Should().Be(TimeSpan.FromHours(3));
        }

        [Test]
        public void CsvDataLoader_DateTimeOffsetConversion()
        {
            IValueConverter converter = new CsvValueConverter();

            object value = 
                converter.ConvertValue("03/05/2013 03:00:00 +01:00", typeof(DateTimeOffset));

            value.Should().Be(
                    new DateTimeOffset(
                        new DateTime(2013, 3, 5, 3, 0, 0),
                        new TimeSpan(1, 0, 0)));
        }

        [Test]
        public void CsvDataLoader_GuidConversion()
        {
            IValueConverter converter = new CsvValueConverter();

            object value = 
                converter.ConvertValue("00000000-0000-0000-0000-000000000000", typeof(Guid));

            value.Should().Be(new Guid());
        }

        [Test]
        public void CsvDataLoader_EmbeddedResource()
        {
            var loader = new CsvDataLoader("res://Effort.Test/Internal/Resources/");
            var factory = loader.CreateTableDataLoaderFactory();
            var tableLoader = factory.CreateTableDataLoader(
                new TableDescription(
                    "Foo",
                    new[]
                    {
                        new ColumnDescription("Id", typeof(int)),
                        new ColumnDescription("Value", typeof(string))
                    }));

            var data = tableLoader.GetData().Single();

            data[0].Should().Be(1);
            data[1].Should().Be("Foo");
        }

        [Test]
        public void CsvDataLoader_EmbeddedResource_NotExisting()
        {
            var loader = new CsvDataLoader("res://Effort.Test/Internal/Resources/");
            var factory = loader.CreateTableDataLoaderFactory();
            var tableLoader = factory.CreateTableDataLoader(
                new TableDescription(
                    "DoesNotExist",
                    new[]
                    {
                        new ColumnDescription("Id", typeof(int)),
                    }));

            var data = tableLoader.GetData().ToList();

            data.Should().HaveCount(0);
        }

        [Test]
        public void CsvDataLoader_EmbeddedResource_NotExistingDirectory()
        {
            var loader = new CsvDataLoader("res://Effort.Test/Internal/NonExisting");
            Exception expected = null;

            try
            {
                loader.CreateTableDataLoaderFactory();
            }
            catch (Exception ex) { expected = ex; }

            expected.Should().BeOfType<ArgumentException>();
        }
    }
}
