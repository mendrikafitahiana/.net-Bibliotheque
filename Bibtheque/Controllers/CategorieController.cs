using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Bibtheque.Models;
using Bibtheque.Models.Context;
using Microsoft.AspNetCore.Authorization;
using System.Globalization;

namespace Bibtheque.Controllers
{
    [Authorize]
    public class CategorieController : Controller
    {
        private readonly BibthequeContext _context;

        public CategorieController(BibthequeContext context)
        {
            _context = context;
        }

        // GET: Categorie
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categorie.ToListAsync());
        }

        // GET: Categorie/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categorie = await _context.Categorie
                .FirstOrDefaultAsync(m => m.id == id);
            if (categorie == null)
            {
                return NotFound();
            }

            return View(categorie);
        }

        // GET: Categorie/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categorie/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,nom")] Categorie categorie)
        {
            if (ModelState.IsValid)
            {
                _context.Add(categorie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(categorie);
        }


        // GET: Categorie/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categorie = await _context.Categorie.FindAsync(id);
            if (categorie == null)
            {
                return NotFound();
            }

            return View(categorie);
        }

        // POST: Categorie/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,nom")] Categorie categorie)
        {
            if (id != categorie.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(categorie);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategorieExists(categorie.id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(categorie);
        }


        // GET: Categorie/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categorie = await _context.Categorie
                .FirstOrDefaultAsync(m => m.id == id);
            if (categorie == null)
            {
                return NotFound();
            }

            return View(categorie);
        }

        // POST: Categorie/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var categorie = await _context.Categorie.FindAsync(id);
            if (categorie != null)
            {
                _context.Categorie.Remove(categorie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategorieExists(int id)
        {
            return _context.Categorie.Any(e => e.id == id);
        }

        // GET: Categorie/Import
        public IActionResult Import()
        {
            return View();
        }

        // POST: Categorie/ImportCSV
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportCSV(IFormFile csvFile)
        {
            if (csvFile != null && csvFile.Length > 0)
            {
                using (var reader = new StreamReader(csvFile.OpenReadStream()))
                {
                    var line = await reader.ReadLineAsync();
                    while (line != null)
                    {
                        var values = line.Split(',');

                        if (values.Length == 2) // Assuming CSV format: id, nom
                        {
                            var categorie = new Categorie
                            {
                                id = int.Parse(values[0], CultureInfo.InvariantCulture),
                                nom = values[1]
                            };
                            _context.Categorie.Add(categorie);
                        }

                        line = await reader.ReadLineAsync();
                    }

                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Veuillez sélectionner un fichier CSV.");
            return View("Import");
        }
    }
}
