using AspnetCoreMvcFull.Data;
using AspnetCoreMvcFull.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace AspnetCoreMvcFull.Controllers
{
  public class CompanyController : Controller
  {
    private readonly GreenboDbContext _context;

    public CompanyController(GreenboDbContext context)
    {
      _context = context;
    }

    [HttpGet]
    public IActionResult CompanyInformation(string searchTerm)
    {
      var companies = _context.Companies.AsQueryable();

      if (!string.IsNullOrEmpty(searchTerm))
      {
        searchTerm = searchTerm.ToLower();
        companies = companies.Where(c =>
            c.CompanyName.ToLower().Contains(searchTerm) ||
            c.RegistrationNumber.ToLower().Contains(searchTerm) ||
            c.KraPin.ToLower().Contains(searchTerm)
        );
      }

      return View(companies.ToList());
    }

    [HttpGet]
    public IActionResult Create()
    {
      return View("CreateCompany", new Company());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Company model)
    {
      try
      {
        // Get all form files at once
        var formFiles = Request.Form.Files;

        // FILTER OUT EMPTY PROJECT ROWS directly from model.Projects
        if (model.Projects != null)
        {
          model.Projects = model.Projects
            .Where(p => !string.IsNullOrWhiteSpace(p.ProjectName) && p.Quantity > 0)
            .ToList();
        }

        // CHECK IF AT LEAST ONE PROJECT EXISTS
        if (model.Projects == null || model.Projects.Count == 0)
        {
          ModelState.AddModelError("", "Please add at least one project with Name and Quantity.");
          return View("CreateCompany", model);
        }

        // CHECK IF RegistrationNumber IS EMPTY
        if (string.IsNullOrWhiteSpace(model.RegistrationNumber))
        {
          ModelState.AddModelError("RegistrationNumber", "Registration Number cannot be empty.");
          return View("CreateCompany", model);
        }

        // CHECK FOR DUPLICATE KRA PIN
        if (!string.IsNullOrWhiteSpace(model.KraPin))
        {
          bool kraPinExists = _context.Companies
              .Any(c => c.KraPin == model.KraPin);

          if (kraPinExists)
          {
            ModelState.AddModelError("KraPin",
                $"A company with KRA PIN '{model.KraPin}' already exists in the system.");
            return View("CreateCompany", model);
          }
        }

        // CHECK FOR DUPLICATE REGISTRATION NUMBER
        bool registrationExists = _context.Companies
            .Any(c => c.RegistrationNumber == model.RegistrationNumber);

        if (registrationExists)
        {
          ModelState.AddModelError("RegistrationNumber",
              "A company with this Registration Number already exists.");
          return View("CreateCompany", model);
        }

        // PREVENT DUPLICATE PROJECT ROWS WITHIN CURRENT SUBMISSION
        var duplicatesInSubmission = model.Projects
            .GroupBy(p => new { p.ProjectName, p.Quantity, p.Status })
            .Where(g => g.Count() > 1)
            .ToList();

        if (duplicatesInSubmission.Any())
        {
          ModelState.AddModelError("", "Duplicate project rows detected in your submission. Please remove duplicate entries.");
          return View("CreateCompany", model);
        }

        // CHECK FOR DUPLICATE PROJECTS IN DATABASE
        foreach (var project in model.Projects)
        {
          var existingProject = _context.Projects
              .FirstOrDefault(p =>
                  p.ProjectName.ToLower() == project.ProjectName.ToLower() &&
                  p.Quantity == project.Quantity &&
                  p.Status == project.Status);

          if (existingProject != null)
          {
            ModelState.AddModelError("",
                $"Project '{project.ProjectName}' with Quantity {project.Quantity} already exists in the database. Please modify or remove this duplicate.");
            return View("CreateCompany", model);
          }
        }

        // SAVE COMPANY HEADER
        model.CreatedAt = DateTime.Now;
        model.CreatedBy = User.Identity?.Name ?? "System";

        _context.Companies.Add(model);
        _context.SaveChanges();   // Generates Company.ID

        // SAVE PROJECT LINES with file handling
        int savedCount = 0;
        var projectsList = model.Projects.ToList();

        for (int i = 0; i < projectsList.Count; i++)
        {
          var project = projectsList[i];

          project.ID = 0;
          project.CompanyID = model.ID;
          project.CreatedAt = DateTime.Now;
          project.CreatedBy = User.Identity?.Name ?? "System";

          // Handle file upload
          var fileKey = $"Projects[{i}].FileUpload";
          var file = formFiles[fileKey];

          if (file != null && file.Length > 0)
          {
            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/projects");
            if (!Directory.Exists(folder))
              Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
              file.CopyTo(stream);
            }

            project.FileUpload = $"/uploads/projects/{fileName}";
          }

          _context.Projects.Add(project);
          savedCount++;
        }

        _context.SaveChanges();

        TempData["Success"] = $"Company and {savedCount} project(s) created successfully!";
        return RedirectToAction("CompanyInformation");
      }
      catch (Exception ex)
      {
        // Deep error trace
        string error = ex.Message;
        var inner = ex.InnerException;

        while (inner != null)
        {
          error += " --> " + inner.Message;
          inner = inner.InnerException;
        }

        TempData["Error"] = "Error saving company: " + error;
        return View("CreateCompany", model);
      }
    }

    [HttpGet]
    public IActionResult View(long id)
    {
      var company = _context.Companies.FirstOrDefault(c => c.ID == id);
      if (company == null)
        return NotFound();

      ViewBag.Projects = _context.Projects.Where(p => p.CompanyID == id).ToList();

      return View("ViewCompany", company);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddProjects(long companyId, List<Project> NewProjects)
    {
      try
      {
        // Verify company exists
        var company = _context.Companies.FirstOrDefault(c => c.ID == companyId);
        if (company == null)
        {
          TempData["Error"] = "Company not found.";
          return RedirectToAction("CompanyInformation");
        }

        // Get all form files at once
        var formFiles = Request.Form.Files;

        // FILTER OUT EMPTY PROJECT ROWS directly from NewProjects
        if (NewProjects != null)
        {
          NewProjects = NewProjects
            .Where(p => !string.IsNullOrWhiteSpace(p.ProjectName) && p.Quantity > 0)
            .ToList();
        }

        // CHECK IF AT LEAST ONE PROJECT EXISTS
        if (NewProjects == null || NewProjects.Count == 0)
        {
          ModelState.AddModelError("", "Please add at least one project with Name and Quantity.");
          ViewBag.Projects = _context.Projects.Where(p => p.CompanyID == companyId).ToList();
          return View("ViewCompany", company);
        }

        // PREVENT DUPLICATE PROJECT ROWS WITHIN CURRENT SUBMISSION
        var duplicatesInSubmission = NewProjects
            .GroupBy(p => new { p.ProjectName, p.Quantity, p.Status })
            .Where(g => g.Count() > 1)
            .ToList();

        if (duplicatesInSubmission.Any())
        {
          ModelState.AddModelError("", "Duplicate project rows detected in your submission. Please remove duplicate entries.");
          ViewBag.Projects = _context.Projects.Where(p => p.CompanyID == companyId).ToList();
          return View("ViewCompany", company);
        }

        // CHECK FOR DUPLICATE PROJECTS IN DATABASE (Global check)
        foreach (var project in NewProjects)
        {
          var existingProject = _context.Projects
              .FirstOrDefault(p =>
                  p.ProjectName.ToLower() == project.ProjectName.ToLower() &&
                  p.Quantity == project.Quantity &&
                  p.Status == project.Status);

          if (existingProject != null)
          {
            ModelState.AddModelError("",
                $"Project '{project.ProjectName}' with Quantity {project.Quantity} already exists in the database. Please modify or remove this duplicate.");
            ViewBag.Projects = _context.Projects.Where(p => p.CompanyID == companyId).ToList();
            return View("ViewCompany", company);
          }
        }

        // SAVE NEW PROJECTS with file handling
        int savedCount = 0;
        for (int i = 0; i < NewProjects.Count; i++)
        {
          var project = NewProjects[i];

          project.ID = 0;
          project.CompanyID = companyId;
          project.CreatedAt = DateTime.Now;
          project.CreatedBy = User.Identity?.Name ?? "System";

          // Handle file upload
          var fileKey = $"NewProjects[{i}].FileUpload";
          var file = formFiles[fileKey];

          if (file != null && file.Length > 0)
          {
            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/projects");
            if (!Directory.Exists(folder))
              Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
              file.CopyTo(stream);
            }

            project.FileUpload = $"/uploads/projects/{fileName}";
          }

          _context.Projects.Add(project);
          savedCount++;
        }

        _context.SaveChanges();

        TempData["Success"] = $"{savedCount} new project(s) added successfully to {company.CompanyName}!";
        return RedirectToAction("View", new { id = companyId });
      }
      catch (Exception ex)
      {
        // Deep error trace
        string error = ex.Message;
        var inner = ex.InnerException;

        while (inner != null)
        {
          error += " --> " + inner.Message;
          inner = inner.InnerException;
        }

        TempData["Error"] = "Error saving projects: " + error;
        return RedirectToAction("View", new { id = companyId });
      }
    }
    [HttpGet]
    public IActionResult Delete(long id)
    {
      var company = _context.Companies.FirstOrDefault(c => c.ID == id);
      if (company != null)
      {
        var projects = _context.Projects.Where(p => p.CompanyID == company.ID).ToList();
        _context.Projects.RemoveRange(projects);

        _context.Companies.Remove(company);
        _context.SaveChanges();

        TempData["Success"] = $"Company '{company.CompanyName}' deleted successfully.";
      }

      return RedirectToAction("CompanyInformation");
    }
  }
}
