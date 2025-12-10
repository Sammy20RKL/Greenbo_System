using AspnetCoreMvcFull.Data;
using AspnetCoreMvcFull.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspnetCoreMvcFull.Controllers
{
  public class MaterialListController : Controller
  {
    private readonly GreenboDbContext _context;

    public MaterialListController(GreenboDbContext context)
    {
      _context = context;
    }
    // GET: View material list for a project
    [HttpGet]
    public IActionResult ViewMaterialList()
    {
      var materials = _context.MaterialLists
          .OrderBy(m => m.PartNumber)
          .ToList();

      return View(materials);
    }


    // GET: Add material list for project
    [HttpGet]
    public IActionResult AddMaterialList(long projectId)
    {
      // Fetch project including company
      var project = _context.Projects
                    .Where(p => p.ID == projectId)
                    .Select(p => new
                    {
                      p.ID,
                      p.ProjectName,
                      CompanyName = p.Company!.CompanyName
                    })
                    .FirstOrDefault();

      if (project == null)
        return NotFound();

      // Pass data to view
      ViewBag.ProjectID = project.ID;
      ViewBag.ProjectName = project.ProjectName;
      ViewBag.CustomerName = project.CompanyName;

      return View();
    }


    public IActionResult IndexMaterialList(long projectId)
    {
      // Load project with company (same as AddMaterialList)
      var project = _context.Projects
                      .Where(p => p.ID == projectId)
                      .Select(p => new
                      {
                        p.ID,
                        p.ProjectName,
                        CompanyName = p.Company!.CompanyName
                      })
                      .FirstOrDefault();

      if (project == null)
        return NotFound();

      // Load all materials for this project
      var materials = _context.MaterialLists
          .Where(m => m.ProjectID == projectId)
          .OrderBy(m => m.PartNumber)
          .ToList();

      // Send data to the view
      ViewBag.ProjectID = project.ID;
      ViewBag.ProjectName = project.ProjectName;
      ViewBag.CustomerName = project.CompanyName;

      return View(materials);
    }


    // POST: Save material list
    [HttpPost]
    public IActionResult AddMaterialList(long projectId, List<MaterialList> materials)
    {
      if (materials == null || materials.Count == 0)
      {
       // TempData["Error"] = "You must add at least one material row.";
        return RedirectToAction("AddMaterialList", new { projectId = projectId });
      }

      foreach (var m in materials)
      {
        m.ProjectID = projectId;
        m.CreatedAt = DateTime.Now;
        m.CreatedBy = "System";
      }

      _context.MaterialLists.AddRange(materials);
      _context.SaveChanges();

      TempData["Success"] = $"Successfully saved {materials.Count} material(s)!";

      return RedirectToAction("ViewMaterialList", new { projectId = projectId });
    }
  }
}
