using Microsoft.Extensions.Logging;
using NSubstitute;
using Topica.Contracts;
using Topica.Executors;

namespace Topica.Unit.Tests.Core.Executors;

[Trait("Category", "Unit")]
public class MessageHandlerExecutorTests
{
    private readonly IHandlerResolver _handlerResolver;

    private readonly MessageHandlerExecutor _executor;

    public MessageHandlerExecutorTests()
    {
        _handlerResolver = Substitute.For<IHandlerResolver>();
        var loggerFactory = Substitute.For<ILoggerFactory>();
        
        _executor = new MessageHandlerExecutor(_handlerResolver, loggerFactory);
    }

    [Fact]
    public async Task ExecuteHandlerAsync_Returns_Null_When_Handler_Not_Found()
    {
        _handlerResolver.ResolveHandler("") .Returns((false, new object(), false, null));
        
        var result = await _executor.ExecuteHandlerAsync("");
        
        Assert.Null(result.Item1);
        Assert.False(result.Item2);
    }
    
    [Fact]
    public async Task ExecuteHandlerAsync_Returns_Null_When_Method_To_Execute_Null()
    {
        _handlerResolver.ResolveHandler("") .Returns((true, new object(), false, null));
        
        var result = await _executor.ExecuteHandlerAsync("");
        
        Assert.Null(result.Item1);
        Assert.False(result.Item2);
    }
    
    [Fact]
    public async Task ExecuteHandlerAsync_Returns_HandlerName_With_validation_False_When_Validation_Fails()
    {
        var handlerImplementationFake = new MessageHandlerExecutorTests();
        _handlerResolver.ResolveHandler("") .Returns((true, handlerImplementationFake, false, new object()));
        
        var result = await _executor.ExecuteHandlerAsync("");
        
        Assert.NotNull(result.Item1);
        Assert.Equal(handlerImplementationFake.GetType().Name, result.Item1);
        Assert.False(result.Item2);
    }
    
    [Fact]
    public async Task ExecuteHandlerAsync_Returns_HandlerName_With_validation_True_When_Validation_Succeeds()
    {
        var handlerImplementationFake = new MessageHandlerExecutorTests();
        _handlerResolver.ResolveHandler("") .Returns((true, handlerImplementationFake, true, Task.FromResult(true)));
        
        var result = await _executor.ExecuteHandlerAsync("");
        
        Assert.NotNull(result.Item1);
        Assert.Equal(handlerImplementationFake.GetType().Name, result.Item1);
        Assert.True(result.Item2);
    }
}