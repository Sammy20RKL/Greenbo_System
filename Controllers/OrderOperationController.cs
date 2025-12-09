using AspnetCoreMvcFull.Data;
using AspnetCoreMvcFull.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace AspnetCoreMvcFull.Controllers
{
  public class OrderOperationController : Controller
  {
    private readonly GreenboDbContext _context;

    public OrderOperationController(GreenboDbContext context)
    {
      _context = context;
    }

    // GET: OrderOperation
    public async Task<IActionResult> IndexOrderOperation(string searchString)
    {
      var operations = _context.OrderOperations
        //  .Include(o => o.Project)  // assumes you have navigation property in OrderOperation
          .AsQueryable();

      if (!string.IsNullOrEmpty(searchString))
      {
        operations = operations.Where(o =>
           // o.Project.ProjectName.Contains(searchString) ||
            o.PartNumber.ToString().Contains(searchString) ||
            o.AssignedTo.ToString().Contains(searchString) ||
            o.Status.Contains(searchString));
      }

      return View(await operations.ToListAsync());
    }

    // GET: View single order operation
    public IActionResult ViewOrderOperation(long id)
    {
      var operation = _context.OrderOperations
          .Include(o => o.Project)           // Get project name
          .FirstOrDefault(o => o.ID == id);

      if (operation == null)
        return NotFound();

      return PartialView("ViewOrderOperation", operation);
    }


    // DELETE: Delete order operation
    public async Task<IActionResult> Delete(long id)
    {
      var operation = await _context.OrderOperations.FindAsync(id);
      if (operation == null) return NotFound();

      _context.OrderOperations.Remove(operation);
      await _context.SaveChangesAsync();

      TempData["Success"] = "Order operation deleted successfully!";
      return RedirectToAction(nameof(IndexOrderOperation));
    }
    public IActionResult CreateOrderOperation()
    {
      // Load users for AssignedTo dropdown
      ViewBag.Users = new SelectList(_context.Users.ToList(), "ID", "Name");
      return View();
    }

    // POST: Save new OrderOperation
    [HttpPost]
    public IActionResult CreateOrderOperation(OrderOperation order)
    {
      if (ModelState.IsValid)
      {
        order.Status = "Approved";
        order.CreatedDate = DateTime.Now;
        _context.OrderOperations.Add(order);
        _context.SaveChanges();
        TempData["Success"] = "Order Operation created successfully!";
        return RedirectToAction("IndexOrderOperation");
      }

      // Collect all validation errors
      var errors = ModelState.Values
          .SelectMany(v => v.Errors)
          .Select(e => e.ErrorMessage)
          .ToList();

      TempData["Error"] = "Failed to save: " + string.Join(", ", errors);
      ViewBag.Users = new SelectList(_context.Users.ToList(), "ID", "Name", order.AssignedTo);
      return View(order);
    }

    // AJAX: Get projects for modal
    [HttpGet]
    public IActionResult GetProjects()
    {
      var projects = _context.Projects
          .Select(p => new
          {
            p.ID,
            p.ProjectName,
            p.CompanyID,
            p.Status
          }).ToList();

      return Json(projects);
    }

    // AJAX: Get parts filtered by project
    [HttpGet]
    public IActionResult GetParts(long projectId)
    {
      var parts = _context.MaterialLists
          .Where(p => p.ProjectID == projectId)
          .Select(p => new
          {
            p.ID,  // Add this line
            p.PartNumber,
            p.Description
          }).ToList();
      return Json(parts);
    }

    // AJAX: Get workflow processes
    [HttpGet]
    public IActionResult GetProcesses()
    {
      var processes = _context.WorkFlow
          .Select(w => new
          {
            w.ID,
            w.ProcessName
          }).ToList();

      return Json(processes);
    }
  }
}
