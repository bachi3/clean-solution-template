using CleanSolutionTemplate.Application.Common.Services;
using CleanSolutionTemplate.Application.Common.Wrappers;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CleanSolutionTemplate.Infrastructure.Tests.Unit;

public class TestBase
{
    private const string TestUserEmail = "test-user-email";

    protected const string TestUserId = "test-user-id";

    private readonly IServiceCollection _services = new ServiceCollection();
    private readonly IServiceProvider _provider;

    protected TestBase()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        this._services.AddInfrastructureServices(configuration);
        this._services.AddLogging();

        this.AddPresentationServiceMocks();
        this.AddApplicationServiceMocks();

        this.SetupWrapperMocks();

        this._provider = this._services.BuildServiceProvider();
    }

    protected DateTimeOffset UtcNow { get; } = DateTimeOffset.UtcNow;

    protected T FindService<T>()
        where T : notnull
    {
        return this._provider.GetRequiredService<T>();
    }

    private void AddPresentationServiceMocks()
    {
        var userServiceMock = new Mock<IUserService>();
        userServiceMock.Setup(us => us.GetCurrentUserId()).Returns(TestUserId);
        userServiceMock.Setup(us => us.GetCurrentUserEmail()).Returns(TestUserEmail);
        this._services.AddTransient(_ => userServiceMock.Object);
    }

    private void AddApplicationServiceMocks()
    {
        var mediatorMock = new Mock<IMediator>();
        this._services.AddTransient(_ => mediatorMock.Object);
    }

    private void SetupWrapperMocks()
    {
        var dateTimeOffsetWrapper = this._services.Single(s => s.ServiceType == typeof(IDateTimeOffsetWrapper));
        this._services.Remove(dateTimeOffsetWrapper);
        var dateTimeOffsetWrapperMock = new Mock<IDateTimeOffsetWrapper>();
        dateTimeOffsetWrapperMock.SetupGet(dow => dow.UtcNow).Returns(this.UtcNow);
        this._services.AddSingleton(_ => dateTimeOffsetWrapperMock.Object);
    }
}
