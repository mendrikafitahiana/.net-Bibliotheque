using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Bibtheque.Models;
using Bibtheque.Models.Context;

namespace Bibtheque.Controllers
{
    public class LivreController : Controller
    {
        private readonly BibthequeContext _context;

        public LivreController(BibthequeContext context)
        {
            _context = context;
        }

        // GET: Livre
        public async Task<IActionResult> Index()
        {
            var livres = _context.Livre.Include(l => l.Categorie);
            return View(await livres.ToListAsync());
        }

        // GET: Livre/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var livre = await _context.Livre
                .Include(l => l.Categorie)
                .FirstOrDefaultAsync(m => m.id == id);
            if (livre == null)
            {
                return NotFound();
            }

            return View(livre);
        }

        // GET: Livre/Create
        public IActionResult Create()
        {
            ViewData["CategorieId"] = new SelectList(_context.Categorie, "id", "nom");
            return View();
        }

        // POST: Livre/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,titre,auteur,resume,image,nbPage,prix,dateEdition,CategorieId")] Livre livre)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    livre.Categorie = await _context.Categorie.FindAsync(livre.CategorieId);
                    _context.Add(livre);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Log l'exception
                    Console.WriteLine($"Erreur lors de la création du livre : {ex.Message}");
                    ModelState.AddModelError("", "Une erreur est survenue lors de la création du livre.");
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine($"Erreur de validation : {error.ErrorMessage}");
                }
            }
            ViewData["CategorieId"] = new SelectList(_context.Categorie, "id", "nom", livre.CategorieId);
            return View(livre);
        }

        // GET: Livre/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var livre = await _context.Livre.FindAsync(id);
            if (livre == null)
            {
                return NotFound();
            }
            ViewData["CategorieId"] = new SelectList(_context.Categorie, "id", "nom", livre.CategorieId);
            return View(livre);
        }

        // POST: Livre/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,titre,auteur,resume,image,nbPage,prix,dateEdition,CategorieId")] Livre livre)
        {
            if (id != livre.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(livre);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LivreExists(livre.id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategorieId"] = new SelectList(_context.Categorie, "id", "nom", livre.CategorieId);
            return View(livre);
        }

        // GET: Livre/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var livre = await _context.Livre
                .Include(l => l.Categorie)
                .FirstOrDefaultAsync(m => m.id == id);
            if (livre == null)
            {
                return NotFound();
            }

            return View(livre);
        }

        // POST: Livre/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var livre = await _context.Livre.FindAsync(id);
            if (livre != null)
            {
                _context.Livre.Remove(livre);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LivreExists(int id)
        {
            return _context.Livre.Any(e => e.id == id);
        }
    }
}
