using MiniCrm.Domain.Customers;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniCrm.Application.Customers;

public sealed record CustomerDto(
    Guid Id, string Name, string Email, string? Company,
    CustomerStatus Status, Guid OwnerId, DateTimeOffset CreatedAt, byte[] Version);

public interface ICustomerDetails
{
    string Name { get; }
    string Email { get; }
    string? Company { get; }
}

public sealed record CreateCustomerRequest(string Name, string Email, string? Company) : ICustomerDetails;

public sealed record UpdateCustomerRequest(string Name, string Email, string? Company, byte[] Version)
    : ICustomerDetails;

public sealed record ChangeCustomerStatusRequest(CustomerStatus Status, byte[] Version);

public enum CustomerSort { Name, Email, CreatedAt }

public sealed record ListCustomersQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    CustomerStatus? Status = null,
    CustomerSort SortBy = CustomerSort.Name,
    bool Descending = false);