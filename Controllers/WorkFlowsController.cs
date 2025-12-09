using AspnetCoreMvcFull.Data;
using AspnetCoreMvcFull.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AspnetCoreMvcFull.Controllers
{
  public class WorkFlowsController : Controller
  {
    private readonly GreenboDbContext _context;

    public WorkFlowsController(GreenboDbContext context)
    {
      _context = context;
    }

    // GET: WorkFlows
    public async Task<IActionResult> IndexWorkflow(string searchString)
    {
      var workflows = _context.WorkFlow.Include(w => w.Machine).AsQueryable();

      if (!string.IsNullOrEmpty(searchString))
      {
        workflows = workflows.Where(w =>
            w.ProcessName!.Contains(searchString) ||
            w.Description!.Contains(searchString) ||
            w.Machine!.MachineName!.Contains(searchString)
        );
      }

      return View(await workflows.ToListAsync());
    }

    public async Task<IActionResult> CreateOrEdit(long? id)
    {
      WorkFlow workflow;

      if (id == null || id == 0)
      {
        workflow = new WorkFlow();
      }
      else
      {
        workflow = await _context.WorkFlow.FindAsync(id);
        if (workflow == null) return NotFound();
      }

      // Pass machines for dropdown
      ViewBag.Machines = new SelectList(
          _context.MachineRegistrations.AsNoTracking(),
          "ID",
          "MachineName",
          workflow.RequiredMachine
      );

      return PartialView("CreateWorkFlow", workflow);
    }

    // POST: WorkFlows/CreateOrEdit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateOrEdit(WorkFlow workflow)
    {
      try
      {
        if (ModelState.IsValid)
        {

        
          if (workflow.ID == 0)
                      _context.Add(workflow);
          else
            _context.Update(workflow);
          
          await _context.SaveChangesAsync();
          return Json(new { success = true });
        }


        // Model validation failed — gather all errors
        var errors = string.Join("<br/>",
            ModelState.Values
                      .SelectMany(v => v.Errors)
                      .Select(e => e.ErrorMessage));

        Response.StatusCode = 400; // Bad request
        return Content(errors);
      }
      catch (Exception ex)
      {
        // Return the actual exception message
        Response.StatusCode = 400;
        return Content(ex.Message + (ex.InnerException != null ? " - " + ex.InnerException.Message : ""));
      }
    }




    // GET: WorkFlows/View/5
    public async Task<IActionResult> View(long id)
    {
      var workflow = await _context.WorkFlow
          .Include(w => w.Machine)
          .FirstOrDefaultAsync(w => w.ID == id);
      if (workflow == null) return NotFound();
      return PartialView("ViewWorkFlow", workflow);
    }

    // POST: WorkFlows/Delete/5
    [HttpPost]
    public async Task<IActionResult> Delete(long id)
    {
      var workflow = await _context.WorkFlow.FindAsync(id);
      if (workflow != null)
      {
        _context.WorkFlow.Remove(workflow);
        await _context.SaveChangesAsync();
      }
      return Json(new { success = true });
    }
  }
}
