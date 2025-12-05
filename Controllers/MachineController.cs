using AspnetCoreMvcFull.Data;
using AspnetCoreMvcFull.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace AspnetCoreMvcFull.Controllers
{
  public class MachineController : Controller
  {
    private readonly GreenboDbContext _context;

    public MachineController(GreenboDbContext context)
    {
      _context = context;
    }

    // GET: Machine/Index
    public async Task<IActionResult> IndexMachine(string searchString)
    {
      var machines = from m in _context.MachineRegistrations
                     select m;

      if (!string.IsNullOrEmpty(searchString))
      {
        machines = machines.Where(m => m.MachineName.Contains(searchString) ||
                                       m.MachinePurpose.Contains(searchString) ||
                                       m.MachineManufacturer.Contains(searchString));
      }

      return View(await machines.ToListAsync());
    }

    // GET: Machine/Create
    public IActionResult CreateMachine()
    {
      return PartialView("CreateMachine"); 

      // return View();
    }

    // POST: Machine/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateMachine(MachineRegistration machine)
    {
      if (ModelState.IsValid)
      {
        _context.MachineRegistrations.Add(machine);
        await _context.SaveChangesAsync();

        // Return JSON for AJAX success
        return Json(new { success = true, message = "Machine created successfully!" });
      }

      // If validation fails, return the form HTML again
      return PartialView("CreateMachine", machine);
    }

    public async Task<IActionResult> EditMachine(long id)
    {
      var machine = await _context.MachineRegistrations.FindAsync(id);
      if (machine == null)
        return NotFound();

      return PartialView("EditMachine", machine);
    }

    // POST: Edit Machine
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditMachine(MachineRegistration machine)
    {
      if (ModelState.IsValid)
      {
        _context.MachineRegistrations.Update(machine);
        await _context.SaveChangesAsync();
        return Json(new { success = true, message = "Machine updated successfully!" });
      }

      return PartialView("EditMachine", machine);
    }


    // GET: Machine/ViewMachine/5
    public IActionResult ViewMachine(long id)
    {
      var machine = _context.MachineRegistrations.FirstOrDefault(m => m.ID == id);
      if (machine == null)
        return NotFound();

      // Return PartialView instead of full view
      return PartialView("ViewMachine", machine);
    }



    // Placeholder: Delete Machine
    public async Task<IActionResult> Delete(long id)
    {
      var machine = await _context.MachineRegistrations.FindAsync(id);

      if (machine == null)
        return NotFound();

      _context.MachineRegistrations.Remove(machine);
      await _context.SaveChangesAsync();

      TempData["Success"] = "Machine deleted successfully!";
      return RedirectToAction(nameof(IndexMachine));
    }

  }
}
