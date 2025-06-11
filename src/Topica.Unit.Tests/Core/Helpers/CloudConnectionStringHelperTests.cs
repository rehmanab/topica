using Topica.Helpers;

namespace Topica.Unit.Tests.Core.Helpers;

public class CloudConnectionStringHelperTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("InvalidConnectionString")]
    [InlineData("Endpoint=;AccountName=;AccountKey=;")]
    public void ParseEndpointCloudConnectionString_Null_For_Strings(string? source)
    {
        var result = CloudConnectionStringHelper.ParseEndpointCloudConnectionString(source);

        Assert.Null(result);
    }
    
    [Theory]
    [InlineData("Endpoint=https://example.com;AccountName=test;AccountKey=key123", "https://example.com")]
    [InlineData("Endpoint=https://example.com/;AccountName=test;AccountKey=key123", "https://example.com")]
    [InlineData("Endpoint=https://example.com/////;AccountName=test;AccountKey=key123", "https://example.com")]
    [InlineData("Endpoint=http://localhost:10000/devstoreaccount1/;AccountName=test", "http://localhost:10000/devstoreaccount1")]
    public void ParseEndpointCloudConnectionString_Returns_Trimmed_Endpoint(string source, string expected)
    {
        var result = CloudConnectionStringHelper.ParseEndpointCloudConnectionString(source);
        
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("AccountName=test;Endpoint=https://example.com;AccountKey=key123", "https://example.com")]
    [InlineData("AccountKey=key123;AccountName=test;Endpoint=https://example.com/", "https://example.com")]
    public void ParseEndpointCloudConnectionString_Works_With_Different_Key_Order(string source, string expected)
    {
        var result = CloudConnectionStringHelper.ParseEndpointCloudConnectionString(source);
        
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("Endpoint=https://example.com", "https://example.com")]
    [InlineData("Endpoint=https://example.com;", "https://example.com")]
    [InlineData("Endpoint=dockerhost;", "dockerhost")]
    public void ParseEndpointCloudConnectionString_Works_With_Only_Endpoint(string source, string expected)
    {
        var result = CloudConnectionStringHelper.ParseEndpointCloudConnectionString(source);
        
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("endpoint=https://example.com;AccountName=test", "https://example.com")]
    [InlineData("ENDPOINT=https://example.com;AccountName=test", "https://example.com")]
    [InlineData("EndPoint=https://example.com;AccountName=test", "https://example.com")]
    public void ParseEndpointCloudConnectionString_Is_Case_InSensitive(string source, string? expected)
    {
        var result = CloudConnectionStringHelper.ParseEndpointCloudConnectionString(source);
        
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("AccountName=test;AccountKey=key123")]
    [InlineData("Key1=Value1;Key2=Value2")]
    [InlineData("=Value1;Key2=")]
    [InlineData(";;;")]
    public void ParseEndpointCloudConnectionString_Returns_Null_When_No_Endpoint(string source)
    {
        var result = CloudConnectionStringHelper.ParseEndpointCloudConnectionString(source);
        
        Assert.Null(result);
    }
    
    [Theory]
    [InlineData("Endpoint=https://example.com;Endpoint=https://another.com", "https://example.com")]
    public void ParseEndpointCloudConnectionString_Uses_First_Endpoint_When_Duplicates_Exist(string source, string expected)
    {
        var result = CloudConnectionStringHelper.ParseEndpointCloudConnectionString(source);
        
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("Endpoint=https://example.com/path with spaces;AccountName=test", "https://example.com/path with spaces")]
    [InlineData("Endpoint=https://example.com/path?query=value&other=123;AccountName=test", "https://example.com/path?query=value&other=123")]
    [InlineData("Endpoint=https://user:pass@example.com:8080/path;AccountName=test", "https://user:pass@example.com:8080/path")]
    public void ParseEndpointCloudConnectionString_Handles_Complex_Urls(string source, string expected)
    {
        var result = CloudConnectionStringHelper.ParseEndpointCloudConnectionString(source);
        
        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData("Endpoint=https://example.com;AccountName=test;Key With Spaces=value", "https://example.com")]
    [InlineData("Endpoint=https://example.com;Account;Name=test;AccountKey=key123", "https://example.com")]
    [InlineData("Endpoint=https://example.com;Key==Value;AccountKey=key123", "https://example.com")]
    public void ParseEndpointCloudConnectionString_Handles_Special_Cases(string source, string expected)
    {
        var result = CloudConnectionStringHelper.ParseEndpointCloudConnectionString(source);
        
        Assert.Equal(expected, result);
    }
}
