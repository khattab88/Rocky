using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rocky.Data;
using Rocky.Models;
using System.Linq;
using Rocky.Utility;
using Rocky.Data.Repositories.Interfaces;

namespace Rocky.Controllers
{
    [Authorize(Roles = Constants.Roles.AdminRole)]
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _repository;

        public CategoryController(ICategoryRepository repository)
        {
            _repository = repository;
        }

        public IActionResult Index()
        {
            var categories = _repository.GetAll();

            return View(categories);
        }

        // GET - Create
        public IActionResult Create()
        {
            return View();
        }

        // POST - Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category category)
        {
            if (!ModelState.IsValid)
            {
                TempData[Constants.Notifications.Error] = "Error while creating category";
                return View(category);
            }


            _repository.Add(category);
            _repository.SaveChanges();

            TempData[Constants.Notifications.Success] = "Category added successfully";

            return RedirectToAction("Index");
        }

        // GET - Edit
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var category = _repository.Find(id.Value);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST - Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                _repository.Update(category);
                _repository.SaveChanges();

                return RedirectToAction("Index");
            }
            else
            {
                return View(category);
            }
        }

        // GET - Delete
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var category = _repository.Find(id.Value);
            if (category == null) 
            {
                return NotFound();
            }

            return View(category);
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

            var category = _repository.Find(id.Value);
            if (category == null) 
            {
                return NotFound();
            }

            _repository.Remove(category);
            _repository.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
