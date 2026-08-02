using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EWOMS_ClassLibrary.DataIntegration;
using EWOMS_ClassLibrary.DataControlled;
using System;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EWOMS_CoreAPI.Controller
{
    [Route("api/admin/construction-expenses")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager")]
    public class ConstructionExpensesController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public ConstructionExpensesController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: api/admin/construction-expenses/categories
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _dbContext.ExpenseCategories
                    .OrderBy(c => c.Name)
                    .Select(c => c.Name)
                    .ToListAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to fetch categories", Error = ex.Message });
            }
        }

        public class CategoryCreateRequest
        {
            public string Name { get; set; } = string.Empty;
        }

        // POST: api/admin/construction-expenses/categories
        [HttpPost("categories")]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryCreateRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return BadRequest("Category Name is required.");
                }

                var cleanName = request.Name.Trim();
                
                // Check duplicate
                var exists = await _dbContext.ExpenseCategories
                    .AnyAsync(c => c.Name.ToLower() == cleanName.ToLower());
                
                if (exists)
                {
                    return BadRequest("Category already exists.");
                }

                var category = new ExpenseCategory
                {
                    Name = cleanName
                };

                _dbContext.ExpenseCategories.Add(category);
                await _dbContext.SaveChangesAsync();

                return Ok(category);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to create category", Error = ex.Message });
            }
        }

        // GET: api/admin/construction-expenses
        [HttpGet]
        public async Task<IActionResult> GetExpenses([FromQuery] string? search, [FromQuery] string? category, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var query = _dbContext.ConstructionExpenses.AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var cleanSearch = search.Trim().ToLower();
                    query = query.Where(e => e.ItemName.ToLower().Contains(cleanSearch) || 
                                             (e.Notes != null && e.Notes.ToLower().Contains(cleanSearch)));
                }

                if (!string.IsNullOrWhiteSpace(category) && category != "All")
                {
                    query = query.Where(e => e.Category == category);
                }

                if (startDate.HasValue)
                {
                    query = query.Where(e => e.SpentOn >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(e => e.SpentOn <= endDate.Value);
                }

                var expenses = await query.OrderByDescending(e => e.SpentOn).ToListAsync();
                return Ok(expenses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to fetch expenses", Error = ex.Message });
            }
        }

        public class ExpenseRequest
        {
            public string ItemName { get; set; } = string.Empty;
            public decimal Amount { get; set; }
            public DateTime SpentOn { get; set; }
            public string Category { get; set; } = "Materials";
            public string? Notes { get; set; }
        }

        // POST: api/admin/construction-expenses
        [HttpPost]
        public async Task<IActionResult> CreateExpense([FromBody] ExpenseRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.ItemName))
                {
                    return BadRequest("Item Name is required.");
                }

                if (request.Amount <= 0)
                {
                    return BadRequest("Amount must be greater than zero.");
                }

                var expense = new ConstructionExpense
                {
                    ItemName = request.ItemName,
                    Amount = request.Amount,
                    SpentOn = request.SpentOn,
                    Category = request.Category,
                    Notes = request.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                _dbContext.ConstructionExpenses.Add(expense);
                await _dbContext.SaveChangesAsync();

                return Ok(expense);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to create expense", Error = ex.Message });
            }
        }

        // PUT: api/admin/construction-expenses/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateExpense(int id, [FromBody] ExpenseRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.ItemName))
                {
                    return BadRequest("Item Name is required.");
                }

                if (request.Amount <= 0)
                {
                    return BadRequest("Amount must be greater than zero.");
                }

                var expense = await _dbContext.ConstructionExpenses.FindAsync(id);
                if (expense == null)
                {
                    return NotFound("Expense entry not found.");
                }

                expense.ItemName = request.ItemName;
                expense.Amount = request.Amount;
                expense.SpentOn = request.SpentOn;
                expense.Category = request.Category;
                expense.Notes = request.Notes;

                await _dbContext.SaveChangesAsync();

                return Ok(expense);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to update expense", Error = ex.Message });
            }
        }

        // DELETE: api/admin/construction-expenses/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExpense(int id)
        {
            try
            {
                var expense = await _dbContext.ConstructionExpenses.FindAsync(id);
                if (expense == null)
                {
                    return NotFound("Expense entry not found.");
                }

                _dbContext.ConstructionExpenses.Remove(expense);
                await _dbContext.SaveChangesAsync();

                return Ok(new { Message = "Expense entry deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to delete expense", Error = ex.Message });
            }
        }

        // GET: api/admin/construction-expenses/monthly-report
        [HttpGet("monthly-report")]
        public async Task<IActionResult> GetMonthlyReport()
        {
            try
            {
                var allExpenses = await _dbContext.ConstructionExpenses.ToListAsync();

                var now = DateTime.UtcNow;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                var today = now.Date;

                decimal totalSpent = allExpenses.Sum(e => e.Amount);
                decimal spentThisMonth = allExpenses.Where(e => e.SpentOn >= startOfMonth).Sum(e => e.Amount);
                decimal spentToday = allExpenses.Where(e => e.SpentOn.Date == today).Sum(e => e.Amount);

                // Group by Category overall
                var categoryBreakdown = allExpenses
                    .GroupBy(e => e.Category)
                    .Select(g => new
                    {
                        CategoryName = g.Key,
                        TotalAmount = g.Sum(e => e.Amount),
                        Percentage = totalSpent > 0 ? Math.Round((g.Sum(e => e.Amount) / totalSpent) * 100, 2) : 0
                    })
                    .OrderByDescending(cb => cb.TotalAmount)
                    .ToList();

                // Group by Month
                var monthlySummaries = allExpenses
                    .GroupBy(e => new { e.SpentOn.Year, e.SpentOn.Month })
                    .OrderByDescending(g => g.Key.Year).ThenByDescending(g => g.Key.Month)
                    .Select(g => {
                        var monthTotal = g.Sum(e => e.Amount);
                        var monthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM yyyy", CultureInfo.InvariantCulture);

                        var cats = g.GroupBy(e => e.Category)
                                   .Select(cg => new {
                                       CategoryName = cg.Key,
                                       CategoryTotal = cg.Sum(e => e.Amount),
                                       Percentage = monthTotal > 0 ? Math.Round((cg.Sum(e => e.Amount) / monthTotal) * 100, 2) : 0
                                   })
                                   .OrderByDescending(cg => cg.CategoryTotal)
                                   .ToList();

                        return new {
                            MonthName = monthName,
                            MonthTotal = monthTotal,
                            Categories = cats
                        };
                    })
                    .ToList();

                return Ok(new
                {
                    TotalSpent = totalSpent,
                    SpentThisMonth = spentThisMonth,
                    SpentToday = spentToday,
                    CategoryBreakdown = categoryBreakdown,
                    MonthlySummaries = monthlySummaries
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Failed to calculate report summaries", Error = ex.Message });
            }
        }
    }
}
