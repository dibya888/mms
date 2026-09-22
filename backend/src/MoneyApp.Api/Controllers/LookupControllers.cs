using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyApp.Api.Extensions;
using MoneyApp.Application.Categories;
using MoneyApp.Application.Common;
using MoneyApp.Application.Ledger;
using MoneyApp.Domain;

namespace MoneyApp.Api.Controllers;

[ApiController, Route("api/v1/transactions"), Authorize]
public class TransactionsController(LedgerService ledger) : ControllerBase
{
    [HttpPost] public async Task<TransactionDto> Post(PostTransactionRequest r, CancellationToken ct) => await ledger.PostAsync(User.UserId(), r, ct);
}

[ApiController, Route("api/v1/categories"), Authorize]
public class CategoriesController(CategoryService svc) : ControllerBase
{
    [HttpGet] public async Task<List<CategoryDto>> List(CancellationToken ct) => await svc.ListAsync(User.UserId(), ct);
    [HttpPost] public async Task<CategoryDto> Create(CreateCategoryRequest r, CancellationToken ct) => await svc.CreateAsync(User.UserId(), r, ct);
}

[ApiController, Route("api/v1/currencies"), Authorize]
public class CurrenciesController(IAppDbContext db) : ControllerBase
{
    [HttpGet] public async Task<List<Currency>> List(CancellationToken ct) => await db.Currencies.AsNoTracking().OrderBy(c => c.Code).ToListAsync(ct);
}
