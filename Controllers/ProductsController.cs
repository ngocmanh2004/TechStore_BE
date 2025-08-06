﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using TechStore_BE.DataConnection;
using TechStore_BE.Models;

namespace TechStore_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductsController(ApplicationDBContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet]
        public async Task<ActionResult> GetProducts()
        {
            var products = await _context.Products
                .Select(p => new {
                    p.product_id,
                    p.product_name,
                    p.brand_id,
                    p.category_id,
                    p.price,
                    p.quantity,
                    p.image_url,
                    p.description,
                    brand_name = p.Brand.brand_name,
                    category_name = p.Category.category_name    
                }).ToListAsync();

            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetProduct(int id)
        {
            var product = await _context.Products
                .Where(p => p.product_id == id)
                .Select(p => new {
                    p.product_id,
                    p.product_name,
                    p.brand_id,
                    p.category_id,
                    p.price,
                    p.quantity,
                    p.image_url,
                    p.description,
                    brand_name = p.Brand.brand_name,
                    category_name = p.Category.category_name
                }).FirstOrDefaultAsync();

            if (product == null) return NotFound(new { Message = "Không tìm thấy sản phẩm." });
            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult> PostProduct([FromBody] Products product)
        {
            if (product == null || string.IsNullOrWhiteSpace(product.product_name) || product.price <= 0)
                return BadRequest(new { Message = "Thông tin sản phẩm không hợp lệ." });

            var newProduct = new Products
            {
                product_name = product.product_name,
                brand_id = product.brand_id,
                category_id = product.category_id,
                price = product.price,
                quantity = product.quantity,
                description = product.description,
                image_url = product.image_url
            };

            _context.Products.Add(newProduct);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Thêm sản phẩm thành công!", product_id = newProduct.product_id });
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, [FromBody] Products product)
        {
            if (id != product.product_id)
                return BadRequest(new { Message = "ID không khớp." });

            var existing = await _context.Products.FindAsync(id);
            if (existing == null) return NotFound(new { Message = "Không tìm thấy sản phẩm." });

            existing.product_name = product.product_name;
            existing.brand_id = product.brand_id;
            existing.category_id = product.category_id;
            existing.price = product.price;
            existing.quantity = product.quantity;
            existing.description = product.description;
            existing.image_url = product.image_url;

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Cập nhật thành công!" });
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound(new { Message = "Không tìm thấy sản phẩm." });

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Xóa thành công!" });
        }

        [HttpGet("ByCategory/{categoryId}")]
        public async Task<IActionResult> GetProductsByCategory(int categoryId)
        {
            var products = await _context.Products
                .Where(p => p.category_id == categoryId)
                .Select(p => new {
                    p.product_id,
                    p.product_name,
                    p.price,
                    p.image_url
                }).ToListAsync();

            if (!products.Any()) return NotFound(new { Message = "Không có sản phẩm nào trong danh mục này." });
            return Ok(products);
        }

        [HttpGet("ByBrand/{brandId}")]
        public async Task<IActionResult> GetProductsByBrand(int brandId)
        {
            var products = await _context.Products
                .Where(p => p.brand_id == brandId)
                .Select(p => new {
                    p.product_id,
                    p.product_name,
                    p.price,
                    p.image_url
                }).ToListAsync();

            if (!products.Any()) return NotFound(new { Message = "Không có sản phẩm nào cho thương hiệu này." });
            return Ok(products);
        }

        [HttpGet("categories/{category_id}/brands/{brand_id}")]
        public async Task<IActionResult> GetProductsByCategoryAndBrand(int category_id, int brand_id)
        {
            var products = await _context.Products
                .Where(p => p.category_id == category_id && p.brand_id == brand_id)
                .Select(p => new {
                    p.product_id,
                    p.product_name,
                    p.price,
                    p.image_url
                }).ToListAsync();

            return Ok(products);
        }

        [HttpGet("by-ids")]
        public async Task<IActionResult> GetProductsByIds([FromQuery] string ids)
        {
            if (string.IsNullOrWhiteSpace(ids)) return BadRequest(new { Message = "Thiếu danh sách ID." });

            try
            {
                var idList = ids.Split(',').Select(int.Parse).ToList();
                var products = await _context.Products
                    .Where(p => idList.Contains(p.product_id))
                    .Select(p => new {
                        p.product_id,
                        p.product_name,
                        p.price,
                        p.image_url
                    }).ToListAsync();

                if (!products.Any()) return NotFound(new { Message = "Không tìm thấy sản phẩm nào phù hợp." });
                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi server", Error = ex.Message });
            }
        }

        [HttpPost("SaveFile")]
        public JsonResult SaveFile()
        {
            try
            {
                var file = Request.Form.Files[0];
                var fileName = Path.GetFileName(file.FileName);
                var savePath = Path.Combine(_env.WebRootPath, "images", fileName);

                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                // Trả JSON object rõ ràng để JS đọc được
                return new JsonResult(new { fileName = fileName });
            }
            catch
            {
                return new JsonResult(new { fileName = "default.jpg" });
            }
        }


        [HttpGet("categories/{category_id}/ByPriceCategory/{priceCategory}")]
        public async Task<IActionResult> GetProductsByPriceCategory(int category_id, string priceCategory)
        {
            var products = _context.Products.Where(p => p.category_id == category_id);

            switch (priceCategory.ToLower())
            {
                case "low":
                    products = products.Where(p => p.price < 1000000); break;
                case "medium":
                    products = products.Where(p => p.price >= 1000000 && p.price < 5000000); break;
                case "high":
                    products = products.Where(p => p.price >= 5000000 && p.price < 10000000); break;
                case "premium":
                    products = products.Where(p => p.price >= 10000000); break;
                default:
                    return BadRequest(new { Message = "Giá trị priceCategory không hợp lệ." });
            }

            var result = await products.Select(p => new {
                p.product_id,
                p.product_name,
                p.price,
                p.image_url
            }).ToListAsync();

            if (!result.Any()) return NotFound(new { Message = "Không có sản phẩm phù hợp." });
            return Ok(result);
        }

        [HttpGet("categories/{category_id}/brands/{brand_id}/ByPriceCategory/{priceCategory}")]
        public async Task<IActionResult> GetProductsByCategoryBrandAndPrice(int category_id, int brand_id, string priceCategory)
        {
            var products = _context.Products.Where(p => p.category_id == category_id && p.brand_id == brand_id);

            switch (priceCategory.ToLower())
            {
                case "low":
                    products = products.Where(p => p.price < 1000000);
                    break;
                case "medium":
                    products = products.Where(p => p.price >= 1000000 && p.price < 5000000);
                    break;
                case "high":
                    products = products.Where(p => p.price >= 5000000 && p.price < 10000000);
                    break;
                case "premium":
                    products = products.Where(p => p.price >= 10000000);
                    break;
                default:
                    return BadRequest(new { Message = "Giá trị priceCategory không hợp lệ." });
            }

            var result = await products.Select(p => new {
                p.product_id,
                p.product_name,
                p.price,
                p.image_url
            }).ToListAsync();

            if (!result.Any()) return NotFound(new { Message = "Không có sản phẩm phù hợp." });
            return Ok(result);
        }


        [HttpGet("search/{searchText}")]
        public async Task<IActionResult> SearchProducts(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return BadRequest(new { Message = "Từ khóa không được để trống." });

            var products = await _context.Products
                .Where(p => EF.Functions.Like(p.product_name!, $"%{searchText}%"))
                .Select(p => new {
                    p.product_id,
                    p.product_name,
                    p.price,
                    p.image_url
                }).ToListAsync();

            if (!products.Any()) return NotFound(new { Message = "Không có kết quả phù hợp." });
            return Ok(products);
        }

        private bool ProductExists(int id) =>
            _context.Products.Any(p => p.product_id == id);
    }
}
