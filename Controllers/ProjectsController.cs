using AspnetCoreMvcFull.Data;
using AspnetCoreMvcFull.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AspnetCoreMvcFull.Controllers
{
  public class ProjectsController : Controller
  {
    private readonly GreenboDbContext _context;

    public ProjectsController(GreenboDbContext context)
    {
      _context = context;
    }

    [HttpGet]
    public IActionResult IndexProject(string searchTerm)
    {
      var projects = _context.Projects
          .Include(p => p.Company) // Include related company
          .AsQueryable();

      if (!string.IsNullOrEmpty(searchTerm))
      {
        searchTerm = searchTerm.ToLower();
        projects = projects.Where(p =>
            p.ProjectName.ToLower().Contains(searchTerm) ||
            (p.Company != null && p.Company.CompanyName.ToLower().Contains(searchTerm))
        );
      }

      var projectList = projects
          .OrderByDescending(p => p.CreatedAt)
          .ToList();

      ViewBag.SearchTerm = searchTerm;

      return View(projectList);
    }
  }
}
