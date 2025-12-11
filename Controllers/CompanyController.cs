using AspnetCoreMvcFull.Data;
using AspnetCoreMvcFull.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
    // POST: Create new customer
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Company model)
    {
      try
      {
        // Get all form files at once
        var formFiles = Request.Form.Files;

        // CRITICAL: Filter out empty project rows FIRST (like SalesOrder does)
        if (model.Projects != null)
        {
          model.Projects = model.Projects
            .Where(p => !string.IsNullOrWhiteSpace(p.ProjectName) && p.Quantity > 0)
            .ToList();
        }

        // CHECK IF AT LEAST ONE PROJECT EXISTS after filtering
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

        
        // Remove navigation properties to avoid EF validation issues
        ModelState.Remove("Projects");

        // SAVE COMPANY HEADER FIRST (like SalesOrder pattern)
        model.CreatedAt = DateTime.Now;
        model.CreatedBy = User.Identity?.Name ?? "System";

        // Store projects temporarily and detach to prevent auto-save
        var projectsList = model.Projects.ToList();
        model.Projects = null;

        _context.Companies.Add(model);
        _context.SaveChanges();   // This generates Company.ID

        // Now attach saved CompanyID to each project and save
        int savedCount = 0;
        for (int i = 0; i < projectsList.Count; i++)
        {
          var project = projectsList[i];

          project.ID = 0;
          project.CompanyID = model.ID; // Link to parent company
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

        TempData["Success"] = $"Company '{model.CompanyName}' and {savedCount} project(s) created successfully!";
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
        try
        {
          // Get all projects for this company
          var projects = _context.Projects
                                 .Where(p => p.CompanyID == company.ID)
                                 .ToList();

          foreach (var project in projects)
          {
            // 1. Delete ProjectDesign rows
            var designs = _context.ProjectDesigns
                                  .Where(d => d.ProjectID == project.ID)
                                  .ToList();
            _context.ProjectDesigns.RemoveRange(designs);

            // 2. Delete MaterialList rows
            var materials = _context.MaterialLists
                                    .Where(m => m.ProjectID == project.ID)
                                    .ToList();
            _context.MaterialLists.RemoveRange(materials);

          
          }

          // 3. Delete the projects
          _context.Projects.RemoveRange(projects);

          // 4. Delete the company
          _context.Companies.Remove(company);

          _context.SaveChanges();

          TempData["Success"] = $"Company '{company.CompanyName}' deleted successfully.";
        }
        catch (DbUpdateException ex)
        {
          TempData["Error"] = ex.InnerException?.Message;
          return RedirectToAction("CreateCompany");
        }
      }

      return RedirectToAction("CompanyInformation");
    }
    [HttpGet]
    public IActionResult EditCompany(long id)
    {
      var company = _context.Companies
          .Include(c => c.Projects)
          .FirstOrDefault(c => c.ID == id);

      if (company == null) return NotFound();

      return View(company);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditCompany(Company model, List<IFormFile> projectFiles)
    {
      try
      {
        // Load company and existing projects from DB
        var company = _context.Companies
            .Include(c => c.Projects)
            .FirstOrDefault(c => c.ID == model.ID);

        if (company == null) return NotFound();

        // Update company fields
        company.CompanyName = model.CompanyName;
        company.RegistrationNumber = model.RegistrationNumber;
        company.KraPin = model.KraPin;
        company.ContactAddress = model.ContactAddress;
        company.Designation = model.Designation;
        company.PhoneNumber = model.PhoneNumber;
        company.Email = model.Email;
        company.Status = model.Status;
        company.UpdatedAt = DateTime.Now;
        company.UpdatedBy = User.Identity?.Name ?? "System";

        // Update existing projects and delete marked ones
        for (int i = 0; i < company.Projects.Count; i++)
        {
          var p = company.Projects.ElementAt(i);

          // Check if this project was marked for deletion (via checkbox)
          var formKeyDelete = $"Projects[{i}].MarkedForDelete";
          var isDelete = Request.Form[formKeyDelete].FirstOrDefault() == "on";

          if (isDelete)
          {
            // Delete related ProjectDesigns
            var designs = _context.ProjectDesigns.Where(d => d.ProjectID == p.ID).ToList();
            _context.ProjectDesigns.RemoveRange(designs);

            // Delete related MaterialLists
            var materials = _context.MaterialLists.Where(m => m.ProjectID == p.ID).ToList();
            _context.MaterialLists.RemoveRange(materials);

            // Delete old file
            if (!string.IsNullOrEmpty(p.FileUpload))
            {
              var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", p.FileUpload.TrimStart('/'));
              if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
            }

            _context.Projects.Remove(p);
          }
          else
          {
            // Update project fields
            var formKeyName = $"Projects[{i}].ProjectName";
            var formKeyQty = $"Projects[{i}].Quantity";
            var formKeyStatus = $"Projects[{i}].Status";

            p.ProjectName = Request.Form[formKeyName];
            p.Quantity = int.TryParse(Request.Form[formKeyQty], out int q) ? q : 0;
            p.Status = Request.Form[formKeyStatus];
            p.UpdatedAt = DateTime.Now;
            p.UpdatedBy = User.Identity?.Name ?? "System";

            // Handle uploaded file
            var file = projectFiles.ElementAtOrDefault(i);
            if (file != null && file.Length > 0)
            {
              var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/projects");
              if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

              var fileName = $"{Guid.NewGuid()}_{file.FileName}";
              var filePath = Path.Combine(folder, fileName);

              using (var stream = new FileStream(filePath, FileMode.Create))
                file.CopyTo(stream);

              // Delete old file
              if (!string.IsNullOrEmpty(p.FileUpload))
              {
                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", p.FileUpload.TrimStart('/'));
                if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
              }

              p.FileUpload = $"/uploads/projects/{fileName}";
            }

            _context.Projects.Update(p);
          }
        }

        // Handle new projects added dynamically
        var newProjectsCount = Request.Form.Keys
            .Count(k => k.StartsWith("Projects[") && k.Contains("].ProjectName") && !string.IsNullOrEmpty(Request.Form[k]));

        for (int i = company.Projects.Count; i < company.Projects.Count + newProjectsCount; i++)
        {
          var projectName = Request.Form[$"Projects[{i}].ProjectName"];
          var quantityStr = Request.Form[$"Projects[{i}].Quantity"];
          var status = Request.Form[$"Projects[{i}].Status"];

          if (string.IsNullOrWhiteSpace(projectName)) continue;

          var newProject = new Project
          {
            CompanyID = company.ID,
            ProjectName = projectName,
            Quantity = int.TryParse(quantityStr, out int q) ? q : 0,
            Status = status,
            CreatedAt = DateTime.Now,
            CreatedBy = User.Identity?.Name ?? "System"
          };

          // Handle file for new project
          var file = projectFiles.ElementAtOrDefault(i);
          if (file != null && file.Length > 0)
          {
            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/projects");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
              file.CopyTo(stream);

            newProject.FileUpload = $"/uploads/projects/{fileName}";
          }

          _context.Projects.Add(newProject);
        }

        _context.SaveChanges();
        TempData["Success"] = "Company and projects updated successfully!";
        return RedirectToAction("CompanyInformation");
      }
      catch (Exception ex)
      {
        TempData["Error"] = ex.Message;
        return View(model);
      }
    }
  }
}
