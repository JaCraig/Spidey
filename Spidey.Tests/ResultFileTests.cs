using NSubstitute;
using Spidey;
using Spidey.Engines;
using System;
using Xunit;

namespace Spidey.Tests
{
    public class ResultFileTests
    {
        [Fact]
        public void Constructor_SetsPropertiesCorrectly()
        {
            // Arrange
            var UrlData = new UrlData([], "ct", "fn", "fl", 200, "url");
            var FileContent = Substitute.For<FileCurator.Formats.Data.Interfaces.IGenericFile>();

            // Act
            var Result = new ResultFile("ct", UrlData, FileContent, "fn", "fl", "loc", 200);

            // Assert
            Assert.Equal("ct", Result.ContentType);
            Assert.Equal(UrlData, Result.Data);
            Assert.Equal(FileContent, Result.FileContent);
            Assert.Equal("fn", Result.FileName);
            Assert.Equal("fl", Result.FinalLocation);
            Assert.Equal("loc", Result.Location);
            Assert.Equal(200, Result.StatusCode);
        }
    }
}