using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Example.Authentication.Keycloak.Controllers;

[Authorize]
[Route("/user")]
public class UserController : ControllerBase
{

    [HttpGet("/get-user")]
    public string GetUser()
    {

        return "Success";
    }
    [HttpGet("/create-user")]
    public string Createuser()
    {

        return "Success";
    }
    [HttpGet("/update-user")]
    public string UpdateUser()
    {

        return "Success";
    }
    [HttpGet("/delete-user")]
    public string DeleteUser()
    {

        return "Success";
    }
}