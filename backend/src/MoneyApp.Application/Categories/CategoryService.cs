using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using MoneyApp.Application.Common;
using MoneyApp.Domain;

namespace MoneyApp.Application.Categories;

public sealed record CreateCategoryRequest([property: Required, StringLength(60, MinimumLength = 1)] string Name);
public sealed record CategoryDto(Guid Id, string? Key, string? Name, bool IsCustom);

public class CategoryService(IAppDbContext db)
{
    // Predefined (i18n key) + the caller's own custom categories. Same endpoint for individual and family use.
    public Task<List<CategoryDto>> ListAsync(Guid userId, CancellationToken ct) =>
        db.ExpenseCategories.Where(c => c.OwnerUserId == null || c.OwnerUserId == userId)
            .OrderBy(c => c.OwnerUserId != null).ThenBy(c => c.Key).ThenBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Key, c.Name, c.OwnerUserId != null)).ToListAsync(ct);

    public async Task<CategoryDto> CreateAsync(Guid userId, CreateCategoryRequest r, CancellationToken ct)
    {
        var c = new ExpenseCategory { OwnerUserId = userId, Name = r.Name.Trim() };
        db.ExpenseCategories.Add(c);
        await db.SaveChangesAsync(ct);
        return new CategoryDto(c.Id, null, c.Name, true);
    }
}
