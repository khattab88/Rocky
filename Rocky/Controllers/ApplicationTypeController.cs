using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rocky.Data;
using System.Linq;
using Rocky.Utility;
using Rocky.Models;
using Rocky.Data.Repositories.Interfaces;

namespace Rocky.Controllers
{
    [Authorize(Roles = Constants.Roles.AdminRole)]
    public class ApplicationTypeController : Controller
    {
        private readonly IApplicationTypeRepository _repository;

        public ApplicationTypeController(IApplicationTypeRepository repository)
        {
            _repository = repository;
        }

        public IActionResult Index()
        {
            var applicationTypes = _repository.GetAll();

            return View(applicationTypes);
        }

        public IActionResult Create() 
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ApplicationType applicationType) 
        {
            if (ModelState.IsValid)
            {
                _repository.Add(applicationType);
                _repository.SaveChanges();

                return RedirectToAction("Index");
            }
            else
            {
                return View(applicationType);
            }
        }

        // GET - Edit
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var appType = _repository.Find(id.GetValueOrDefault());
            if (appType == null)
            {
                return NotFound();
            }

            return View(appType);
        }

        // POST - Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ApplicationType appType)
        {
            if (ModelState.IsValid)
            {
                _repository.Update(appType);
                _repository.SaveChanges();

                return RedirectToAction("Index");
            }
            else
            {
                return View(appType);
            }
        }

        // GET - Delete
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var appType = _repository.Find(id.GetValueOrDefault());
            if (appType == null)
            {
                return NotFound();
            }

            return View(appType);
        }

        // POST - Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var appType = _repository.Find(id.GetValueOrDefault());
            if (appType == null)
            {
                return NotFound();
            }

            _repository.Remove(appType);
            _repository.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
