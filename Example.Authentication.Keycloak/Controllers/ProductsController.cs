using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Example.Authentication.Keycloak.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ProductsController : ControllerBase
{
  
    [HttpGet]
    public IActionResult GetAllProducts()
    {
     
        return Ok("Success");
    }

    [HttpGet("{id}")]
    public IActionResult GetProductById(int id)
    {

        return Ok("Success");
    }

    [HttpPost]
    public IActionResult AddProduct()
    {
        return Ok("Success");
    }

    [HttpPut("{id}")]
    public IActionResult UpdateProduct(int id)
    {
        return Ok("Success");
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteProduct(int id)
    {
        return Ok("Success");
    }
}