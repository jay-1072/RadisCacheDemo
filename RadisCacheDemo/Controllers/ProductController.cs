using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RadisCacheDemo.Cache;
using RadisCacheDemo.Model;

namespace RadisCacheDemo.Controllers
{
    public class ProductController: ControllerBase
    {
        private readonly DatabaseContext _dbContext;
        private readonly ICacheService _cacheService;
    
        public ProductController(DatabaseContext dbContext, ICacheService cacheService)
        {
            _dbContext = dbContext;
            _cacheService = cacheService;
        }

        [HttpPost("addproduct")]
        public async Task<Product> AddProduct(Product product)
        {
            var obj = await _dbContext.Products.AddAsync(product);
            await _cacheService.RemoveData("product");
            await _dbContext.SaveChangesAsync();

            return obj.Entity;
        }

        [HttpPut("updateproduct")]
        public async Task UpdateProduct(Product product)
        {
            _dbContext.Products.Update(product);
            await _cacheService.RemoveData("product");
            await _dbContext.SaveChangesAsync();
        }

        [HttpDelete("deleteproduct")]
        public async Task DeleteProduct(int Id)
        {
            var product = _dbContext.Products.Where(x => x.Id == Id).FirstOrDefault();

            if (product != null)
            {
                _dbContext.Remove(product);
                await _cacheService.RemoveData("product");
                await _dbContext.SaveChangesAsync();
            }
        }

        [HttpGet("product")]
        public async Task<Product> GetProductById(int productId)
        {
            var cachedProducts = await _cacheService.GetData<IEnumerable<Product>>("product");

            if (!cachedProducts.IsNullOrEmpty())
            {
                return cachedProducts.Where(x => x.Id == productId).FirstOrDefault();
            }
            
            return await _dbContext.Products.Where(x => x.Id == productId).FirstOrDefaultAsync();
        }

        [HttpGet("products")]
        public async Task<IEnumerable<Product>> GetProducts()
        {
            var cachedProducts = await _cacheService.GetData<IEnumerable<Product>>("product");

            if (!cachedProducts.IsNullOrEmpty())
            {
                return cachedProducts;
            }

            var expirationTime = DateTimeOffset.UtcNow.AddMinutes(2);

            cachedProducts = _dbContext.Products.ToList();

            await _cacheService.SetData("product", cachedProducts, expirationTime);

            return cachedProducts;
        }
    }
}