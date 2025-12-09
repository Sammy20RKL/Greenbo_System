using AspnetCoreMvcFull.Data;
using AspnetCoreMvcFull.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace AspnetCoreMvcFull.Controllers
{
  public class MachineAllocationController : Controller
  {
    private readonly GreenboDbContext _context;

    public MachineAllocationController(GreenboDbContext context)
    {
      _context = context;
    }

    // LIST + SEARCH
    public async Task<IActionResult> IndexAllocation(string searchString)
    {
      var allocations = _context.MachineAllocations
          .Include(a => a.Machine)
          .Include(a => a.Project)
          .AsQueryable();

      if (!string.IsNullOrEmpty(searchString))
      {
        allocations = allocations.Where(a =>
            a.Machine.MachineName.Contains(searchString) ||
            a.Project.ProjectName.Contains(searchString) ||
            a.UserID.Contains(searchString));
      }

      return View(await allocations.ToListAsync());
    }

    // CREATE GET
    public IActionResult CreateAllocation()
    {
      return PartialView("CreateAllocation", new MachineAllocation());
    }

    // CREATE POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateAllocation(MachineAllocation allocation)
    {
      if (ModelState.IsValid)
      {
        _context.MachineAllocations.Add(allocation);
        _context.SaveChanges();
        return Json(new { success = true, message = "Machine allocated successfully!" });
      }
      return PartialView("CreateAllocation", allocation);
    }

    // EDIT GET
    public IActionResult EditAllocation(long id)
    {
      var allocation = _context.MachineAllocations
          .Include(a => a.Machine)
          .Include(a => a.Project)
          .FirstOrDefault(a => a.ID == id);
      if (allocation == null) return NotFound();

      return PartialView("CreateAllocation", allocation);
    }

    // EDIT POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditAllocation(MachineAllocation allocation)
    {
      if (ModelState.IsValid)
      {
        var existing = _context.MachineAllocations.Find(allocation.ID);
        if (existing == null) return NotFound();

        existing.MachineID = allocation.MachineID;
        existing.ProjectID = allocation.ProjectID;
        existing.UserID = allocation.UserID;
        existing.Datex = allocation.Datex;

        _context.SaveChanges();
        return Json(new { success = true, message = "Allocation updated successfully!" });
      }
      return PartialView("CreateAllocation", allocation);
    }

    // VIEW GET
    public IActionResult ViewAllocation(long id)
    {
      var allocation = _context.MachineAllocations
          .Include(a => a.Machine)
          .Include(a => a.Project)
          .FirstOrDefault(a => a.ID == id);

      if (allocation == null) return NotFound();

      return PartialView("ViewAllocation", allocation);
    }

    // DELETE
    public async Task<IActionResult> Delete(long id)
    {
      var alloc = await _context.MachineAllocations.FindAsync(id);
      if (alloc == null) return NotFound();

      _context.MachineAllocations.Remove(alloc);
      await _context.SaveChangesAsync();

      TempData["Success"] = "Allocation deleted successfully!";
      return RedirectToAction(nameof(IndexAllocation));
    }
    public IActionResult MachineList()
    {
      var machines = _context.MachineRegistrations.ToList();
      return PartialView("MachineList", machines);
    }

    public IActionResult ProjectList()
    {
      var projects = _context.Projects
          .Include(p => p.Company) // Include company for displaying company name
          .ToList();
      return PartialView("ProjectList", projects);
    }
    [HttpGet]
    public IActionResult GetUsers()
    {
      var users = _context.Users
          .Select(u => new { id = u.ID.ToString(), name = u.Name })
          .ToList();
      return Json(users);
    }
    [HttpGet]
    public IActionResult GetMachines()
    {
      var machines = _context.MachineRegistrations
          .Select(m => new {
            id = m.ID.ToString(),
            name = m.MachineName,
            purpose = m.MachinePurpose ?? "N/A"
          })
          .ToList();
      return Json(machines);
    }

    [HttpGet]
    public IActionResult GetProjects()
    {
      var projects = _context.Projects
          .Include(p => p.Company)
          .Select(p => new {
            id = p.ID.ToString(),
            name = p.ProjectName,
            company = p.Company != null ? p.Company.CompanyName : "N/A",
            quantity = p.Quantity
          })
          .ToList();
      return Json(projects);
    }

  }
}
