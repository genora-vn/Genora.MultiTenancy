using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg;
using Genora.MultiTenancy.AppDtos.HoaLinh;
using Genora.MultiTenancy.AppHlg;
using Genora.MultiTenancy.AppServices.Hlg;
using Genora.MultiTenancy.AppServices.HoaLinh;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.DomainModels.AppHlg;
using Genora.MultiTenancy.DomainModels.AppHlPoints;
using Genora.MultiTenancy.Enums.Hlg;
using Genora.MultiTenancy.Hlg;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Linq;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;
using Xunit;

namespace Genora.MultiTenancy.Hlg;

public class HlgPharmacyRegistrationTests : IDisposable
{
    public const string OwnerPhone = "0900000001";
    private readonly List<Customer> _customers = new();
    private readonly List<HlgUserProfile> _profiles = new();
    private readonly ICurrentTenant _tenant = Substitute.For<ICurrentTenant>();
    private readonly IHlApiClientService _dms = Substitute.For<IHlApiClientService>();
    private readonly IHlgRegistrationLock _registrationLock = Substitute.For<IHlgRegistrationLock>();
    private readonly IUnitOfWorkManager _uow = Substitute.For<IUnitOfWorkManager>();
    private readonly IUnitOfWork _transaction = Substitute.For<IUnitOfWork>();
    private readonly ServiceProvider _provider;
    private readonly HlgProfileAppService _service;

    public HlgPharmacyRegistrationTests()
    {
        _tenant.Id.Returns(Guid.NewGuid());
        _uow.Begin(Arg.Any<AbpUnitOfWorkOptions>(), Arg.Any<bool>()).Returns(_transaction);
        var guid = Substitute.For<IGuidGenerator>();
        guid.Create().Returns(_ => Guid.NewGuid());
        _provider = new ServiceCollection().AddSingleton(guid)
            .AddSingleton(Substitute.For<IDataFilter>())
            .AddSingleton<IAsyncQueryableExecuter>(new AsyncQueryableExecuter(Array.Empty<IAsyncQueryableProvider>()))
            .BuildServiceProvider();
        _service = new HlgProfileAppService(Repo(_customers), Repo(_profiles),
            Repo(new List<HlPointTransaction>()), Repo(new List<HlgLearningProgress>()),
            Repo(new List<HlgGameSession>()), Repo(new List<HlgProduct>()), Repo(new List<HlgRewardHistory>()),
            Substitute.For<IHlgGameAppService>(),
            _tenant, NullLogger<HlgProfileAppService>.Instance, new ConfigurationBuilder().Build(), _dms, _registrationLock, _uow)
        { LazyServiceProvider = new AbpLazyServiceProvider(_provider) };
        _dms.GetCustomerByPhoneAsync(OwnerPhone).Returns(HlApiResult<List<HlCustomerDto>>.Ok(new()
        {
            Branch("DMS01", false), Branch("DMS02", true)
        }));
    }

    private static HlCustomerDto Branch(string code, bool gkhl) => new()
    { CustCode = code, CustName = "Test branch", Address = "Test address", CustPhone = OwnerPhone, Phone = OwnerPhone, IsCustomer = true, IsGkhl = gkhl };

    private static IRepository<T, Guid> Repo<T>(List<T> rows) where T : class, IEntity<Guid>
    {
        var repo = Substitute.For<IRepository<T, Guid>>();
        repo.GetQueryableAsync().Returns(_ => rows.AsQueryable());
        // AnyAsync/FirstOrDefaultAsync are ABP extension methods; execute their real predicates.
        repo.AsyncExecuter.Returns(new AsyncQueryableExecuter(Array.Empty<IAsyncQueryableProvider>()));
        repo.InsertAsync(Arg.Any<T>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(c => { var row = c.Arg<T>(); rows.Add(row); return row; });
        repo.UpdateAsync(Arg.Any<T>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(c => c.Arg<T>());
        return repo;
    }

    private HlgCustomerUpsertPayloadDto Payload(string phone = OwnerPhone) => new()
    { Phone = phone, PharmaPhone = OwnerPhone, CustomerCode = "DMS02", FullName = "Test member", CustomerType = "pharmacy", Address = "Selected branch address" };

    private Customer AddCustomer(string phone, bool linked = false, Guid? tenant = null)
    {
        var customer = new Customer(Guid.NewGuid(), phone, "Existing")
        { TenantId = tenant ?? _tenant.Id, CustomerCode = "LOCAL" + _customers.Count, BonusPoint = 100, BonusAmount = 200 };
        _customers.Add(customer);
        if (linked) _profiles.Add(new HlgUserProfile(Guid.NewGuid(), customer.Id, customer.TenantId)
        { PharmaPhone = OwnerPhone, IsRegistered = true, CustomerType = HlgCustomerType.Pharmacy });
        return customer;
    }

    [Theory]
    [InlineData(OwnerPhone)]
    [InlineData("+84900000001")]
    [InlineData("84900000001")]
    [InlineData("090 000-0001")]
    public async Task Owner_Preflight_Lists_All_Branches_Including_Non_Gkhl_Without_Writing(string phone)
    {
        var result = await _service.CheckCustomerAsync(phone);
        result.Branches.Count.ShouldBe(2);
        result.Phone.ShouldBe(OwnerPhone);
        result.PharmaPhone.ShouldBe(OwnerPhone);
        result.IsOwner.ShouldBeTrue(); result.CanRegister.ShouldBeTrue();
        _customers.ShouldBeEmpty(); _profiles.ShouldBeEmpty();
        _registrationLock.ReceivedCalls().ShouldBeEmpty();
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("123")]
    [InlineData("")]
    public async Task Invalid_Phones_Fail_Before_Dms(string phone)
    {
        var ex = await Should.ThrowAsync<UserFriendlyException>(() => _service.CheckCustomerAsync(phone));
        ex.Code.ShouldBe("HlgRegistration:InvalidPhone");
        _dms.ReceivedCalls().ShouldBeEmpty();
    }

    [Fact]
    public async Task Missing_Dms_Customer_Blocks_Both_Preflight_And_Direct_Upsert()
    {
        _dms.GetCustomerByPhoneAsync(OwnerPhone).Returns(HlApiResult<List<HlCustomerDto>>.Ok(new()));
        (await Should.ThrowAsync<UserFriendlyException>(() => _service.CheckCustomerAsync(OwnerPhone))).Message.ShouldBe(HlgRegistrationRules.DmsCustomerNotFound);
        (await Should.ThrowAsync<UserFriendlyException>(() => _service.UpsertCustomerAsync(Payload()))).Message.ShouldBe(HlgRegistrationRules.DmsCustomerNotFound);
        _customers.ShouldBeEmpty(); _profiles.ShouldBeEmpty();
    }

    [Fact]
    public async Task Dms_Failure_Is_Not_Reported_As_Customer_Not_Found_And_Does_Not_Register()
    {
        _dms.GetCustomerByPhoneAsync(OwnerPhone).Returns(HlApiResult<List<HlCustomerDto>>.Fail("Partner unavailable"));
        (await Should.ThrowAsync<UserFriendlyException>(() => _service.UpsertCustomerAsync(Payload()))).Code.ShouldBe("HlgRegistration:DmsUnavailable");
        _customers.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(false, OwnerPhone)]
    [InlineData(true, "0900009999")]
    public async Task Noncustomers_And_Branches_Of_Another_Phone_Cannot_Be_Used(bool isCustomer, string phone)
    {
        var branch = Branch("DMS02", true); branch.IsCustomer = isCustomer; branch.CustPhone = branch.Phone = phone;
        _dms.GetCustomerByPhoneAsync(OwnerPhone).Returns(HlApiResult<List<HlCustomerDto>>.Ok(new() { branch }));
        (await Should.ThrowAsync<UserFriendlyException>(() => _service.UpsertCustomerAsync(Payload()))).Code.ShouldBe("HlgRegistration:DmsCustomerNotFound");
    }

    [Fact]
    public async Task Employee_Requires_Owner_Customer_In_Same_Tenant()
    {
        AddCustomer(OwnerPhone, tenant: Guid.NewGuid());
        (await Should.ThrowAsync<UserFriendlyException>(() => _service.CheckCustomerAsync("0900000002", OwnerPhone))).Message.ShouldBe(HlgRegistrationRules.OwnerRequired);
        (await Should.ThrowAsync<UserFriendlyException>(() => _service.UpsertCustomerAsync(Payload("0900000002")))).Message.ShouldBe(HlgRegistrationRules.OwnerRequired);
        _customers.Count.ShouldBe(1); _profiles.ShouldBeEmpty();
    }

    [Fact]
    public async Task Owner_Can_Register_First_With_Selected_Dms_Code_And_Address()
    {
        var result = await _service.UpsertCustomerAsync(Payload());
        result.CustomerCode.ShouldBe("DMS02"); result.DmsCustomerCode.ShouldBe("DMS02");
        result.PharmaPhone.ShouldBe(OwnerPhone); result.IsRegistered.ShouldBeTrue();
        result.Address.ShouldBe("Selected branch address");
        _customers.Count.ShouldBe(1); _profiles.Count.ShouldBe(1);
        await _registrationLock.Received(1).AcquireAsync(Arg.Any<CancellationToken>());
        await _transaction.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Employee_Uses_Owner_Dms_Phone_And_Unique_Local_Code_With_Shared_Branch()
    {
        await _service.UpsertCustomerAsync(Payload());
        var employee = await _service.UpsertCustomerAsync(Payload("0900000002"));
        employee.CustomerCode.ShouldBe("HLGKH000001"); employee.DmsCustomerCode.ShouldBe("DMS02");
        employee.PharmaPhone.ShouldBe(OwnerPhone);
        await _dms.DidNotReceive().GetCustomerByPhoneAsync("0900000002");
    }

    [Fact]
    public async Task Five_Total_Accounts_Allow_Retry_But_Reject_Sixth_Through_Both_APIs()
    {
        await _service.UpsertCustomerAsync(Payload());
        for (var i = 2; i <= 5; i++) await _service.UpsertCustomerAsync(Payload("090000000" + i));
        var check = await _service.CheckCustomerAsync("0900000005", OwnerPhone);
        check.LinkedAccountCount.ShouldBe(5); check.CanRegister.ShouldBeTrue();
        await _service.UpsertCustomerAsync(Payload("0900000005"));
        await _service.UpsertCustomerAsync(Payload());
        (await Should.ThrowAsync<UserFriendlyException>(() => _service.CheckCustomerAsync("0900000006", OwnerPhone))).Message.ShouldBe(HlgRegistrationRules.AccountLimitReached);
        (await Should.ThrowAsync<UserFriendlyException>(() => _service.UpsertCustomerAsync(Payload("0900000006")))).Message.ShouldBe(HlgRegistrationRules.AccountLimitReached);
        _customers.Count.ShouldBe(5); _profiles.Count.ShouldBe(5);
        _customers.Select(x => x.CustomerCode).Distinct().Count().ShouldBe(5);
    }

    [Fact]
    public async Task Sales_Only_Owner_Reserves_One_Slot_And_Codes_And_Balances_Are_Preserved()
    {
        var owner = AddCustomer(OwnerPhone); owner.CustomerCode = "SALES99";
        for (var i = 2; i <= 5; i++) await _service.UpsertCustomerAsync(Payload("090000000" + i));
        (await Should.ThrowAsync<UserFriendlyException>(() => _service.UpsertCustomerAsync(Payload("0900000006")))).Code.ShouldBe("HlgRegistration:AccountLimitReached");
        var result = await _service.UpsertCustomerAsync(Payload());
        result.CustomerCode.ShouldBe("SALES99"); result.DmsCustomerCode.ShouldBe("DMS02");
        owner.BonusPoint.ShouldBe(100); owner.BonusAmount.ShouldBe(200);
    }

    [Fact]
    public async Task Existing_Sales_Employee_Keeps_Its_Local_Identity()
    {
        AddCustomer(OwnerPhone);
        var employee = AddCustomer("0900000002"); employee.CustomerCode = "SALES-EMPLOYEE";
        var result = await _service.UpsertCustomerAsync(Payload(employee.PhoneNumber));
        result.CustomerCode.ShouldBe("SALES-EMPLOYEE"); result.DmsCustomerCode.ShouldBe("DMS02");
        employee.BonusPoint.ShouldBe(100); employee.BonusAmount.ShouldBe(200);
    }

    [Fact]
    public async Task Existing_Hlg_Code_Also_Remains_Stable_When_Owner_Selects_Dms_Branch()
    {
        AddCustomer(OwnerPhone).CustomerCode = "HLGKH000100";
        var result = await _service.UpsertCustomerAsync(Payload());
        result.CustomerCode.ShouldBe("HLGKH000100"); result.DmsCustomerCode.ShouldBe("DMS02");
    }

    [Fact]
    public async Task Deleted_Customer_Phones_And_Codes_Are_Not_Silently_Reused()
    {
        AddCustomer(OwnerPhone).IsDeleted = true;
        (await Should.ThrowAsync<UserFriendlyException>(() => _service.UpsertCustomerAsync(Payload()))).Code.ShouldBe("HlgRegistration:CustomerUnavailable");
        _customers.Clear();
        var previous = AddCustomer("0900000002"); previous.CustomerCode = "DMS02"; previous.IsDeleted = true;
        (await Should.ThrowAsync<UserFriendlyException>(() => _service.UpsertCustomerAsync(Payload()))).Code.ShouldBe("HlgRegistration:CustomerCodeInUse");
        _customers.Count.ShouldBe(1); _profiles.ShouldBeEmpty();
    }

    [Fact]
    public async Task Deleted_Hlg_Profile_Cannot_Be_Recreated_Over_Its_Unique_Customer_Link()
    {
        AddCustomer(OwnerPhone, linked: true);
        _profiles[0].IsDeleted = true;
        (await Should.ThrowAsync<UserFriendlyException>(() => _service.CheckCustomerAsync(OwnerPhone))).Code.ShouldBe("HlgRegistration:CustomerUnavailable");
        (await Should.ThrowAsync<UserFriendlyException>(() => _service.UpsertCustomerAsync(Payload()))).Code.ShouldBe("HlgRegistration:CustomerUnavailable");
        (await Should.ThrowAsync<UserFriendlyException>(() => _service.GetByPhoneAsync(OwnerPhone))).Code.ShouldBe("HlgRegistration:CustomerUnavailable");
        _profiles.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Employee_Code_Sequence_Includes_Deleted_Customers()
    {
        AddCustomer(OwnerPhone);
        var deleted = AddCustomer("0900000002"); deleted.CustomerCode = "HLGKH000100"; deleted.IsDeleted = true;
        (await _service.UpsertCustomerAsync(Payload("0900000003"))).CustomerCode.ShouldBe("HLGKH000101");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("FOREIGN")]
    public async Task Missing_Or_Forged_Branch_Does_Not_Write(string? branch)
    {
        var request = Payload(); request.CustomerCode = branch;
        await Should.ThrowAsync<UserFriendlyException>(() => _service.UpsertCustomerAsync(request));
        _customers.ShouldBeEmpty(); _profiles.ShouldBeEmpty();
    }

    [Fact]
    public async Task Host_Cannot_Use_Another_Tenants_Owner()
    {
        AddCustomer(OwnerPhone);
        _tenant.Id.Returns((Guid?)null);
        (await Should.ThrowAsync<UserFriendlyException>(() => _service.UpsertCustomerAsync(Payload("0900000002")))).Code.ShouldBe("HlgRegistration:OwnerRequired");
    }

    [Fact]
    public async Task Host_Owner_And_Employee_Are_Supported()
    {
        _tenant.Id.Returns((Guid?)null);
        await _service.UpsertCustomerAsync(Payload());
        await _service.UpsertCustomerAsync(Payload("0900000002"));
        _customers.All(x => x.TenantId == null).ShouldBeTrue();
        _profiles.All(x => x.TenantId == null).ShouldBeTrue();
    }

    [Fact]
    public async Task Cannot_Move_Linked_User_To_Another_Pharmacy_Or_Use_Employee_As_Owner()
    {
        AddCustomer(OwnerPhone, linked: true);
        _profiles.Single().PharmaPhone = "0900009999";
        (await Should.ThrowAsync<UserFriendlyException>(() => _service.UpsertCustomerAsync(Payload()))).Code.ShouldBe("HlgRegistration:PharmacyAlreadyLinked");
        (await Should.ThrowAsync<UserFriendlyException>(() => _service.UpsertCustomerAsync(Payload("0900000002")))).Code.ShouldBe("HlgRegistration:OwnerRequired");
    }

    [Fact]
    public async Task Existing_Legacy_PharmacyCode_Is_Preserved_When_Linking()
    {
        var owner = AddCustomer(OwnerPhone, linked: true);
        _profiles[0].PharmaPhone = null; _profiles[0].PharmacyCode = "LEGACY-CODE";
        (await _service.UpsertCustomerAsync(Payload())).PharmacyCode.ShouldBe("LEGACY-CODE");
    }

    [Fact]
    public async Task Inactive_Members_Still_Occupy_Slots_But_Other_Tenants_Do_Not()
    {
        AddCustomer(OwnerPhone);
        for (var i = 2; i <= 5; i++) AddCustomer("090000000" + i, linked: true).IsActive = false;
        AddCustomer("0900000007", linked: true, tenant: Guid.NewGuid());
        (await Should.ThrowAsync<UserFriendlyException>(() => _service.UpsertCustomerAsync(Payload("0900000006")))).Code.ShouldBe("HlgRegistration:AccountLimitReached");
    }

    [Fact]
    public async Task Linked_Phone_Cannot_Be_Changed_Through_Profile_Edit()
    {
        await _service.UpsertCustomerAsync(Payload());
        await Should.ThrowAsync<UserFriendlyException>(() => _service.UpdateProfileAsync(OwnerPhone, new() { Phone = "0900009999" }));
        _customers[0].PhoneNumber.ShouldBe(OwnerPhone);
    }

    [Fact]
    public async Task Profile_Edit_Cannot_Complete_Registration_Without_Dms_Gate()
    {
        AddCustomer(OwnerPhone);
        await Should.ThrowAsync<UserFriendlyException>(() => _service.UpdateProfileAsync(OwnerPhone, new() { FullName = "Bypass" }));
        _profiles.Single().IsRegistered.ShouldBeFalse();
    }

    public void Dispose() => _provider.Dispose();
}
