﻿using API_Network.Context;
using API_Network.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_Network.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class CategoriesController : Controller
    {
        private readonly DbContextNetwork _context;

        public CategoriesController(DbContextNetwork wcContext)
        {
            _context = wcContext;
        }

        //[Authorize]
        [HttpGet("Listado")]
        public async Task<List<Category>> Listado()
        {
            var list = await _context.Categories.ToListAsync();

            if (list == null)
            {
                return new List<Category>();
            }
            else
            {
                return list;
            }//end if/else
        }//end Listado

        //[Authorize]
        [HttpGet("Consultar")]
        public async Task<Category> Consultar(int categoryId)
        {
            var temp = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == categoryId);

            return temp;
        }//end consultar 

        //[Authorize]
        [HttpPost("Agregar")]
        public string Agregar(Category category)
        {
            string msj = "";

            //verifica si ya hay un Categorie con los mismos datos
            bool categorie = _context.Categories.Any(c => c.CategoryId == category.CategoryId);

            try
            {
                if (!categorie)
                {
                    _context.Categories.Add(category);
                    _context.SaveChanges();
                    msj = "Categoria registrada correctamente";
                }
                else
                {
                    msj = "Esos datos ya existen o son incorrectos";
                }//end else
            }
            catch (Exception ex)
            {
                msj = $"Error: {ex.Message} {ex.InnerException.ToString()}";
            }//end

            return msj;
        }//end Agregar

        //[Authorize]
        [HttpPut("Editar")]
        public string Editar(Category category)
        {
            string msj = "";

            //verifica si ya hay una category con los mismos datos
            bool categoryExist = _context.Categories.Any(c => c.CategoryName == category.CategoryName);

            try
            {
                if (!categoryExist)
                {
                    _context.Categories.Update(category);
                    _context.SaveChanges();
                    msj = "Categoria editada correctamente";
                }
                else
                {
                    msj = "Esos datos ya existen o son incorrectos";
                }//end else
            }
            catch (Exception ex)
            {
                msj = $"Error: {ex.Message} {ex.InnerException.ToString()}";
            }//end

            return msj;
        }//end Editar

        //[Authorize]
        [HttpDelete("Eliminar")]
        public async Task<string> Eliminar(int categoryId)
        {
            string msj = "";

            try
            {
                var data = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == categoryId);

                //Hacer este procedimiento comentado pero con la nueva tabla WorkCategories
                // //se busca en la tabla workProfile si hay alguno con esta categoria
                // var listWorkProfile = await _context.WorkProfiles.Where(wp => wp.CategoryId == categoryId).ToListAsync();


                // //Se cambian los workProfile por la categoria con id 14 = Sin categoria
                // foreach (var wp in listWorkProfile)
                // {
                //     wp.CategoryId = 14;
                //     _context.WorkProfiles.Update(wp);
                //     _context.SaveChanges();

                // }//end foreach

                _context.Categories.Remove(data);
                _context.SaveChanges();
                msj = $"La Categoria: {data.CategoryName}, ha sido eliminada correctamente";
            }
            catch (Exception ex)
            {
                msj = $"Error: {ex.Message} {ex.InnerException.ToString()}";
            }
            return msj;
        }//end Eliminar

    }
}