using AspnetCoreMvcFull.Data;
using AspnetCoreMvcFull.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AspnetCoreMvcFull.Controllers
{
  public class ProjectDesignController : Controller
  {
    private readonly GreenboDbContext _context;

    public ProjectDesignController(GreenboDbContext context)
    {
      _context = context;
    }

    // STEP 1: Open form with Project name filled in
    [HttpGet]
    public IActionResult CreateFromProject(long id)
    {
      var project = _context.Projects.FirstOrDefault(p => p.ID == id);

      if (project == null)
      {
        TempData["Error"] = "Project not found.";
        return RedirectToAction("CompanyInformation", "Company");
      }

      var model = new ProjectDesign
      {
        ProjectID = project.ID
      };

      ViewBag.ProjectName = project.ProjectName;

      // Check if this project already has a design
      ViewBag.HasDesign = _context.ProjectDesigns.Any(d => d.ProjectID == project.ID);

      return View("ViewProjectDesign", model);
    }

    // STEP 2: Save ProjectDesign
    [HttpPost]
    public IActionResult Create(ProjectDesign design, IFormFile uploadFile)
    {
      if (uploadFile != null)
      {
        var filePath = Path.Combine("wwwroot/uploads", uploadFile.FileName);
        using (var fs = new FileStream(filePath, FileMode.Create))
        {
          uploadFile.CopyTo(fs);
        }

        design.FileUpload = "/uploads/" + uploadFile.FileName;
      }
      else
      {
        TempData["Error"] = "Please select a file before saving.";
        return RedirectToAction("CreateFromProject", new { id = design.ProjectID });
      }

      design.CreatedAt = DateTime.Now;

      _context.ProjectDesigns.Add(design);
      _context.SaveChanges();

      TempData["Success"] = "Project converted to design successfully!";
      return RedirectToAction("IndexProjectDesign", "ProjectDesign");
    }

    [HttpGet]
    public IActionResult IndexProjectDesign(string searchTerm)
    {
      var designs = _context.ProjectDesigns
          .Include(d => d.Project)
          .AsQueryable();

      if (!string.IsNullOrEmpty(searchTerm))
      {
        searchTerm = searchTerm.ToLower();
        designs = designs.Where(d =>
            d.ProjectID.ToString().Contains(searchTerm) ||
            (!string.IsNullOrEmpty(d.FileUpload) && d.FileUpload.ToLower().Contains(searchTerm))
        );
      }

      var designList = designs
          .OrderByDescending(d => d.CreatedAt)
          .ToList();

      ViewBag.SearchTerm = searchTerm;

      return View(designList);
    }
  }
}
