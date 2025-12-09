using AspnetCoreMvcFull.Data;
using AspnetCoreMvcFull.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspnetCoreMvcFull.Controllers
{
  public class WorkOrderStepController : Controller
  {
    private readonly GreenboDbContext _context;

    public WorkOrderStepController(GreenboDbContext context)
    {
      _context = context;
    }

    // ==================== INDEX ====================
    public async Task<IActionResult> IndexStep(string searchString)
    {
      var query = _context.WorkOrderStepEffort
                  .Include(x => x.Machine)
                  .Include(x => x.OrderOperation)
                  .AsQueryable();

      if (!string.IsNullOrEmpty(searchString))
      {
        query = query.Where(x =>
               x.OperationId.ToString().Contains(searchString) ||
               x.Machine.MachineName.Contains(searchString) ||
               x.OperatorId.ToString().Contains(searchString));
      }

      var list = await query.ToListAsync();
      return View(list);
    }


    // GET: Create or Edit Step
    public async Task<IActionResult> CreateEditStep(long id = 0)
    {
      WorkOrderStepEffort model; // outer model

      if (id == 0)
      {
        model = new WorkOrderStepEffort(); // Create
      }
      else
      {
        // Assign to the outer model, not redeclare
        model = await _context.WorkOrderStepEffort.FirstOrDefaultAsync(w => w.EffortId == id);

        if (model == null) return NotFound();
      }

      // Populate embedded popup lists asynchronously
      ViewBag.Operations = await _context.OrderOperations
          .Include(o => o.Project)
          .ToListAsync();

      ViewBag.Machines = await _context.MachineRegistrations.ToListAsync();
      ViewBag.Users = await _context.Users.ToListAsync();

      return PartialView(model); // return partial view for modal
    }


    // POST: Create or Edit Step
    [HttpPost]
    public async Task<IActionResult> CreateEditStep(WorkOrderStepEffort model)
    {
      // Check required fields
      if (model.OperationId == 0)
      {
        return Json(new { success = false, message = "Please select an Operation." });
      }

      if (model.MachineId == null)
      {
        return Json(new { success = false, message = "Please select a Machine." });
      }

      if (model.OperatorId == null)
      {
        return Json(new { success = false, message = "Please select an Operator." });
      }

      if (model.EffortId == 0) // Create
      {
        model.CreatedAt = DateTime.Now;
        _context.WorkOrderStepEffort.Add(model);
      }
      else // Edit
      {
        var existing = await _context.WorkOrderStepEffort.FindAsync(model.EffortId);
        if (existing == null)
          return Json(new { success = false, message = "Step not found." });

        // Update fields
        existing.OperationId = model.OperationId;
        existing.MachineId = model.MachineId;
        existing.OperatorId = model.OperatorId;
        existing.QuantityAssigned = model.QuantityAssigned;
        existing.QuantityCompleted = model.QuantityCompleted;
        existing.UnitOfMeasure = model.UnitOfMeasure;
        existing.ExpectedDuration = model.ExpectedDuration;
        existing.MachineStartTime = model.MachineStartTime;
        existing.MachineEndTime = model.MachineEndTime;
        existing.UpdatedAt = DateTime.Now;
      }

      await _context.SaveChangesAsync();
      return Json(new { success = true, message = "Step saved successfully!" });
    }





    // ==================== VIEW POPUP ====================
    public async Task<IActionResult> ViewStep(long id)
    {
      var step = await _context.WorkOrderStepEffort
              .Include(x => x.Machine)
              .Include(x => x.OrderOperation)
              .FirstOrDefaultAsync(x => x.EffortId == id);

      if (step == null) return NotFound();

      return PartialView("ViewStep", step);
    }


    // ==================== DELETE ====================
    public async Task<IActionResult> Delete(long id)
    {
      try
      {
        var step = await _context.WorkOrderStepEffort.FindAsync(id);
        if (step == null)
          return Json(new { success = false, message = "Step not found." });

        _context.WorkOrderStepEffort.Remove(step);
        await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Step deleted successfully!" });
      }
      catch (Exception ex)
      {
        return Json(new { success = false, message = "Error: " + ex.Message });
      }
    }


    // ==================== MACHINE AVAILABILITY CHECK ====================
    [HttpGet]
    public async Task<JsonResult> CheckMachineAvailability(long machineId)
    {
      var step = await _context.WorkOrderStepEffort
               .Where(x => x.MachineId == machineId && x.MachineEndTime > DateTime.Now)
               .OrderByDescending(x => x.MachineEndTime)
               .FirstOrDefaultAsync();

      if (step == null)
      {
        return Json(new { inUse = false });
      }

      return Json(new
      {
        inUse = true,
        message = $"Machine #{machineId} is currently in use and will be available at {step.MachineEndTime?.ToString("hh:mm tt")}."
      });
    }

    public async Task<IActionResult> SelectOperation()
    {
      var operations = await _context.OrderOperations.ToListAsync();
      return PartialView("_SelectOperation", operations);
    }

    public async Task<IActionResult> SelectMachine()
    {
      var machines = await _context.MachineRegistrations.ToListAsync();
      return PartialView("_SelectMachine", machines);
    }

    public async Task<IActionResult> SelectOperator()
    {
      var operators = await _context.Users.ToListAsync();
      return PartialView("_SelectOperator", operators);
    }
  }
}
